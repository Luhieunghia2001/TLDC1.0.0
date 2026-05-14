using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;

public class PetRealmUpUI : MonoBehaviour
{
    public static PetRealmUpUI Instance { get; private set; }

    [Header("Main Panel")]
    [SerializeField] private GameObject panel;

    [Header("Realm Text")]
    [SerializeField] private TextMeshProUGUI currentRealmTxt;
    [SerializeField] private TextMeshProUGUI nextRealmTxt;

    [Header("Current Stats")]
    [SerializeField] private TextMeshProUGUI curHpTxt;
    [SerializeField] private TextMeshProUGUI curAtkPhyTxt;
    [SerializeField] private TextMeshProUGUI curAtkMagTxt;
    [SerializeField] private TextMeshProUGUI curDefPhyTxt;
    [SerializeField] private TextMeshProUGUI curDefMagTxt;
    [SerializeField] private TextMeshProUGUI curSpeedTxt;

    [Header("Next Stats (Preview)")]
    [SerializeField] private TextMeshProUGUI nextHpTxt;
    [SerializeField] private TextMeshProUGUI nextAtkPhyTxt;
    [SerializeField] private TextMeshProUGUI nextAtkMagTxt;
    [SerializeField] private TextMeshProUGUI nextDefPhyTxt;
    [SerializeField] private TextMeshProUGUI nextDefMagTxt;
    [SerializeField] private TextMeshProUGUI nextSpeedTxt;

    [Header("Requirements & Action")]
    [SerializeField] private Transform itemsContainer;
    [SerializeField] private RequirementItemUI reqItemPrefab; // Tái sử dụng Prefab của Thăng Sao
    [SerializeField] private Button realmUpBtn;

    private ProgressionNode currentRealmNode;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        realmUpBtn.onClick.AddListener(OnRealmUpClicked);
        
        // Đăng ký sự kiện ngay trong Awake để không bỏ lỡ dữ liệu khi vừa SetActive
        PetManager.Instance.OnPetSelected += OnPetChanged;
        PetManager.Instance.OnPetStatsUpdated += OnPetChanged;
    }

    private void OnDestroy()
    {
        if (PetManager.Instance != null)
        {
            PetManager.Instance.OnPetSelected -= OnPetChanged;
            PetManager.Instance.OnPetStatsUpdated -= OnPetChanged;
        }
    }

    private void OnEnable()
    {
        _ = RefreshUI();
    }

    private void OnPetChanged(PetModel pet)
    {
        if (panel != null && panel.activeInHierarchy) _ = RefreshUI();
    }

    public async Task RefreshUI()
    {
        var pet = PetManager.Instance.CurrentPet;
        if (pet == null) return;

        var baseInfo = PetManager.Instance.GetPetBaseByID(pet.petBaseId);
        if (baseInfo == null || baseInfo.progressionTable == null) return;

        // --- 1. Hiển thị thông tin Tầng ---
        currentRealmTxt.text = "Tầng " + pet.realm;
        currentRealmNode = baseInfo.progressionTable.GetRealmCost(pet.realm);
        
        // --- 2. Cập nhật Chỉ số Hiện Tại ---
        PetFinalStats curStats = PetStatsCalculator.GetFinalStats(pet, baseInfo);
        curHpTxt.text = curStats.HP.ToString();
        curAtkPhyTxt.text = curStats.AtkPhy.ToString();
        curAtkMagTxt.text = curStats.AtkMag.ToString();
        curDefPhyTxt.text = curStats.DefPhy.ToString();
        curDefMagTxt.text = curStats.DefMag.ToString();
        curSpeedTxt.text = curStats.Speed.ToString();

        if (currentRealmNode != null)
        {
            nextRealmTxt.text = "Tầng " + (pet.realm + 1);
            
            // Giả lập PetModel mới với Tầng + 1 để tính xem trước chỉ số
            PetModel fakeNextPet = new PetModel { 
                id = pet.id,
                petBaseId = pet.petBaseId,
                level = pet.level,
                star = pet.star,
                realm = pet.realm + 1 // CỘNG 1 TẦNG
            };
            PetFinalStats nextStats = PetStatsCalculator.GetFinalStats(fakeNextPet, baseInfo);
            
            nextHpTxt.text = nextStats.HP.ToString();
            nextAtkPhyTxt.text = nextStats.AtkPhy.ToString();
            nextAtkMagTxt.text = nextStats.AtkMag.ToString();
            nextDefPhyTxt.text = nextStats.DefPhy.ToString();
            nextDefMagTxt.text = nextStats.DefMag.ToString();
            nextSpeedTxt.text = nextStats.Speed.ToString();

            // --- 3. Cập nhật ô Nguyên Liệu ---
            await RenderRequirements(currentRealmNode, itemsContainer, realmUpBtn);
        }
        else
        {
            // Đã Đạt MAX Tầng
            nextRealmTxt.text = "MAX";
            nextHpTxt.text = "MAX";
            nextAtkPhyTxt.text = "MAX";
            nextAtkMagTxt.text = "MAX";
            nextDefPhyTxt.text = "MAX";
            nextDefMagTxt.text = "MAX";
            nextSpeedTxt.text = "MAX";
            realmUpBtn.interactable = false;
            ClearContainer(itemsContainer);
        }
    }

    private async Task RenderRequirements(ProgressionNode node, Transform container, Button upgradeBtn)
    {
        ClearContainer(container);

        var inventory = await InventoryManager.Instance.GetMyInventory();
        bool canUpgrade = true;

        foreach (var req in node.requiredItems)
        {
            var reqItem = Instantiate(reqItemPrefab, container);

            var invItem = inventory.Find(x => x.itemId == req.item.itemID);
            int currentQty = invItem != null ? invItem.quantity : 0;

            reqItem.Setup(req.item.icon, currentQty, req.quantity);
            
            if (currentQty < req.quantity)
            {
                canUpgrade = false;
            }
        }

        // Tùy chọn: Có thể kiểm tra thêm điều kiện Vàng (goldCost) ở đây nếu game bạn xài Vàng

        upgradeBtn.interactable = canUpgrade;
    }

    private void ClearContainer(Transform container)
    {
        foreach (Transform child in container) Destroy(child.gameObject);
    }

    private async void OnRealmUpClicked()
    {
        var pet = PetManager.Instance.CurrentPet;
        if (pet != null && currentRealmNode != null)
        {
            realmUpBtn.interactable = false; // Tránh spam click
            await InventoryManager.Instance.PetRealmUp(pet.id, currentRealmNode);
        }
    }
}
