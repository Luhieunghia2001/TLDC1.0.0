using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour
{
    public static InventoryUIController Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Transform contentContainer;

    [Header("Tab Buttons")]
    [SerializeField] private Button consumableTab;
    [SerializeField] private Button materialTab;
    [SerializeField] private Button equipmentTab;

    private ItemType currentTab = ItemType.Consumable;
    private List<InventoryModel> fullInventory = new List<InventoryModel>();

    private void Awake()
    {
        Instance = this;
        // Gán sự kiện cho các Tab
        consumableTab.onClick.AddListener(() => SwitchTab(ItemType.Consumable));
        materialTab.onClick.AddListener(() => SwitchTab(ItemType.Material));
        equipmentTab.onClick.AddListener(() => SwitchTab(ItemType.Equipment));
    }

    private void OnEnable()
    {
        RefreshInventory();
    }

    public async void RefreshInventory()
    {
        // 1. Lấy toàn bộ dữ liệu từ Server
        fullInventory = await InventoryManager.Instance.GetMyInventory();
        
        // 2. Hiển thị theo Tab đang chọn
        UpdateDisplay();
    }

    private void SwitchTab(ItemType newTab)
    {
        currentTab = newTab;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        // Xóa các ô cũ
        foreach (Transform child in contentContainer) Destroy(child.gameObject);

        // Lọc và hiển thị vật phẩm theo Tab
        foreach (var item in fullInventory)
        {
            var baseInfo = InventoryManager.Instance.GetItemBaseByID(item.itemId);
            
            // Nếu vật phẩm khớp với Tab đang chọn và có BaseInfo thì mới hiện
            if (baseInfo != null && baseInfo.type == currentTab)
            {
                if (currentTab == ItemType.Equipment)
                {
                    // Đối với trang bị: Sinh ra nhiều ô độc lập tương ứng với số lượng trong kho
                    for (int i = 0; i < item.quantity; i++)
                    {
                        GameObject newItem = Instantiate(itemPrefab, contentContainer);
                        newItem.GetComponent<InventoryUIItem>().Setup(item);
                    }
                }
                else
                {
                    GameObject newItem = Instantiate(itemPrefab, contentContainer);
                    newItem.GetComponent<InventoryUIItem>().Setup(item);
                }
            }
        }
    }
}
