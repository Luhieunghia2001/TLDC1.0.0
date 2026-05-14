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

    public async Task RefreshUI()
    {
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
            await RenderRequirements(currentStarNode, starItemsContainer, starUpBtn);
        }
        else
        {
            nextStarTxt.text = "MAX";
            starUpBtn.interactable = false;
            ClearContainer(starItemsContainer);
        }

        // --- Cập nhật giao diện THĂNG TẦNG ---
        currentRealmTxt.text = "Tầng " + pet.realm.ToString();
        currentRealmNode = baseInfo.progressionTable.GetRealmCost(pet.realm);

        if (currentRealmNode != null)
        {
            nextRealmTxt.text = "Tầng " + (pet.realm + 1).ToString();
            await RenderRequirements(currentRealmNode, realmItemsContainer, realmUpBtn);
        }
        else
        {
            nextRealmTxt.text = "MAX";
            realmUpBtn.interactable = false;
            ClearContainer(realmItemsContainer);
        }
    }

    private async Task RenderRequirements(ProgressionNode node, Transform container, Button upgradeBtn)
    {
        ClearContainer(container);

        var inventory = await InventoryManager.Instance.GetMyInventory();
        bool canUpgrade = true;

        foreach (var req in node.requiredItems)
        {
            // Sinh ra Prefab nguyên liệu
            var go = Instantiate(reqItemPrefab, container);
            
            // Tìm component theo tên (Hoặc bạn có thể viết script riêng cho Prefab này)
            var iconImg = go.transform.Find("Icon").GetComponent<Image>();
            var qtyTxt = go.transform.Find("QtyTxt").GetComponent<TextMeshProUGUI>();

            iconImg.sprite = req.item.icon;

            // Kiểm tra số lượng trong túi
            var invItem = inventory.Find(x => x.itemId == req.item.itemID);
            int currentQty = invItem != null ? invItem.quantity : 0;

            qtyTxt.text = $"{currentQty}/{req.quantity}";
            
            // Đổi màu chữ nếu thiếu đồ
            if (currentQty < req.quantity)
            {
                qtyTxt.color = Color.red;
                canUpgrade = false; // Thiếu 1 món cũng cấm bấm nút
            }
            else
            {
                qtyTxt.color = Color.green;
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
