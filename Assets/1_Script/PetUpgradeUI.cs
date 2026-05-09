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
    [SerializeField] private Button openUpgradeBtn; // Nút mở bảng từ bên ngoài (ví dụ ở Home)

    [SerializeField] private GameObject panel;
    private PetModel currentPet;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        panel.SetActive(false);
        quickUpgradeBtn.onClick.AddListener(OnQuickUpgrade);
        
        // Gán sự kiện mở bảng bằng code
        if (openUpgradeBtn != null)
            openUpgradeBtn.onClick.AddListener(OpenWithFirstPet);
    }

    private void OnEnable()
    {
        // Mỗi khi bảng được bật lên (kể cả qua PanelManager), tự Refresh dữ liệu
        if (currentPet != null)
        {
            _ = RefreshUI();
        }
    }

    public async void Open(PetModel pet)
    {
        currentPet = pet;
        panel.SetActive(true);
        await RefreshUI();
    }

    // Hàm mới để mở nhanh cho con Pet đầu tiên trong danh sách
    public async void OpenWithFirstPet()
    {
        if (LoadingUI.Instance != null) LoadingUI.Instance.Show();
        
        var myPets = await PetManager.Instance.GetMyPets();
        if (myPets != null && myPets.Count > 0)
        {
            Open(myPets[0]); // Mở cho con đầu tiên
        }
        else
        {
            Debug.LogWarning("Bạn chưa có con Pet nào để nâng cấp!");
        }

        if (LoadingUI.Instance != null) LoadingUI.Instance.Hide();
    }

    public async Task RefreshUI()
    {
        if (currentPet == null) return;

        // Lấy lại dữ liệu Pet mới nhất từ Server thông qua PetManager
        var myPets = await PetManager.Instance.GetMyPets();
        var latestPet = myPets.Find(x => x.id == currentPet.id);
        if (latestPet != null) currentPet = latestPet;

        // 1. Cập nhật Level và Thanh EXP
        levelTxt.text = "Cấp: " + currentPet.level;
        int maxExp = currentPet.level * 100;
        expTxt.text = $"{currentPet.currentExp}/{maxExp}";
        expFillBar.fillAmount = (float)currentPet.currentExp / maxExp;

        // 2. Cập nhật số lượng từng loại bình EXP trong túi
        var inventory = await InventoryManager.Instance.GetMyInventory();
        foreach (var slot in expItemSlots)
        {
            var itemData = inventory.Find(x => x.itemId == slot.GetItemID());
            int quantity = (itemData != null) ? itemData.quantity : 0;
            slot.UpdateQuantity(quantity);
        }
    }

    private async void OnQuickUpgrade()
    {
        if (currentPet == null) return;
        Debug.Log("Đang thực hiện Thăng cấp nhanh...");

        // Lấy kho đồ để xem có bao nhiêu bình EXP
        var inventory = await InventoryManager.Instance.GetMyInventory();
        
        // Sắp xếp các ô vật phẩm (thường ô 0 là bình nhỏ nhất)
        foreach (var slot in expItemSlots)
        {
            var itemBase = slot.GetItemBase();
            var itemInInv = inventory.Find(x => x.itemId == itemBase.itemID);
            
            if (itemInInv != null && itemInInv.quantity > 0)
            {
                // Dùng hết số lượng bình này hoặc dùng cho đến khi Pet lên cấp (tùy bạn muốn)
                // Ở đây ta cứ dùng thử 1 bình để demo, bạn có thể cho chạy vòng lặp
                await InventoryManager.Instance.UseExpPotion(currentPet.id, itemBase);
                
                // Nếu muốn dùng tiếp thì ta gọi lại hàm này hoặc chạy Loop
                // Tạm thời ta để dùng từng bình một mỗi lần bấm để an toàn
                break; 
            }
        }
    }

    // Hàm bổ trợ để các ô vật phẩm gọi khi được bấm
    public async void UseItem(ItemBaseSO potion)
    {
        if (currentPet == null) return;
        await InventoryManager.Instance.UseExpPotion(currentPet.id, potion);
        
        // Sau khi dùng, chúng ta cần lấy lại dữ liệu Pet mới nhất để cập nhật thanh EXP
        // Ở đây ta có thể tạm thời cộng giả lập hoặc Refresh lại toàn bộ
        // (Tôi sẽ hướng dẫn bạn cách đồng bộ tối ưu nhất sau)
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
