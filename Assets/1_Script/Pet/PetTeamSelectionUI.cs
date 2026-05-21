using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;

public class PetTeamSelectionUI : MonoBehaviour
{
    public static PetTeamSelectionUI Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;

    [Header("Lineup Slots (Top Panel)")]
    [SerializeField] private PetUIItem[] teamSlots; 

    [Header("All Pets List (Bottom Panel)")]
    [SerializeField] private Transform allPetsContainer;
    [SerializeField] private PetUIItem petItemPrefab;

    [Header("Filter Buttons (Optional)")]
    [SerializeField] private Button filterAllBtn;
    [SerializeField] private Button filterFireBtn;
    [SerializeField] private Button filterWaterBtn;
    [SerializeField] private Button filterEarthBtn;
    [SerializeField] private Button filterWindBtn;
    [SerializeField] private Color activeBtnColor = Color.white;
    [SerializeField] private Color inactiveBtnColor = new Color(0.7f, 0.7f, 0.7f, 1.0f);

    private List<PetModel> selectedTeam => PetManager.Instance.SelectedTeam;
    private List<PetModel> cachedMyPets = new List<PetModel>();
    private string currentFilter = "all"; // "all", "fire", "water", "earth", "wind"

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (PetManager.Instance != null)
        {
            // Lắng nghe sự kiện cập nhật Pet từ Server (Level Up, Star Up, Realm Up)
            PetManager.Instance.OnPetStatsUpdated += HandleGlobalPetUpdate;
        }

        // Tự động đăng ký sự kiện click cho các nút bộ lọc nếu được gán
        if (filterAllBtn != null) filterAllBtn.onClick.AddListener(() => SetFilter("all"));
        if (filterFireBtn != null) filterFireBtn.onClick.AddListener(() => SetFilter("fire"));
        if (filterWaterBtn != null) filterWaterBtn.onClick.AddListener(() => SetFilter("water"));
        if (filterEarthBtn != null) filterEarthBtn.onClick.AddListener(() => SetFilter("earth"));
        if (filterWindBtn != null) filterWindBtn.onClick.AddListener(() => SetFilter("wind"));

