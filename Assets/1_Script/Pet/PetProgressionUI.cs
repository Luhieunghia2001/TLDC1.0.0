using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;

public class PetProgressionUI : MonoBehaviour
{
    public static PetProgressionUI Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject panel; // Kéo Panel chứa Giao diện Thăng Sao/Thăng Tầng vào đây

    [Header("Star Up UI")]
    public TextMeshProUGUI currentStarTxt;
    public TextMeshProUGUI nextStarTxt;
    public Transform starItemsContainer; // Nơi chứa các ô nguyên liệu
    public GameObject reqItemPrefab; // Prefab UI hiển thị nguyên liệu (Có Image "Icon" và Text "QtyTxt")
    public Button starUpBtn;

    [Header("Realm Up UI")]
    public TextMeshProUGUI currentRealmTxt;
    public TextMeshProUGUI nextRealmTxt;
    public Transform realmItemsContainer;
    public Button realmUpBtn;

    private ProgressionNode currentStarNode;
    private ProgressionNode currentRealmNode;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        
        starUpBtn.onClick.AddListener(OnStarUpClicked);
        realmUpBtn.onClick.AddListener(OnRealmUpClicked);
        
        if (panel != null)
        {
            var tracker = panel.AddComponent<EnableTracker>();
            tracker.onEnableEvent += () => _ = RefreshUI();
        }
    }

    private class EnableTracker : MonoBehaviour
    {
        public System.Action onEnableEvent;
        private void OnEnable() { if (onEnableEvent != null) onEnableEvent.Invoke(); }
    }

    private void Start()
    {
        // Tự động làm mới UI nếu người chơi đang mở Panel này và có thao tác đổi Pet / Lên cấp
        PetManager.Instance.OnPetSelected += (p) => { if (panel != null && panel.activeInHierarchy) _ = RefreshUI(); };
        PetManager.Instance.OnPetStatsUpdated += (p) => { if (panel != null && panel.activeInHierarchy) _ = RefreshUI(); };
    }

    private int refreshId = 0;

    public async Task RefreshUI()
    {
        int currentRefresh = ++refreshId;

        var pet = PetManager.Instance.CurrentPet;
        if (pet == null) return;

        var baseInfo = PetManager.Instance.GetPetBaseByID(pet.petBaseId);
        if (baseInfo == null || baseInfo.progressionTable == null) return;

        // --- Cập nhật giao diện THĂNG SAO ---
        currentStarTxt.text = pet.star.ToString() + " Sao";
        currentStarNode = baseInfo.progressionTable.GetStarCost(pet.star);
        
        if (currentStarNode != null)
        {
            nextStarTxt.text = (pet.star + 1).ToString() + " Sao";
            await RenderRequirements(currentStarNode, starItemsContainer, starUpBtn, currentRefresh);
        }
        else
        {
            nextStarTxt.text = "MAX";
            starUpBtn.interactable = false;
            ClearContainer(starItemsContainer);
        }

        // Kiểm tra lại sau khi await RenderRequirements (Star)
        if (currentRefresh != refreshId) return;

        // --- Cập nhật giao diện THĂNG TẦNG ---
        currentRealmTxt.text = "Tầng " + pet.realm.ToString();
        currentRealmNode = baseInfo.progressionTable.GetRealmCost(pet.realm);

        if (currentRealmNode != null)
        {
            nextRealmTxt.text = "Tầng " + (pet.realm + 1).ToString();
            await RenderRequirements(currentRealmNode, realmItemsContainer, realmUpBtn, currentRefresh);
        }
        else
        {
            nextRealmTxt.text = "MAX";
            realmUpBtn.interactable = false;
            ClearContainer(realmItemsContainer);
        }
    }

    private async Task RenderRequirements(ProgressionNode node, Transform container, Button upgradeBtn, int currentRefresh)
    {
        var inventory = await InventoryManager.Instance.GetMyInventory();
        
        // Nếu có yêu cầu mới hơn thì dừng lại
        if (currentRefresh != refreshId) return;

        ClearContainer(container);

        bool canUpgrade = true;

        foreach (var req in node.requiredItems)
        {
            // Sinh ra Prefab nguyên liệu
            var go = Instantiate(reqItemPrefab, container);
            if (go.TryGetComponent<RewardItemUI>(out var reqUI))
            {
                var invItem = inventory.Find(x => x.itemId == req.item.itemID);
                int currentQty = invItem != null ? invItem.quantity : 0;
                
                Color textColor = currentQty < req.quantity ? Color.red : Color.green;
                if (currentQty < req.quantity) canUpgrade = false;

                reqUI.Setup(req.item.icon, $"{currentQty}/{req.quantity}", textColor);
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
            await InventoryManager.Instance.PetStarUp(pet.id, currentStarNode);
        }
    }

    private async void OnRealmUpClicked()
    {
        var pet = PetManager.Instance.CurrentPet;
        if (pet != null && currentRealmNode != null)
        {
            await InventoryManager.Instance.PetRealmUp(pet.id, currentRealmNode);
        }
    }
}
