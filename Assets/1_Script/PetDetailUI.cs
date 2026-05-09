using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PetDetailUI : MonoBehaviour
{
    public static PetDetailUI Instance { get; private set; }
   
    [Header("Global References")]
    [SerializeField] private TextMeshProUGUI petNameTxt;
    [SerializeField] private Image elementImg;
    [SerializeField] private Image tierImg;

    [Header("UI References")]
    [SerializeField] private GameObject canvas;
    [SerializeField] private GameObject statpanel;

    [SerializeField] private TextMeshProUGUI levelTxt;
    [SerializeField] private TextMeshProUGUI expTxt;
    [SerializeField] private TextMeshProUGUI atkTypeTxt;


    [Header("Icon Assets")]
    [SerializeField] private Sprite[] elementSprites;
    [SerializeField] private Sprite[] tierSprites;

    [Header("Stats Texts")]
    [SerializeField] private TextMeshProUGUI hpTxt;
    [SerializeField] private TextMeshProUGUI atkPhyTxt;
    [SerializeField] private TextMeshProUGUI atkMagTxt;
    [SerializeField] private TextMeshProUGUI defPhyTxt;
    [SerializeField] private TextMeshProUGUI defMagTxt;
    [SerializeField] private TextMeshProUGUI speedTxt;

    [Header("Spawn Pet Model")]
    [SerializeField] private Transform spawnPoint; // Vị trí để hiện con Pet (3D hoặc 2D)
    private GameObject currentSpawnedPet; // Lưu con Pet đang hiện để xóa khi đổi con khác

    [Header("Buttons")]
    [SerializeField] private Button closeBtn;
    [SerializeField] private Button openStatsBtn; // Nút chuyển tab hoặc mở từ Home
    [SerializeField] private Button openCanvasBtn; // Nút mở toàn bộ Canvas từ Home

    private PetModel currentPet;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        canvas.SetActive(false);
        closeBtn.onClick.AddListener(ClosePanel);

        if (openStatsBtn != null)
            openStatsBtn.onClick.AddListener(OpenWithFirstPet);

        if (openCanvasBtn != null)
            openCanvasBtn.onClick.AddListener(OpenWithFirstPet);
    }

    private void OnEnable()
    {
        // Khi quay lại bảng Stats, tự làm mới chỉ số
        if (currentPet != null) Open(currentPet);
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
        currentPet = pet;
        canvas.SetActive(true);
        ClearSpawnedPet(); // Xóa con cũ

        petNameTxt.text = pet.petName;
        levelTxt.text = "Level: " + pet.level;
        
        int maxExp = pet.level * 100;
        expTxt.text = $"EXP: {pet.currentExp}/{maxExp}";

        // 1. Hiển thị Icon và Model
        var baseInfo = PetManager.Instance.GetPetBaseByID(pet.petBaseId);
        if (baseInfo != null)
        {
            SetElementIcon(pet.element);
            SetTierIcon(pet.tier);
            
            // Spawn con Pet mới
            if (baseInfo.petPrefab != null && spawnPoint != null)
            {
                currentSpawnedPet = Instantiate(baseInfo.petPrefab, spawnPoint.position, spawnPoint.rotation, spawnPoint);
                // Nếu là UI 3D, bạn có thể cần chỉnh Layer hoặc Scale ở đây
            }
        }

        atkTypeTxt.text = (pet.petType.ToLower() == "physical") ? "Hệ: Vật Lý" : "Hệ: Ma Pháp";

        hpTxt.text = "HP: " + pet.hp;
        atkPhyTxt.text = "ATK Vật Lý: " + pet.atkPhy;
        atkMagTxt.text = "ATK Ma Pháp: " + pet.atkMag;
        defPhyTxt.text = "THỦ Vật Lý: " + pet.defPhy;
        defMagTxt.text = "THỦ Ma Pháp: " + pet.defMag;
        speedTxt.text = "Tốc Độ: " + pet.speed;
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
}
