using UnityEngine;
using UnityEngine.UI;

public class PetEquipmentUI : MonoBehaviour
{
    public static PetEquipmentUI Instance { get; private set; }

    [Header("Equipment Slot Buttons")]
    [SerializeField] private Button helmetBtn;
    [SerializeField] private Button armorBtn;
    [SerializeField] private Button weaponBtn;
    [SerializeField] private Button bootsBtn;
    [SerializeField] private Button wingsBtn;
    [SerializeField] private Button amuletBtn;

    [Header("Slot Icon Images")]
    [SerializeField] private Image helmetIcon;
    [SerializeField] private Image armorIcon;
    [SerializeField] private Image weaponIcon;
    [SerializeField] private Image bootsIcon;
    [SerializeField] private Image wingsIcon;
    [SerializeField] private Image amuletIcon;

    [Header("Default Placeholder Sprites (Optional)")]
    [SerializeField] private Sprite defaultHelmetSprite;
    [SerializeField] private Sprite defaultArmorSprite;
    [SerializeField] private Sprite defaultWeaponSprite;
    [SerializeField] private Sprite defaultBootsSprite;
    [SerializeField] private Sprite defaultWingsSprite;
    [SerializeField] private Sprite defaultAmuletSprite;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Đăng ký sự kiện nút bấm cho các slot
        if (helmetBtn != null) helmetBtn.onClick.AddListener(() => OpenSelection(EquipmentSlot.Helmet));
        if (armorBtn != null) armorBtn.onClick.AddListener(() => OpenSelection(EquipmentSlot.Armor));
        if (weaponBtn != null) weaponBtn.onClick.AddListener(() => OpenSelection(EquipmentSlot.Weapon));
        if (bootsBtn != null) bootsBtn.onClick.AddListener(() => OpenSelection(EquipmentSlot.Boots));
        if (wingsBtn != null) wingsBtn.onClick.AddListener(() => OpenSelection(EquipmentSlot.Wings));
        if (amuletBtn != null) amuletBtn.onClick.AddListener(() => OpenSelection(EquipmentSlot.Amulet));
    }

    private void Start()
    {
        if (PetManager.Instance != null)
        {
            PetManager.Instance.OnPetSelected += OnPetChanged;
            PetManager.Instance.OnPetStatsUpdated += OnPetChanged;
        }

        // Tự động làm mới giao diện khi khởi chạy
        RefreshUI();
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
        RefreshUI();
    }

    public void RefreshUI()
    {
        var pet = PetManager.Instance.CurrentPet;
        if (pet == null) return;

        UpdateSlotUI(pet.helmetId, helmetIcon, defaultHelmetSprite);
        UpdateSlotUI(pet.armorId, armorIcon, defaultArmorSprite);
        UpdateSlotUI(pet.weaponId, weaponIcon, defaultWeaponSprite);
        UpdateSlotUI(pet.bootsId, bootsIcon, defaultBootsSprite);
        UpdateSlotUI(pet.wingsId, wingsIcon, defaultWingsSprite);
        UpdateSlotUI(pet.amuletId, amuletIcon, defaultAmuletSprite);
    }

    private void UpdateSlotUI(string itemId, Image iconImage, Sprite defaultSprite)
    {
        if (iconImage == null) return;

        if (string.IsNullOrEmpty(itemId))
        {
            iconImage.sprite = defaultSprite;
            // Cho icon hơi mờ/trong suốt nếu trống
            var color = iconImage.color;
            color.a = 0.5f;
            iconImage.color = color;
        }
        else
        {
            var itemBase = InventoryManager.Instance.GetItemBaseByID(itemId);
            if (itemBase != null)
            {
                iconImage.sprite = itemBase.icon;
                var color = iconImage.color;
                color.a = 1.0f;
                iconImage.color = color;
            }
            else
            {
                iconImage.sprite = defaultSprite;
                var color = iconImage.color;
                color.a = 0.5f;
                iconImage.color = color;
            }
        }
    }

    private void OpenSelection(EquipmentSlot slot)
    {
        if (PetEquipmentSelectionUI.Instance != null)
        {
            PetEquipmentSelectionUI.Instance.Open(slot);
        }
        else
        {
            Debug.LogError("Chưa khởi tạo PetEquipmentSelectionUI Instance!");
        }
    }
}
