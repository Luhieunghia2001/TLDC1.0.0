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

    private void Start()
    {
        // Lấy dữ liệu từ AuthManager hoặc ResourceManager
        // Ở đây ta có thể tạo một hàm GetCurrentCharacter trong AuthManager để tiện lấy
        InvokeRepeating(nameof(UpdateUI), 0f, 0.5f); // Cập nhật UI mỗi 0.5 giây
    }

    private void UpdateUI()
    {
        // Ở ResourceManager mình nên để currentCharacter là public để bên này truy cập được
        var player = ResourceManager.Instance.GetCharacterData();
        
        if (player == null) return;

        nameTxt.text = player.characterName;
        levelTxt.text = "Lv. " + player.level;
        expTxt.text = "EXP: " + player.currentExp;
        
        goldTxt.text = player.gold.ToString("N0"); // N0 để tự thêm dấu phẩy ngăn cách nghìn
        diamondTxt.text = player.diamond.ToString("N0");

        // Hiển thị dạng Hiện tại/Tối đa
        energyTxt.text = $"{player.energy}/240";
        staminaTxt.text = $"{player.stamina}/120";
    }
}
