using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryTest : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField idInput;      // Ô nhập ID vật phẩm
    [SerializeField] private TMP_InputField qtyInput;     // Ô nhập số lượng
    [SerializeField] private Button addBtn;               // Nút bấm nhận đồ
    [SerializeField] private Button addPetBtn;            // Nút bấm nhận Pet

    private void Start()
    {
        // Gán sự kiện cho nút bấm
        if (addBtn != null)
        {
            addBtn.onClick.AddListener(OnAddBtnClick);
        }
        if (addPetBtn != null)
        {
            addPetBtn.onClick.AddListener(OnAddPetBtnClick);
        }
    }

    private async void OnAddBtnClick()
    {
        string itemID = idInput.text;
        
        // Chuyển đổi số lượng từ chữ sang số
        if (!int.TryParse(qtyInput.text, out int qty))
        {
            qty = 1; // Mặc định là 1 nếu nhập sai
        }

        if (string.IsNullOrEmpty(itemID))
        {
            Debug.LogError("Vui lòng nhập Item ID!");
            return;
        }

        addBtn.interactable = false; // Khóa nút khi đang xử lý
        Debug.Log($"<color=cyan>[UI Test]</color> Đang thêm {qty} '{itemID}'...");

        if (InventoryManager.Instance != null)
        {
            await InventoryManager.Instance.AddItem(itemID, qty);
            Debug.Log("<color=green>[UI Test]</color> Thành công!");
        }
        else
        {
            Debug.LogError("InventoryManager Instance not found!");
        }

        addBtn.interactable = true; // Mở lại nút
    }

    private async void OnAddPetBtnClick()
    {
        string petID = idInput.text;

        if (string.IsNullOrEmpty(petID))
        {
            Debug.LogError("Vui lòng nhập Pet ID!");
            return;
        }

        addPetBtn.interactable = false; // Khóa nút khi đang xử lý
        Debug.Log($"<color=cyan>[UI Test]</color> Đang thêm 1 Pet '{petID}'...");

        if (PetManager.Instance != null)
        {
            var petBase = PetManager.Instance.GetPetBaseByID(petID);
            if (petBase != null)
            {
                await PetManager.Instance.CreateNewPet(petBase);
                Debug.Log("<color=green>[UI Test]</color> Thêm Pet thành công!");
                
                // Refresh list Pet
                if (PetListController.Instance != null)
                {
                    PetListController.Instance.RefreshPetList();
                }
            }
            else
            {
                Debug.LogError($"Không tìm thấy dữ liệu mẫu cho Pet ID: {petID}");
            }
        }
        else
        {
            Debug.LogError("PetManager Instance not found!");
        }

        addPetBtn.interactable = true; // Mở lại nút
    }

    // Vẫn giữ lại ContextMenu để bạn có thể dùng cả 2 cách
    [ContextMenu("Check My Inventory")]
    public async void CheckInventory()
    {
        if (InventoryManager.Instance != null)
        {
            var items = await InventoryManager.Instance.GetMyInventory();
            Debug.Log($"<color=green>[Inventory]</color> Bạn có {items.Count} loại đồ.");
            foreach (var item in items)
            {
                Debug.Log($"- {item.itemId}: {item.quantity}");
            }
        }
    }
}
