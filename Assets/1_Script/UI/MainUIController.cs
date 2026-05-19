using UnityEngine;
using TMPro;

public class MainUIController : MonoBehaviour
{
    [Header("Top Bar Info")]
    [SerializeField] private TextMeshProUGUI nameTxt;
    [SerializeField] private TextMeshProUGUI levelTxt;
    [SerializeField] private TextMeshProUGUI expTxt;

    [Header("Currencies")]
    [SerializeField] private TextMeshProUGUI goldTxt;
    [SerializeField] private TextMeshProUGUI diamondTxt;

    [Header("Resources")]
    [SerializeField] private TextMeshProUGUI energyTxt;
    [SerializeField] private TextMeshProUGUI staminaTxt;

    private CharacterModel player;

    private void OnEnable()
    {
        // Đăng ký lắng nghe sự kiện cập nhật dữ liệu nhân vật từ ResourceManager
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnCharacterDataUpdated += UpdateUI;
        }
        // Cập nhật UI ngay lập tức khi panel được bật để hiển thị dữ liệu ban đầu
        UpdateUI(ResourceManager.Instance.GetCharacterData());
    }

    private void OnDisable()
    {
        // Hủy đăng ký sự kiện khi panel bị tắt để tránh lỗi và rò rỉ bộ nhớ
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnCharacterDataUpdated -= UpdateUI;
        }
    }

    // Xóa hàm Start() và InvokeRepeating vì chúng ta sẽ dùng sự kiện
    // private void Start()
    // {
    //     // Lấy dữ liệu từ AuthManager hoặc ResourceManager
    //     // Ở đây ta có thể tạo một hàm GetCurrentCharacter trong AuthManager để tiện lấy
    //     InvokeRepeating(nameof(UpdateUI), 0f, 0.5f); // Cập nhật UI mỗi 0.5 giây
    // }

    // Hàm này sẽ được gọi mỗi khi ResourceManager kích hoạt sự kiện OnCharacterDataUpdated
    // và nhận về CharacterModel mới nhất
    private void UpdateUI(CharacterModel updatedPlayer)
    {
        if (updatedPlayer == null) return;

        nameTxt.text = updatedPlayer.character_name;
        levelTxt.text = "Lv. " + updatedPlayer.level;

        // Hiển thị EXP dạng Hiện tại/Tổng cần để lên cấp tiếp theo
        // Nếu Level 1 cần 100, Level 2 cần 200...
        int expToNextLevel = updatedPlayer.level * 100;
        if (expTxt != null && updatedPlayer != null)
        {
            expTxt.text = $"EXP: {updatedPlayer.current_exp:N0}/{expToNextLevel:N0}";
        }
        
        goldTxt.text = updatedPlayer.gold.ToString("N0"); 
        diamondTxt.text = updatedPlayer.diamond.ToString("N0");

        // Hiển thị dạng Hiện tại/Tối đa
        energyTxt.text = $"{updatedPlayer.energy}/240";
        staminaTxt.text = $"{updatedPlayer.stamina}/120";
    }
}
