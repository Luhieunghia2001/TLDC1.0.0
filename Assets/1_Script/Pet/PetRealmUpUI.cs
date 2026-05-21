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

    private class EnableTracker : MonoBehaviour
    {
        public System.Action onEnableEvent;
        private void OnEnable() { if (onEnableEvent != null) onEnableEvent.Invoke(); }
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        realmUpBtn.onClick.AddListener(OnRealmUpClicked);
        
        if (panel != null)
        {
            var tracker = panel.AddComponent<EnableTracker>();
            tracker.onEnableEvent += OnPanelOpened;
            if (panel.activeInHierarchy) OnPanelOpened();
        }

        PetManager.Instance.OnPetSelected += OnPetChanged;
        PetManager.Instance.OnPetStatsUpdated += OnPetChanged;
    }

    private void OnPanelOpened()
    {
        _ = RefreshUI();
    }

    private void OnDestroy()
    {
        if (PetManager.Instance != null)
        {
            PetManager.Instance.OnPetSelected -= OnPetChanged;
            PetManager.Instance.OnPetStatsUpdated -= OnPetChanged;
        }
    }

    private void OnPetChanged(PetModel pet)
    {
        if (panel != null && panel.activeInHierarchy) _ = RefreshUI();
    }

    private int refreshId = 0;

    public async Task RefreshUI()
    {
        int currentRefresh = ++refreshId;

        var pet = PetManager.Instance.CurrentPet;
        if (pet == null) return;

        var baseInfo = PetManager.Instance.GetPetBaseByID(pet.petBaseId);
        if (baseInfo == null || baseInfo.progressionTable == null) return;

        if (LoadingUI.Instance != null) LoadingUI.Instance.Show();
        try
        {
            // --- 1. Hiển thị thông tin Tầng ---
            currentRealmTxt.text = "Tầng " + pet.realm;
            currentRealmNode = baseInfo.progressionTable.GetRealmCost(pet.realm);
            
            // --- 2. Cập nhật Chỉ số Hiện Tại ---
            var curStats = await PetManager.Instance.GetPetFinalStatsFromServer(pet.id);
            if (currentRefresh != refreshId) return;
            SetStatsText(curStats, curHpTxt, curAtkPhyTxt, curAtkMagTxt, curDefPhyTxt, curDefMagTxt, curSpeedTxt);

            if (currentRealmNode != null)
            {
                nextRealmTxt.text = "Tầng " + (pet.realm + 1);
                
                var nextStats = await PetManager.Instance.GetPetFinalStatsPreviewFromServer(pet.id, pet.level, pet.star, pet.realm + 1);
                if (currentRefresh != refreshId) return;
                
                SetStatsText(nextStats, nextHpTxt, nextAtkPhyTxt, nextAtkMagTxt, nextDefPhyTxt, nextDefMagTxt, nextSpeedTxt);

                // --- 3. Cập nhật ô Nguyên Liệu ---
                await RenderRequirements(currentRealmNode, itemsContainer, realmUpBtn, currentRefresh);
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
        finally
        {
            if (LoadingUI.Instance != null) LoadingUI.Instance.Hide();
        }
    }

    private async Task RenderRequirements(ProgressionNode node, Transform container, Button upgradeBtn, int currentRefresh)
    {
        var inventory = await InventoryManager.Instance.GetMyInventory();

        if (currentRefresh != refreshId) return;

        ClearContainer(container);
        
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

    private void SetStatsText(PetServerStats stats, TextMeshProUGUI hp, TextMeshProUGUI atkPhy, TextMeshProUGUI atkMag, TextMeshProUGUI defPhy, TextMeshProUGUI defMag, TextMeshProUGUI speed)
    {
        if (stats == null) return;
        hp.text = stats.hp.ToString();
        atkPhy.text = stats.atk_phy.ToString();
        atkMag.text = stats.atk_mag.ToString();
        defPhy.text = stats.def_phy.ToString();
        defMag.text = stats.def_mag.ToString();
        speed.text = stats.speed.ToString();
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
