using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;

public class PetEquipmentSelectionUI : MonoBehaviour
{
    public static PetEquipmentSelectionUI Instance { get; private set; }

    [Header("Main Panel")]
    [SerializeField] private GameObject panel;

    [Header("UI Texts")]
    [SerializeField] private TextMeshProUGUI slotTitleTxt;

    [Header("Equipped Item Section")]
    [SerializeField] private GameObject equippedSection;
    [SerializeField] private Image equippedIcon;
    [SerializeField] private Image equippedTierImg; // Hiển thị khung/nhãn Tier của trang bị đang mặc
    [SerializeField] private TextMeshProUGUI equippedNameTxt;
    [SerializeField] private TextMeshProUGUI equippedStatsTxt;
    [SerializeField] private Button unequipBtn;

    [Header("Available List Section")]
    [SerializeField] private Transform listContainer;
    [SerializeField] private GameObject selectionItemPrefab; // Item prefab in list

    [Header("Close Button")]
    [SerializeField] private Button closeBtn;

    private EquipmentSlot currentSlot;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (panel != null) panel.SetActive(false);

        if (unequipBtn != null) unequipBtn.onClick.AddListener(OnUnequipClicked);
        if (closeBtn != null) closeBtn.onClick.AddListener(ClosePanel);
    }

    public void Open(EquipmentSlot slot)
    {
        currentSlot = slot;
        if (panel != null)
        {
            panel.SetActive(true);
            _ = RefreshUI();
        }
    }

    public void ClosePanel()
    {
        if (panel != null) panel.SetActive(false);
    }

    public async Task RefreshUI()
    {
        // Đợi nạp xong templates từ CSDL trước khi xử lý logic bên dưới
        await InventoryManager.Instance.LoadTask;

        var pet = PetManager.Instance.CurrentPet;
        if (pet == null) return;

        // 1. Cập nhật Title dựa theo Slot
        string slotTitle = "";
        string equippedItemId = "";
        switch (currentSlot)
        {
            case EquipmentSlot.Helmet: slotTitle = "CHỌN NÓN"; equippedItemId = pet.helmetId; break;
            case EquipmentSlot.Armor: slotTitle = "CHỌN ÁO"; equippedItemId = pet.armorId; break;
            case EquipmentSlot.Weapon: slotTitle = "CHỌN VŨ KHÍ"; equippedItemId = pet.weaponId; break;
            case EquipmentSlot.Boots: slotTitle = "CHỌN GIÀY"; equippedItemId = pet.bootsId; break;
            case EquipmentSlot.Wings: slotTitle = "CHỌN CÁNH"; equippedItemId = pet.wingsId; break;
            case EquipmentSlot.Amulet: slotTitle = "CHỌN BỘI"; equippedItemId = pet.amuletId; break;
        }
        slotTitleTxt.text = slotTitle;

        // 2. Hiển thị Trang bị đang mặc
        if (string.IsNullOrEmpty(equippedItemId))
        {
            equippedSection.SetActive(false);
            if (equippedTierImg != null) equippedTierImg.gameObject.SetActive(false);
        }
        else
        {
            equippedSection.SetActive(true);
            var itemBase = InventoryManager.Instance.GetItemBaseByID(equippedItemId);
            var itemTemplate = InventoryManager.Instance.GetItemTemplateByID(equippedItemId);
            if (itemBase != null)
            {
                equippedIcon.sprite = itemBase.icon;
                equippedNameTxt.text = itemTemplate != null ? itemTemplate.name : itemBase.itemName;
                equippedStatsTxt.text = GetStatsString(itemTemplate);

                if (equippedTierImg != null)
                {
                    if (InventoryManager.Instance != null)
                    {
                        PetTier tierEnum = PetTier.D;
                        if (itemTemplate != null) System.Enum.TryParse(itemTemplate.tier, true, out tierEnum);
                        else tierEnum = itemBase.tier;

                        Sprite s = InventoryManager.Instance.GetTierSprite(tierEnum);
                        if (s != null)
                        {
                            equippedTierImg.gameObject.SetActive(true);
                            equippedTierImg.sprite = s;
                        }
                        else
                        {
                            equippedTierImg.gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        equippedTierImg.gameObject.SetActive(false);
                    }
                }
            }
        }

        // 3. Hiển thị danh sách trang bị tương thích trong kho + trang bị của Pet khác
        foreach (Transform child in listContainer)
        {
            Destroy(child.gameObject);
        }

        var myPets = await PetManager.Instance.GetMyPets();
        var inventory = await InventoryManager.Instance.GetMyInventory();
        Debug.Log($"<color=cyan>[SelectionUI]</color> Tổng đồ trong kho: {inventory.Count}. Đang lọc cho slot: {currentSlot}");

        List<SelectionItemData> allOptions = new List<SelectionItemData>();

        // A. Thêm các trang bị rảnh rỗi trong túi đồ (Không có Pet nào sử dụng)
        foreach (var invItem in inventory)
        {
            var baseInfo = InventoryManager.Instance.GetItemBaseByID(invItem.itemId);
            
            // Chỉ xử lý nếu là Trang bị và đúng Slot
            if (baseInfo == null || baseInfo.type != ItemType.Equipment || baseInfo.equipSlot != currentSlot)
                continue;

            var template = InventoryManager.Instance.GetItemTemplateByID(invItem.itemId);

            // Lúc này mới Warning nếu tìm thấy SO nhưng không thấy Template trong DB
            if (template == null) 
                Debug.LogWarning($"[SelectionUI] Item {invItem.itemId} là Trang bị nhưng không tìm thấy Template trong DB! Hãy kiểm tra bảng item_templates.");

            // Vẫn thêm vào danh sách hiển thị dù template null
            for (int i = 0; i < invItem.quantity; i++)
            {
                allOptions.Add(new SelectionItemData
                {
                    inventoryId = invItem.id,
                    itemBase = baseInfo,
                    itemTemplate = template,
                    quantity = 1,
                    enhancementLevel = invItem.enhancement_level,
                    equippedPetName = "Không"
                });
            }
        }

        // B. Thêm các trang bị đang được mặc bởi các Pet KHÁC
        foreach (var otherPet in myPets)
        {
            if (otherPet.id == pet.id) continue; // Bỏ qua chính Pet đang chọn

            string otherEquipId = "";
            switch (currentSlot)
            {
                case EquipmentSlot.Helmet: otherEquipId = otherPet.helmetId; break;
                case EquipmentSlot.Armor: otherEquipId = otherPet.armorId; break;
                case EquipmentSlot.Weapon: otherEquipId = otherPet.weaponId; break;
                case EquipmentSlot.Boots: otherEquipId = otherPet.bootsId; break;
                case EquipmentSlot.Wings: otherEquipId = otherPet.wingsId; break;
                case EquipmentSlot.Amulet: otherEquipId = otherPet.amuletId; break;
            }

            if (!string.IsNullOrEmpty(otherEquipId))
            {
                var baseInfo = InventoryManager.Instance.GetItemBaseByID(otherEquipId);
                var template = InventoryManager.Instance.GetItemTemplateByID(otherEquipId);
                if (baseInfo != null)
                {
                    allOptions.Add(new SelectionItemData
                    {
                        itemBase = baseInfo,
                        itemTemplate = template,
                        quantity = 1,
                        equippedPetName = otherPet.petName
                    });
                }
            }
        }

        // Sinh ra các ô trang bị lựa chọn
        foreach (var option in allOptions)
        {
            GameObject newItem = Instantiate(selectionItemPrefab, listContainer);
            if (newItem.TryGetComponent<PetEquipmentSelectionItemUI>(out var itemUI))
            {
                itemUI.Setup(option.itemBase, option.itemTemplate, option.quantity, option.equippedPetName, async () => {
                    if (string.IsNullOrEmpty(option.inventoryId)) return;
                    await InventoryManager.Instance.EquipEquipment(pet.id, currentSlot, option.inventoryId);
                    _ = RefreshUI();
                    if (PetEquipmentUI.Instance != null) PetEquipmentUI.Instance.RefreshUI();
                });
            }
        }
    }

    private async void OnUnequipClicked()
    {
        var pet = PetManager.Instance.CurrentPet;
        if (pet == null) return;

        await InventoryManager.Instance.UnequipEquipment(pet.id, currentSlot);
        _ = RefreshUI();
        if (PetEquipmentUI.Instance != null) PetEquipmentUI.Instance.RefreshUI();
    }

    public static string GetStatsString(ItemTemplateModel item)
    {
        if (item == null) return "<color=red>Chưa có chỉ số trong CSDL</color>";

        var parts = new List<string>();
        if (item.bonusHP > 0) parts.Add($"+{item.bonusHP} HP");
        if (item.bonusAtkPhy > 0) parts.Add($"+{item.bonusAtkPhy} Công Vật Lý");
        if (item.bonusAtkMag > 0) parts.Add($"+{item.bonusAtkMag} Công Ma Pháp");
        if (item.bonusDefPhy > 0) parts.Add($"+{item.bonusDefPhy} Thủ Vật Lý");
        if (item.bonusDefMag > 0) parts.Add($"+{item.bonusDefMag} Thủ Ma Pháp");
        if (item.bonusSpeed > 0) parts.Add($"+{item.bonusSpeed} Tốc Độ");

        if (item.percentHP > 0) parts.Add($"+{Mathf.RoundToInt(item.percentHP * 100)}% HP");
        if (item.percentAtk > 0) parts.Add($"+{Mathf.RoundToInt(item.percentAtk * 100)}% ATK");
        if (item.percentSpeed > 0) parts.Add($"+{Mathf.RoundToInt(item.percentSpeed * 100)}% Tốc Độ");

        return string.Join("\n", parts);
    }
}

public class SelectionItemData
{
    public string inventoryId;
    public ItemBaseSO itemBase;
    public ItemTemplateModel itemTemplate;
    public int quantity;
    public int enhancementLevel;
    public string equippedPetName;
}
