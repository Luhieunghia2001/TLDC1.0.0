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
                { "p_item_id", potion.itemID }
                // XÓA: Không gửi p_exp_amount lên nữa, Server tự tra cứu
            };

            await SupabaseManager.Instance.Client.Rpc("use_pet_exp_potion_secure", parameters);
            Debug.Log($"Sử dụng thành công {potion.itemName} cho Pet!");
            
            // Cập nhật lại UI Pet, UI Kho đồ và UI Nâng cấp
            if (PetListController.Instance != null) PetListController.Instance.RefreshPetList();
            if (InventoryUIController.Instance != null) InventoryUIController.Instance.RefreshInventory();
            if (PetUpgradeUI.Instance != null) await PetUpgradeUI.Instance.RefreshUI();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lỗi khi dùng bình EXP (Chi tiết): \n" + e.ToString());
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
                { "p_qty", qty }
                // XÓA: Không gửi p_price_per_unit lên nữa
            };

            await SupabaseManager.Instance.Client.Rpc("sell_item_secure", parameters);
            Debug.Log($"Đã gửi lệnh bán {qty} vật phẩm lên Server!");
            
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

    public async Task PetStarUp(string petID, ProgressionNode costNode)
    {
        if (LoadingUI.Instance != null) LoadingUI.Instance.Show();
        try
        {
            List<string> itemIds = new List<string>();
            List<int> itemQtys = new List<int>();

            foreach (var req in costNode.requiredItems)
            {
                itemIds.Add(req.item.itemID);
                itemQtys.Add(req.quantity);
            }

            var parameters = new Dictionary<string, object>
            {
                { "p_pet_id", petID },
                { "p_item_ids", itemIds },
                { "p_item_qtys", itemQtys }
            };

            await SupabaseManager.Instance.Client.Rpc("pet_star_up", parameters);
            Debug.Log("Thăng Sao Thành Công!");
            
            // Cập nhật lại UI
            if (PetListController.Instance != null) PetListController.Instance.RefreshPetList();
            if (InventoryUIController.Instance != null) InventoryUIController.Instance.RefreshInventory();
            if (PetUpgradeUI.Instance != null) await PetUpgradeUI.Instance.RefreshUI();
            
            // Force fetch myPets to update global state
            var myPets = await PetManager.Instance.GetMyPets();
            var latestPet = myPets.Find(x => x.id == petID);
            if (latestPet != null) PetManager.Instance.NotifyPetStatsUpdated(latestPet);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lỗi khi Thăng Sao: \n" + e.ToString());
        }
        if (LoadingUI.Instance != null) LoadingUI.Instance.Hide();
    }

    public async Task PetRealmUp(string petID, ProgressionNode costNode)
    {
        if (LoadingUI.Instance != null) LoadingUI.Instance.Show();
        try
        {
            List<string> itemIds = new List<string>();
            List<int> itemQtys = new List<int>();

            foreach (var req in costNode.requiredItems)
            {
                itemIds.Add(req.item.itemID);
                itemQtys.Add(req.quantity);
            }

            var parameters = new Dictionary<string, object>
            {
                { "p_pet_id", petID },
                { "p_item_ids", itemIds },
                { "p_item_qtys", itemQtys }
            };

            await SupabaseManager.Instance.Client.Rpc("pet_realm_up", parameters);
            Debug.Log("Thăng Tầng Thành Công!");
            
            // Cập nhật lại UI
            if (PetListController.Instance != null) PetListController.Instance.RefreshPetList();
            if (InventoryUIController.Instance != null) InventoryUIController.Instance.RefreshInventory();
            if (PetUpgradeUI.Instance != null) await PetUpgradeUI.Instance.RefreshUI();
            
            // Force fetch myPets to update global state
            var myPets = await PetManager.Instance.GetMyPets();
            var latestPet = myPets.Find(x => x.id == petID);
            if (latestPet != null) PetManager.Instance.NotifyPetStatsUpdated(latestPet);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lỗi khi Thăng Tầng: \n" + e.ToString());
        }
        if (LoadingUI.Instance != null) LoadingUI.Instance.Hide();
    }

    public async Task EquipEquipment(string petID, EquipmentSlot slot, string itemID)
    {
        if (LoadingUI.Instance != null) LoadingUI.Instance.Show();
        try
        {
            string slotStr = slot.ToString().ToLower();
            var parameters = new Dictionary<string, object>
            {
                { "p_pet_id", petID },
                { "p_slot", slotStr },
                { "p_item_id", itemID }
            };

            await SupabaseManager.Instance.Client.Rpc("equip_pet_item", parameters);
            Debug.Log($"Trang bị thành công {itemID} vào vị trí {slotStr}!");

            // Refresh UI
            if (PetListController.Instance != null) PetListController.Instance.RefreshPetList();
            if (InventoryUIController.Instance != null) InventoryUIController.Instance.RefreshInventory();
            
            // Force fetch myPets to update global state
            var myPets = await PetManager.Instance.GetMyPets();
            var latestPet = myPets.Find(x => x.id == petID);
            if (latestPet != null) PetManager.Instance.NotifyPetStatsUpdated(latestPet);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lỗi khi mặc trang bị: \n" + e.ToString());
        }
        if (LoadingUI.Instance != null) LoadingUI.Instance.Hide();
    }

    public async Task UnequipEquipment(string petID, EquipmentSlot slot)
    {
        if (LoadingUI.Instance != null) LoadingUI.Instance.Show();
        try
        {
            string slotStr = slot.ToString().ToLower();
            var parameters = new Dictionary<string, object>
            {
                { "p_pet_id", petID },
                { "p_slot", slotStr }
            };

            await SupabaseManager.Instance.Client.Rpc("unequip_pet_item", parameters);
            Debug.Log($"Tháo trang bị thành công tại vị trí {slotStr}!");

            // Refresh UI
            if (PetListController.Instance != null) PetListController.Instance.RefreshPetList();
            if (InventoryUIController.Instance != null) InventoryUIController.Instance.RefreshInventory();
            
            // Force fetch myPets to update global state
            var myPets = await PetManager.Instance.GetMyPets();
            var latestPet = myPets.Find(x => x.id == petID);
            if (latestPet != null) PetManager.Instance.NotifyPetStatsUpdated(latestPet);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lỗi khi tháo trang bị: \n" + e.ToString());
        }
        if (LoadingUI.Instance != null) LoadingUI.Instance.Hide();
    }
}
