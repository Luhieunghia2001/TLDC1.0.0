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

    private List<PetModel> selectedTeam => PetManager.Instance.SelectedTeam;

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

        // Nếu Pet này nằm trong đội hình, vẽ lại giao diện ngay
        if (isChanged)
        {
            UpdateTeamVisuals();
        }
    }

    private async void OnEnable()
    {
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

            // 3. Xóa danh sách cũ và vẽ mới
            ClearAllPetsContainer();

            if (myPets == null) return;

            // 4. Lấy tập hợp các ID Pet đang trong đội
            HashSet<string> selectedIds = new HashSet<string>();
            foreach (var p in selectedTeam)
            {
                if (p != null && !string.IsNullOrEmpty(p.id))
                    selectedIds.Add(p.id.Trim().ToLower());
            }

            // 5. Sinh ra các ô Pet mới
            int displayedCount = 0;
            foreach (var pet in myPets)
            {
                if (pet == null || string.IsNullOrEmpty(pet.id)) continue;

                string petId = pet.id.Trim().ToLower();
                bool isSelected = selectedIds.Contains(petId);

                var item = Instantiate(petItemPrefab, allPetsContainer);
                if (item != null)
                {
                    item.Setup(pet, isSelected, () => AddToTeam(pet));
                    displayedCount++;
                }
            }
            
            // 6. Đồng bộ lại cả giao diện đội hình phía trên
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
