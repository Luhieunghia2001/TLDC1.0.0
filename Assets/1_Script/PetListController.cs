using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class PetListController : MonoBehaviour
{
    public static PetListController Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject petItemPrefab; // Kéo Prefab ô Pet vào đây
    [SerializeField] private Transform contentContainer; // Kéo Content của ScrollView vào đây

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        // Mỗi khi bảng Pet được mở lên, chúng ta sẽ làm mới danh sách
        RefreshPetList();
    }

    public async void RefreshPetList()
    {
        // 1. Xóa các ô cũ đang hiện có (nếu có)
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. Lấy danh sách Pet mới nhất từ Server
        if (PetManager.Instance == null)
        {
            Debug.LogError("Lỗi: PetManager.Instance chưa được khởi tạo!");
            return;
        }
        List<PetModel> myPets = await PetManager.Instance.GetMyPets();

        // 3. Sinh ra các ô Pet mới
        foreach (var pet in myPets)
        {
            GameObject newItem = Instantiate(petItemPrefab, contentContainer);
            if (newItem.TryGetComponent<PetUIItem>(out var uiItem))
            {
                uiItem.Setup(pet);
            }
        }
        
        Debug.Log($"Đã cập nhật danh sách: {myPets.Count} Pet.");
    }
}
