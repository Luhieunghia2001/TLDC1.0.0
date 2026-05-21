using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class PetDetailUI : MonoBehaviour
{
    public static PetDetailUI Instance { get; private set; }
   
    [Header("Global References")]
    [SerializeField] private TextMeshProUGUI petNameTxt;
    [SerializeField] private Image elementImg;
    [SerializeField] private Image tierImg;
    [SerializeField] private Image realmImg;

    [Header("UI References")]
    [SerializeField] private GameObject canvas;
    [SerializeField] private GameObject statpanel;

    [SerializeField] private TextMeshProUGUI levelTxt;
    [SerializeField] private TextMeshProUGUI expTxt;
    [SerializeField] private TextMeshProUGUI atkTypeTxt;


    [Header("Icon Assets")]
    [SerializeField] private Sprite[] elementSprites;
    [SerializeField] private Sprite[] tierSprites;
    [SerializeField] private Sprite[] realmSprites;

    [Header("Stats Texts")]
    [SerializeField] private TextMeshProUGUI hpTxt;
    [SerializeField] private TextMeshProUGUI atkPhyTxt;
    [SerializeField] private TextMeshProUGUI atkMagTxt;
    [SerializeField] private TextMeshProUGUI defPhyTxt;
    [SerializeField] private TextMeshProUGUI defMagTxt;
    [SerializeField] private TextMeshProUGUI speedTxt;
    [SerializeField] private TextMeshProUGUI combatPowerTxt;

    [Header("Spawn Pet Model")]
    [SerializeField] private Transform spawnPoint; // Vị trí để hiện con Pet (3D hoặc 2D)
    private GameObject currentSpawnedPet; // Lưu con Pet đang hiện để xóa khi đổi con khác

    [Header("Buttons")]
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button openCanvasBtn; // Nút mở toàn bộ Canvas từ Home
    [SerializeField] private Button quickEquipBtn;
    [SerializeField] private Button unequipAllBtn;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        canvas.SetActive(false);
        closeBtn.onClick.AddListener(ClosePanel);

        if (openCanvasBtn != null)
            openCanvasBtn.onClick.AddListener(OpenWithFirstPet);

        if (quickEquipBtn != null)
            quickEquipBtn.onClick.AddListener(OnQuickEquipClicked);

        if (unequipAllBtn != null)
            unequipAllBtn.onClick.AddListener(OnUnequipAllClicked);

        if (statpanel != null)
        {
            var tracker = statpanel.AddComponent<EnableTracker>();
            tracker.onEnableEvent += OnStatPanelOpened;
        }
    }

    private void Start()
    {
        // Đăng ký lắng nghe sự kiện từ Global State
        PetManager.Instance.OnPetSelected += HandlePetSelected;
        PetManager.Instance.OnPetStatsUpdated += RefreshStatsUI;
    }

    private void OnDestroy()
    {
        if (PetManager.Instance != null)
        {
            PetManager.Instance.OnPetSelected -= HandlePetSelected;
            PetManager.Instance.OnPetStatsUpdated -= RefreshStatsUI;
        }
    }

    private class EnableTracker : MonoBehaviour
    {
        public System.Action onEnableEvent;
        private void OnEnable() { if (onEnableEvent != null) onEnableEvent.Invoke(); }
    }

    private int statRefreshId = 0;

    private async void OnStatPanelOpened()
    {
        var pet = PetManager.Instance.CurrentPet;
        if (pet == null) return;
        
        int currentRefresh = ++statRefreshId;

        RefreshStatsUI(pet); // Cập nhật ngay bằng Local State

        // Cập nhật ngầm từ Server
        var myPets = await PetManager.Instance.GetMyPets();
        if (currentRefresh != statRefreshId) return;

        var latestPet = myPets.Find(x => x.id == pet.id);
        if (latestPet != null)
        {
            PetManager.Instance.NotifyPetStatsUpdated(latestPet); // Bắn Event cho toàn cục
        }
    }

    private void RefreshStatsUI(PetModel pet)
    {
        var baseInfo = PetManager.Instance.GetPetBaseByID(pet.petBaseId);
        if (baseInfo == null) return;

        petNameTxt.text = pet.petName;
        levelTxt.text = $"Level: {pet.level}";
        
        int maxExp = pet.level * 100;
        expTxt.text = $"EXP: {pet.currentExp}/{maxExp}";

        atkTypeTxt.text = (pet.petType.ToLower() == "physical") ? "Tinh linh: Vật Lý" : "Tinh linh: Ma Pháp";

        // Cập nhật icons (Element, Tier, Realm)
        SetElementIcon(pet.element);
        SetTierIcon(pet.tier);
        SetRealmIcon(pet.realm);

        // Yêu cầu máy tính (Client) Tính toán tức thời các chỉ số
        PetFinalStats finalStats = PetStatsCalculator.GetFinalStats(pet, baseInfo);

        hpTxt.text = "HP: " + finalStats.HP;
        atkPhyTxt.text = "ATK Vật Lý: " + finalStats.AtkPhy;
        atkMagTxt.text = "ATK Ma Pháp: " + finalStats.AtkMag;
        defPhyTxt.text = "THỦ Vật Lý: " + finalStats.DefPhy;
        defMagTxt.text = "THỦ Ma Pháp: " + finalStats.DefMag;
        speedTxt.text = "Tốc Độ: " + finalStats.Speed;

        // Tính và hiển thị Lực Chiến
        int combatPower = PetStatsCalculator.CalculateCombatPower(finalStats);
        if (combatPowerTxt != null)
            combatPowerTxt.text = "Lực Chiến: " + combatPower;
    }

    private void ClosePanel()
    {
        canvas.SetActive(false);
        ClearSpawnedPet();
    }

    private void ClearSpawnedPet()
    {
        if (currentSpawnedPet != null)
        {
            Destroy(currentSpawnedPet);
        }
    }

    public async void OpenWithFirstPet()
    {
        if (LoadingUI.Instance != null) LoadingUI.Instance.Show();
        
        var myPets = await PetManager.Instance.GetMyPets();
        if (myPets != null && myPets.Count > 0)
        {
            Open(myPets[0]);
        }
        
        if (LoadingUI.Instance != null) LoadingUI.Instance.Hide();
    }

    public void Open(PetModel pet)
    {
        canvas.SetActive(true);
        PetManager.Instance.SelectPet(pet); // Cập nhật Global State, tự động trigger HandlePetSelected
    }

    private void HandlePetSelected(PetModel pet)
    {
        RefreshStatsUI(pet);

        // 1. Hiển thị Icon và Model
        var baseInfo = PetManager.Instance.GetPetBaseByID(pet.petBaseId);
        if (baseInfo != null)
        {
            SetElementIcon(pet.element);
            SetTierIcon(pet.tier);
            SetRealmIcon(pet.realm);
            
            ClearSpawnedPet();
            // Spawn con Pet mới
            if (baseInfo.petPrefab != null && spawnPoint != null)
            {
                currentSpawnedPet = Instantiate(baseInfo.petPrefab, spawnPoint.position, spawnPoint.rotation, spawnPoint);
            }
        }
    }

    private void SetElementIcon(string element)
    {
        int index = 0;
        switch (element.ToLower())
        {
            case "fire": index = 0; break;
            case "wind": index = 1; break;
            case "earth": index = 2; break;
            case "water": index = 3; break;
        }
        if (index < elementSprites.Length) elementImg.sprite = elementSprites[index];
    }

    private void SetTierIcon(string tier)
    {
        int index = 0;
        switch (tier.ToUpper())
        {
            case "D": index = 0; break;
            case "C": index = 1; break;
            case "B": index = 2; break;
            case "A": index = 3; break;
            case "S": index = 4; break;
            case "SS": index = 5; break;
            case "SSS": index = 6; break;
        }
        if (index < tierSprites.Length) tierImg.sprite = tierSprites[index];
    }

    private void SetRealmIcon(int realm)
    {
        if (realmImg == null || realmSprites == null || realmSprites.Length == 0) return;
        int index = Mathf.Clamp(realm - 1, 0, realmSprites.Length - 1);
        realmImg.sprite = realmSprites[index];
    }

    // ===== QUICK EQUIP ALL =====
    private async void OnQuickEquipClicked()
    {
        var pet = PetManager.Instance.CurrentPet;
        if (pet == null) return;
        await QuickEquipAll(pet);
    }

    private async Task QuickEquipAll(PetModel pet)
    {
        if (LoadingUI.Instance != null) LoadingUI.Instance.Show();

        try
        {
            var inventory = await InventoryManager.Instance.GetMyInventory();

            EquipmentSlot[] slots = { EquipmentSlot.Helmet, EquipmentSlot.Armor, EquipmentSlot.Weapon, EquipmentSlot.Boots, EquipmentSlot.Wings, EquipmentSlot.Amulet };

            foreach (var slot in slots)
            {
                // Bỏ qua slot đã có trang bị
                if (!string.IsNullOrEmpty(GetEquippedItemId(pet, slot))) continue;

                // Tìm trang bị tốt nhất trong kho cho slot này
                ItemBaseSO bestItem = null;
                string bestInventoryItemId = null;
                int bestTierValue = -1;

                foreach (var invItem in inventory)
                {
                    if (invItem.quantity <= 0) continue;
                    var baseInfo = InventoryManager.Instance.GetItemBaseByID(invItem.itemId);
                    if (baseInfo == null) continue;
                    if (baseInfo.type != ItemType.Equipment) continue;
                    if (baseInfo.equipSlot != slot) continue;

                    int tierVal = (int)baseInfo.tier;
                    if (tierVal > bestTierValue)
                    {
                        bestTierValue = tierVal;
                        bestItem = baseInfo;
                        bestInventoryItemId = invItem.itemId;
                    }
                }

                if (bestItem != null && bestInventoryItemId != null)
                {
                    await InventoryManager.Instance.EquipEquipment(pet.id, slot, bestInventoryItemId);
                    // Refresh inventory sau mỗi lần equip để tránh mặc trùng
                    inventory = await InventoryManager.Instance.GetMyInventory();
                }
            }

            // Refresh lại Pet data sau khi mặc xong
            var myPets = await PetManager.Instance.GetMyPets();
            var latestPet = myPets.Find(x => x.id == pet.id);
            if (latestPet != null)
                PetManager.Instance.NotifyPetStatsUpdated(latestPet);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lỗi khi mặc nhanh trang bị: \n" + e.ToString());
        }

        if (LoadingUI.Instance != null) LoadingUI.Instance.Hide();
    }

    // ===== UNEQUIP ALL =====
    private async void OnUnequipAllClicked()
    {
        var pet = PetManager.Instance.CurrentPet;
        if (pet == null) return;
        await UnequipAll(pet);
    }

    private async Task UnequipAll(PetModel pet)
    {
        if (LoadingUI.Instance != null) LoadingUI.Instance.Show();

        try
        {
            EquipmentSlot[] slots = { EquipmentSlot.Helmet, EquipmentSlot.Armor, EquipmentSlot.Weapon, EquipmentSlot.Boots, EquipmentSlot.Wings, EquipmentSlot.Amulet };

            foreach (var slot in slots)
            {
                string equippedId = GetEquippedItemId(pet, slot);
                if (!string.IsNullOrEmpty(equippedId))
                {
                    await InventoryManager.Instance.UnequipEquipment(pet.id, slot);
                }
            }

            // Refresh lại Pet data sau khi tháo xong
            var myPets = await PetManager.Instance.GetMyPets();
            var latestPet = myPets.Find(x => x.id == pet.id);
            if (latestPet != null)
                PetManager.Instance.NotifyPetStatsUpdated(latestPet);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lỗi khi tháo hết trang bị: \n" + e.ToString());
        }

        if (LoadingUI.Instance != null) LoadingUI.Instance.Hide();
    }

    // ===== HELPER =====
    private string GetEquippedItemId(PetModel pet, EquipmentSlot slot)
    {
        switch (slot)
        {
            case EquipmentSlot.Helmet: return pet.helmetId;
            case EquipmentSlot.Armor: return pet.armorId;
            case EquipmentSlot.Weapon: return pet.weaponId;
            case EquipmentSlot.Boots: return pet.bootsId;
            case EquipmentSlot.Wings: return pet.wingsId;
            case EquipmentSlot.Amulet: return pet.amuletId;
            default: return null;
        }
    }
}
