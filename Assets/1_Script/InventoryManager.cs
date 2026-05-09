using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Item Database")]
    [SerializeField] private List<ItemBaseSO> allItems;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    // Lấy thông tin mẫu của Item dựa trên ID
    public ItemBaseSO GetItemBaseByID(string id)
    {
        return allItems.Find(x => x.itemID == id);
    }

    // Lấy toàn bộ kho đồ của người chơi
    public async Task<List<InventoryModel>> GetMyInventory()
    {
        try
        {
            var response = await SupabaseManager.Instance.Client
                .From<InventoryModel>()
                .Get();
            return response.Models;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lỗi lấy kho đồ: " + e.Message);
            return new List<InventoryModel>();
        }
    }

    // Thêm vật phẩm (Gọi RPC add_item an toàn)
    public async Task AddItem(string itemID, int qty)
    {
        if (LoadingUI.Instance != null) LoadingUI.Instance.Show();
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "p_item_id", itemID },
                { "p_qty", qty }
            };

            await SupabaseManager.Instance.Client.Rpc("add_item", parameters);
            Debug.Log($"Đã thêm {qty} vật phẩm {itemID} vào kho!");
            
            // Cập nhật lại UI Kho đồ nếu đang mở
            if (InventoryUIController.Instance != null) InventoryUIController.Instance.RefreshInventory();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lỗi khi thêm đồ: " + e.Message);
        }
        if (LoadingUI.Instance != null) LoadingUI.Instance.Hide();
    }

    // Sử dụng bình EXP cho Pet
    public async Task UseExpPotion(string petID, ItemBaseSO potion)
    {
        if (LoadingUI.Instance != null) LoadingUI.Instance.Show();
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "p_pet_id", petID },
                { "p_item_id", potion.itemID },
                { "p_exp_amount", potion.value }
            };

            await SupabaseManager.Instance.Client.Rpc("use_pet_exp_potion", parameters);
            Debug.Log($"Sử dụng thành công {potion.itemName} cho Pet!");
            
            // Cập nhật lại UI Pet và UI Kho đồ
            if (PetListController.Instance != null) PetListController.Instance.RefreshPetList();
            if (InventoryUIController.Instance != null) InventoryUIController.Instance.RefreshInventory();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lỗi khi dùng bình EXP: " + e.Message);
        }
        if (LoadingUI.Instance != null) LoadingUI.Instance.Hide();
    }

    // Bán vật phẩm lấy Vàng
    public async Task SellItem(string itemID, int qty, int pricePerUnit)
    {
        if (LoadingUI.Instance != null) LoadingUI.Instance.Show();
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "p_item_id", itemID },
                { "p_qty", qty },
                { "p_price_per_unit", pricePerUnit }
            };

            await SupabaseManager.Instance.Client.Rpc("sell_item", parameters);
            Debug.Log($"Bán thành công {qty} vật phẩm. Nhận được {qty * pricePerUnit} Vàng!");
            
            // Cập nhật lại UI Kho đồ
            if (InventoryUIController.Instance != null) InventoryUIController.Instance.RefreshInventory();
            // Cập nhật lại các chỉ số Vàng trên màn hình chính
            if (ResourceManager.Instance != null) await ResourceManager.Instance.SyncWithServer();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lỗi khi bán đồ: " + e.Message);
        }
        if (LoadingUI.Instance != null) LoadingUI.Instance.Hide();
    }
}
