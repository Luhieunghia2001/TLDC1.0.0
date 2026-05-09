using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PetUIItem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image tierImg;  // Ảnh hiển thị Tier (D, C, B, A, S...)
    [SerializeField] private Image iconImg;  // Ảnh đại diện của Pet
    [SerializeField] private TextMeshProUGUI levelTxt;
    [SerializeField] private Image realmImg; // Ảnh hiển thị Realm (Tầng 0-25)
    [SerializeField] private Button clickBtn; // Nút để bấm vào xem stats

    [Header("Tier Sprites")]
    [SerializeField] private Sprite[] tierSprites; // Kéo các ảnh Tier theo thứ tự D, C, B, A, S, SS, SSS
    [SerializeField] private Sprite[] realmSprites; // Kéo 26 ảnh Realm vào đây (Tầng 0 -> 25)

    private PetModel petData;

    public void Setup(PetModel pet)
    {
        this.petData = pet;

        // 1. Lấy dữ liệu mẫu (Icon) từ PetManager
        var baseInfo = PetManager.Instance.GetPetBaseByID(pet.petBaseId);
        if (baseInfo != null)
        {
            iconImg.sprite = baseInfo.icon;
        }

        // 2. Hiển thị Level
        levelTxt.text = "Lv." + pet.level;

        // 3. Hiển thị Ảnh Tier
        SetTierImage(pet.tier);

        // 4. Hiển thị Ảnh Realm
        SetRealmImage(pet.realm);

        // 5. Sự kiện bấm
        clickBtn.onClick.RemoveAllListeners();
        clickBtn.onClick.AddListener(OnShowDetail);
    }

    private void SetTierImage(string tier)
    {
        // Chuyển đổi chữ cái Tier thành chỉ số mảng (D=0, C=1, B=2, A=3, S=4, SS=5, SSS=6)
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

        if (index < tierSprites.Length)
        {
            tierImg.sprite = tierSprites[index];
        }
    }

    private void SetRealmImage(int realm)
    {
        if (realmSprites == null || realmSprites.Length == 0) return;

        // Ép kiểu/Clamp để tránh lỗi mảng nếu realm vượt quá số lượng sprite
        int index = Mathf.Clamp(realm, 0, realmSprites.Length - 1);
        
        if (realmImg != null)
        {
            realmImg.sprite = realmSprites[index];
        }
    }

    private void OnShowDetail()
    {
        if (PetDetailUI.Instance != null)
        {
            PetDetailUI.Instance.Open(petData);
        }
    }
}