        UpdateFilterButtonVisuals();
    }

    private void OnDestroy()
    {
        if (PetManager.Instance != null)
        {
            PetManager.Instance.OnPetStatsUpdated -= HandleGlobalPetUpdate;
        }
    }

    private void HandleGlobalPetUpdate(PetModel updatedPet)
    {
        if (updatedPet == null || selectedTeam == null) return;

        bool isChanged = false;
        // Cập nhật lại dữ liệu mới nhất cho Pet trong đội hình
        for (int i = 0; i < selectedTeam.Count; i++)
        {
            if (selectedTeam[i] != null && selectedTeam[i].id == updatedPet.id)
            {
                selectedTeam[i] = updatedPet;
                isChanged = true;
                break;
            }
        }

        // Cập nhật dữ liệu mới nhất trong danh sách cache
        if (cachedMyPets != null)
        {
            for (int i = 0; i < cachedMyPets.Count; i++)
            {
                if (cachedMyPets[i] != null && cachedMyPets[i].id == updatedPet.id)
                {
                    cachedMyPets[i] = updatedPet;
                    UpdatePetsDisplay();
                    break;
                }
            }
        }

        // Nếu Pet này nằm trong đội hình, vẽ lại giao diện ngay
        if (isChanged)
        {
            UpdateTeamVisuals();
        }
    }

    private async void OnEnable()
    {
        // Reset bộ lọc về "all" mỗi lần mở lại giao diện
        currentFilter = "all";
        UpdateFilterButtonVisuals();

        // 1. Luôn cập nhật giao diện đội hình ngay lập tức (dữ liệu local)
        UpdateTeamVisuals();
        
        // 2. Đợi một nhịp để đảm bảo Unity đã active hoàn toàn object
        await Task.Yield();
        if (this == null || !gameObject.activeInHierarchy) return;

        // 3. Bắt đầu tải danh sách Pet
        _ = RefreshAllPetsList();
    }

    private void ClearAllPetsContainer()
    {
        if (allPetsContainer == null) return;
        
        // Xóa sạch container trước khi tạo mới
        for (int i = allPetsContainer.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(allPetsContainer.GetChild(i).gameObject);
        }
    }

    private int refreshId = 0;

    public async Task RefreshAllPetsList()
    {
        int currentRefresh = ++refreshId;
        
        // 1. Hiện Loading
        if (LoadingUI.Instance != null) LoadingUI.Instance.Show();

        try
        {
            // 2. Tải danh sách Pet mới nhất từ Server
            var myPets = await PetManager.Instance.GetMyPets();
            
            // Kiểm tra nếu có yêu cầu mới hơn hoặc Panel đã bị đóng trong lúc đợi await
            if (currentRefresh != refreshId || !gameObject.activeInHierarchy) return;

            // Lưu danh sách lấy từ server vào cache
            cachedMyPets = myPets ?? new List<PetModel>();

            // 3. Vẽ danh sách dựa theo bộ lọc
            UpdatePetsDisplay();
            
            // 4. Đồng bộ lại cả giao diện đội hình phía trên
            UpdateTeamVisuals();
        }
        catch (System.Exception e)
        {
            Debug.LogError("[PetTeamSelection] Lỗi khi nạp danh sách: " + e.Message);
        }
        finally
        {
            if (currentRefresh == refreshId && LoadingUI.Instance != null)
            {
                LoadingUI.Instance.Hide();
            }
        }
    }

    public void SetFilter(string filter)
    {
        currentFilter = filter.ToLower();
        UpdateFilterButtonVisuals();
        UpdatePetsDisplay();
    }

    private void UpdateFilterButtonVisuals()
    {
        UpdateButtonColor(filterAllBtn, currentFilter == "all");
        UpdateButtonColor(filterFireBtn, currentFilter == "fire");
        UpdateButtonColor(filterWaterBtn, currentFilter == "water");
        UpdateButtonColor(filterEarthBtn, currentFilter == "earth");
        UpdateButtonColor(filterWindBtn, currentFilter == "wind");
    }

    private void UpdateButtonColor(Button btn, bool isActive)
    {
        if (btn == null) return;
        var img = btn.GetComponent<Image>();
        if (img != null)
        {
            img.color = isActive ? activeBtnColor : inactiveBtnColor;
        }
    }

    private void UpdatePetsDisplay()
    {
        // 1. Xóa sạch danh sách hiển thị cũ
        ClearAllPetsContainer();

        if (cachedMyPets == null) return;

        // 2. Lấy tập hợp các ID Pet đang trong đội hình để đánh dấu
        HashSet<string> selectedIds = new HashSet<string>();
        foreach (var p in selectedTeam)
        {
            if (p != null && !string.IsNullOrEmpty(p.id))
                selectedIds.Add(p.id.Trim().ToLower());
        }

        // 3. Sinh các ô Pet phù hợp với bộ lọc
        foreach (var pet in cachedMyPets)
        {
            if (pet == null || string.IsNullOrEmpty(pet.id)) continue;

            // Kiểm tra bộ lọc hệ
            if (currentFilter != "all")
            {
                if (string.IsNullOrEmpty(pet.element) || pet.element.ToLower() != currentFilter)
                {
                    continue;
                }
            }

            string petId = pet.id.Trim().ToLower();
            bool isSelected = selectedIds.Contains(petId);

            var item = Instantiate(petItemPrefab, allPetsContainer);
            if (item != null)
            {
                item.Setup(pet, isSelected, () => AddToTeam(pet));
            }
        }
    }

    private void AddToTeam(PetModel pet)
    {
        if (pet == null || string.IsNullOrEmpty(pet.id)) return;
        
        string petId = pet.id.Trim().ToLower();
        // So sánh an toàn
        if (selectedTeam.Exists(p => p != null && !string.IsNullOrEmpty(p.id) && p.id.Trim().ToLower() == petId)) return;

        if (selectedTeam.Count < PetManager.MAX_TEAM_SIZE)
        {
            selectedTeam.Add(pet);
            UpdateTeamVisuals();
            _ = RefreshAllPetsList(); 
        }
    }

    private void RemoveFromTeam(int index)
    {
        if (index >= 0 && index < selectedTeam.Count)
        {
            selectedTeam.RemoveAt(index);
            UpdateTeamVisuals();
            _ = RefreshAllPetsList(); 
        }
    }

    private void UpdateTeamVisuals()
    {
        if (teamSlots == null) return;

        for (int i = 0; i < teamSlots.Length; i++)
        {
            if (teamSlots[i] == null) continue;

            int index = i; 
            if (i < selectedTeam.Count)
            {
                teamSlots[i].SetEmpty(false);
                // Truyền false cho isSelected vì các ô phía trên không cần hiện overlay chọn
                teamSlots[i].Setup(selectedTeam[i], false, () => RemoveFromTeam(index));
            }
            else
            {
                teamSlots[i].SetEmpty(true);
            }
        }
    }

    public List<PetModel> GetSelectedTeam()
    {
        return selectedTeam;
    }
}
