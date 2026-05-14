using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;

public class PetUpgradeUI : MonoBehaviour
{
    public static PetUpgradeUI Instance { get; private set; }

    [Header("Pet Info")]
    [SerializeField] private TextMeshProUGUI levelTxt;
    [SerializeField] private TextMeshProUGUI expTxt;
    [SerializeField] private Image expFillBar; // Image Type là Horizontal

    [Header("Exp Items Setup")]
    [SerializeField] private List<ExpItemSlot> expItemSlots; // Danh sách 6 ô vật phẩm trong ảnh

    [Header("Quick Upgrade")]
    [SerializeField] private Button quickUpgradeBtn;

    [SerializeField] private GameObject panel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        
        if (panel != null)
        {
            var tracker = panel.AddComponent<EnableTracker>();
            tracker.onEnableEvent += OnPanelOpened;
            if (panel.activeInHierarchy) OnPanelOpened();
        }

        quickUpgradeBtn.onClick.AddListener(OnQuickUpgrade);
    }

    private void Start()
    {
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

    private class EnableTracker : MonoBehaviour
    {
        public System.Action onEnableEvent;
        private void OnEnable() { if (onEnableEvent != null) onEnableEvent.Invoke(); }
    }

    private void OnPetChanged(PetModel pet)
    {
        if (panel != null && panel.activeInHierarchy)
        {
            // Chỉ cập nhật lại UI text nội bộ, KHÔNG gọi lại RefreshUI (tránh loop vô hạn tải từ Server)
            UpdateTexts(pet);
        }
    }

    private void OnPanelOpened()
    {
        _ = RefreshUI();
    }

    public void Open(PetModel pet)
    {
        if (PetManager.Instance.CurrentPet?.id != pet.id)
        {
            PetManager.Instance.SelectPet(pet); // Kích hoạt Event toàn cục
        }
        panel.SetActive(true);
    }

    private int refreshId = 0;

    public async Task RefreshUI()
    {
        var targetPet = PetManager.Instance.CurrentPet;
        if (targetPet == null) return;
        
        int currentRefresh = ++refreshId;

        if (LoadingUI.Instance != null) LoadingUI.Instance.Show();
        try
        {
            UpdateTexts(targetPet);

            var myPets = await PetManager.Instance.GetMyPets();
            if (currentRefresh != refreshId) return;

            var latestPet = myPets.Find(x => x.id == targetPet.id);
            if (latestPet != null) 
            {
                PetManager.Instance.NotifyPetStatsUpdated(latestPet); // Broadcast để UI khác cũng biết
            }

            // 2. Cập nhật số lượng từng loại bình EXP trong túi
            var inventory = await InventoryManager.Instance.GetMyInventory();
            foreach (var slot in expItemSlots)
            {
                var itemData = inventory.Find(x => x.itemId == slot.GetItemID());
                int quantity = (itemData != null) ? itemData.quantity : 0;
                slot.UpdateQuantity(quantity);
            }
        }
        finally
        {
            if (LoadingUI.Instance != null) LoadingUI.Instance.Hide();
        }
    }

    private void UpdateTexts(PetModel pet)
    {
        levelTxt.text = "Cấp: " + pet.level;
        int maxExp = pet.level * 100;
        expTxt.text = $"{pet.currentExp}/{maxExp}";
        expFillBar.fillAmount = (float)pet.currentExp / maxExp;
    }

    private async void OnQuickUpgrade()
    {
        var targetPet = PetManager.Instance.CurrentPet;
        if (targetPet == null) return;
        Debug.Log("Đang thực hiện Thăng cấp nhanh...");

        // Lấy kho đồ để xem có bao nhiêu bình EXP
        var inventory = await InventoryManager.Instance.GetMyInventory();
        
        // Sắp xếp các ô vật phẩm (thường ô 0 là bình nhỏ nhất)
        foreach (var slot in expItemSlots)
        {
            var itemBase = slot.GetItemBase();
            var itemInInv = inventory.Find(x => x.itemId == itemBase.itemID);
            int quantity = (itemInInv != null) ? itemInInv.quantity : 0;
            
            if (quantity > 0)
            {
                // Dùng hết số lượng bình này hoặc dùng cho đến khi Pet lên cấp (tùy bạn muốn)
                // Ở đây ta cứ dùng thử 1 bình để demo, bạn có thể cho chạy vòng lặp
                await InventoryManager.Instance.UseExpPotion(targetPet.id, itemBase);
                
                // Nếu muốn dùng tiếp thì ta gọi lại hàm này hoặc chạy Loop
                // Tạm thời ta để dùng từng bình một mỗi lần bấm để an toàn
                break; 
            }
        }
    }

    // Hàm bổ trợ để các ô vật phẩm gọi khi được bấm
    public async void UseItem(ItemBaseSO potion)
    {
        var targetPet = PetManager.Instance.CurrentPet;
        if (targetPet == null) return;
        await InventoryManager.Instance.UseExpPotion(targetPet.id, potion);
    }
}

[System.Serializable]
public class ExpItemSlot
{
    [SerializeField] private ItemBaseSO itemBase;
    [SerializeField] private Image iconImg;
    [SerializeField] private TextMeshProUGUI quantityTxt;
    [SerializeField] private TextMeshProUGUI expValueTxt; // Chữ "Exp +30"
    [SerializeField] private Button useBtn;

    public string GetItemID() => itemBase.itemID;
    public ItemBaseSO GetItemBase() => itemBase;

    public void UpdateQuantity(int qty)
    {
        quantityTxt.text = qty.ToString();
        expValueTxt.text = "Exp +" + itemBase.value;
        iconImg.sprite = itemBase.icon;

        // Nếu hết đồ thì làm mờ nút hoặc khóa nút
        useBtn.interactable = qty > 0;
        
        useBtn.onClick.RemoveAllListeners();
        useBtn.onClick.AddListener(() => PetUpgradeUI.Instance.UseItem(itemBase));
    }
}
