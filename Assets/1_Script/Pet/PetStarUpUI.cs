using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;

public class PetStarUpUI : MonoBehaviour
{
    public static PetStarUpUI Instance { get; private set; }

    [Header("Main Panel")]
    [SerializeField] private GameObject panel;

    [Header("Star Images (Max 5)")]
    [SerializeField] private List<Image> currentStars; // Kéo 5 ảnh sao hiện tại vào đây
    [SerializeField] private List<Image> nextStars;    // Kéo 5 ảnh sao mục tiêu vào đây
    
    [Header("Star Sprites (Awakening)")]
    [SerializeField] private Sprite inactiveStarSprite;// Sao tối (Chưa đạt)
    [SerializeField] private Sprite starTier1Sprite;   // Sao bật 1 (1-5 sao, VD: Vàng)
    [SerializeField] private Sprite starTier2Sprite;   // Sao bật 2 (6-10 sao, VD: Đỏ hoặc Cầu vồng)
    [SerializeField] private Sprite starTier3Sprite;   // Sao bật 3 (11-15 sao, VD: Tím hoặc Kim cương)
    [SerializeField] private Sprite starTier4Sprite;   // Sao bật 4 (16-20 sao, VD: Vàng kim hoặc Huyền thoại)


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
    [SerializeField] private RequirementItemUI reqItemPrefab; // Kéo Prefab có gắn script RequirementItemUI vào đây
    [SerializeField] private Button starUpBtn;

    private ProgressionNode currentStarNode;

    private class EnableTracker : MonoBehaviour
    {
        public System.Action onEnableEvent;
        private void OnEnable() { if (onEnableEvent != null) onEnableEvent.Invoke(); }
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        starUpBtn.onClick.AddListener(OnStarUpClicked);
        
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
            // --- 1. Cập nhật Dãy Sao ---
            DrawStars(currentStars, pet.star);
            
            currentStarNode = baseInfo.progressionTable.GetStarCost(pet.star);
            
            // --- 2. Cập nhật Chỉ số Hiện Tại ---
            PetFinalStats curStats = PetStatsCalculator.GetFinalStats(pet, baseInfo);
            curHpTxt.text = curStats.HP.ToString();
            curAtkPhyTxt.text = curStats.AtkPhy.ToString();
            curAtkMagTxt.text = curStats.AtkMag.ToString();
            curDefPhyTxt.text = curStats.DefPhy.ToString();
            curDefMagTxt.text = curStats.DefMag.ToString();
            curSpeedTxt.text = curStats.Speed.ToString();

            if (currentStarNode != null)
            {
                // Có thể nâng cấp
                DrawStars(nextStars, pet.star + 1);
                
                // Tự tạo một Pet ảo có số sao +1 để đưa vào máy tính (Calculator) mượn xem trước chỉ số
                PetModel fakeNextPet = new PetModel { 
                    level = pet.level,
                    star = pet.star + 1,
                    realm = pet.realm 
                };
                PetFinalStats nextStats = PetStatsCalculator.GetFinalStats(fakeNextPet, baseInfo);
                
                nextHpTxt.text = nextStats.HP.ToString();
                nextAtkPhyTxt.text = nextStats.AtkPhy.ToString();
                nextAtkMagTxt.text = nextStats.AtkMag.ToString();
                nextDefPhyTxt.text = nextStats.DefPhy.ToString();
                nextDefMagTxt.text = nextStats.DefMag.ToString();
                nextSpeedTxt.text = nextStats.Speed.ToString();

                // --- 3. Cập nhật ô Nguyên Liệu ---
                await RenderRequirements(currentStarNode, itemsContainer, starUpBtn, currentRefresh);
            }
            else
            {
                // Đã Đạt MAX Sao
                DrawStars(nextStars, pet.star); 
                nextHpTxt.text = "MAX";
                nextAtkPhyTxt.text = "MAX";
                nextAtkMagTxt.text = "MAX";
                nextDefPhyTxt.text = "MAX";
                nextDefMagTxt.text = "MAX";
                nextSpeedTxt.text = "MAX";
                starUpBtn.interactable = false;
                ClearContainer(itemsContainer);
            }
        }
        finally
        {
            if (LoadingUI.Instance != null) LoadingUI.Instance.Hide();
        }
    }

    private void DrawStars(List<Image> starImages, int totalStars)
    {
        for (int i = 0; i < starImages.Count; i++)
        {
            if (i >= 5) break; // Chỉ hỗ trợ tối đa 5 slot ảnh hiển thị

            if (totalStars <= 5)
            {
                // Bật 1: Từ 1 đến 5 sao
                if (i < totalStars) starImages[i].sprite = starTier1Sprite;
                else starImages[i].sprite = inactiveStarSprite;
            }
            else if (totalStars <= 10)
            {
                // Bật 2: Sao thứ 6 sẽ lấy 1 sao Bật 2 ĐÈ LÊN sao Bật 1
                int tier2Count = totalStars - 5;
                if (i < tier2Count) starImages[i].sprite = starTier2Sprite;
                else starImages[i].sprite = starTier1Sprite;
            }
            else if (totalStars <= 15)
            {
                // Bật 3: Từ 11 đến 15 sao, sao Bật 3 sẽ ĐÈ LÊN sao Bật 2
                int tier3Count = totalStars - 10;
                if (i < tier3Count) starImages[i].sprite = starTier3Sprite;
                else starImages[i].sprite = starTier2Sprite;
            }
            else
            {
                // Bật 4: Từ 16 đến 20 sao, sao Bật 4 sẽ ĐÈ LÊN sao Bật 3
                int tier4Count = totalStars - 15;
                if (i < tier4Count) starImages[i].sprite = starTier4Sprite;
                else starImages[i].sprite = starTier3Sprite;
            }
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

        upgradeBtn.interactable = canUpgrade;
    }

    private void ClearContainer(Transform container)
    {
        foreach (Transform child in container) Destroy(child.gameObject);
    }

    private async void OnStarUpClicked()
    {
        var pet = PetManager.Instance.CurrentPet;
        if (pet != null && currentStarNode != null)
        {
            starUpBtn.interactable = false; // Tránh spam click
            await InventoryManager.Instance.PetStarUp(pet.id, currentStarNode);
        }
    }
}
