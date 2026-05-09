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

    [Header("Star UI (5 images)")]
    [Tooltip("Gán đúng 5 Image slots (left -> right).")]
    [SerializeField] private Image[] starImgs; // Mảng 5 image để hiển thị sao

    [Header("Tier Sprites")]
    [SerializeField] private Sprite[] tierSprites; // Kéo các ảnh Tier theo thứ tự D, C, B, A, S, SS, SSS
    [SerializeField] private Sprite[] realmSprites; // Kéo 26 ảnh Realm vào đây (Tầng 0 -> 25)
    [SerializeField] private Sprite[] starSprites; // index mapping: [0]=fallback, [1]=1s_v2, [2]=1s_v3, ...

    private PetModel petData;

    public void Setup(PetModel pet)
    {
        this.petData = pet;

        // 1. Lấy dữ liệu mẫu (Icon) từ PetManager
        var baseInfo = PetManager.Instance.GetPetBaseByID(pet.petBaseId);
        if (baseInfo != null && iconImg != null)
        {
            iconImg.sprite = baseInfo.icon;
        }

        // 2. Hiển thị Level
        if (levelTxt != null)
        {
            levelTxt.text = "Lv." + pet.level;
        }

        // 3. Hiển thị Ảnh Tier
        SetTierImage(pet.tier);

        // 4. Hiển thị Ảnh Realm
        SetRealmImage(pet.realm);

        // 5. Hiển thị Stars (sử dụng mảng 5 Image)
        SetStarImages(pet.star);

        // 6. Sự kiện bấm
        if (clickBtn != null)
        {
            clickBtn.onClick.RemoveAllListeners();
            clickBtn.onClick.AddListener(OnShowDetail);
        }
    }

    private void SetTierImage(string tier)
    {
        if (tierImg == null || tierSprites == null || tierSprites.Length == 0) return;

        int index = 0;
        switch ((tier ?? string.Empty).ToUpper())
        {
            case "D": index = 0; break;
            case "C": index = 1; break;
            case "B": index = 2; break;
            case "A": index = 3; break;
            case "S": index = 4; break;
            case "SS": index = 5; break;
            case "SSS": index = 6; break;
            default: index = 0; break;
        }

        index = Mathf.Clamp(index, 0, tierSprites.Length - 1);
        tierImg.sprite = tierSprites[index];
    }

    private void SetRealmImage(int realm)
    {
        if (realmImg == null || realmSprites == null || realmSprites.Length == 0) return;

        int index = Mathf.Clamp(realm, 0, realmSprites.Length - 1);
        realmImg.sprite = realmSprites[index];
    }

    private void SetStarImages(int star)
    {
        // if no star image slots provided, nothing to do
        if (starImgs == null || starImgs.Length == 0)
            return;

        int slotCount = starImgs.Length;

        // If no star sprites available, disable all slots
        if (starSprites == null || starSprites.Length == 0)
        {
            for (int i = 0; i < slotCount; i++)
            {
                if (starImgs[i] == null) continue;
                starImgs[i].enabled = false;
                starImgs[i].sprite = null;
            }
            return;
        }

        // Hide all if star <= 0
        if (star <= 0)
        {
            for (int i = 0; i < slotCount; i++)
            {
                if (starImgs[i] == null) continue;
                starImgs[i].enabled = false;
                starImgs[i].sprite = null;
            }
            return;
        }

        // Determine how many images should be filled (1..5)
        int filled = star % 5;
        if (filled == 0) filled = 5; // e.g., 5, 10 -> show 5 filled images

        // Determine which sprite tier to use based on star group of 5:
        // stars 1..5 -> tierIndex = 1 (1s_v2), stars 6..10 -> tierIndex = 2 (1s_v3), ...
        int tierIndex = 1 + ((star - 1) / 5);
        tierIndex = Mathf.Clamp(tierIndex, 0, starSprites.Length - 1);

        Sprite chosenFilledSprite = starSprites[tierIndex];
        // fallback to index 0 if chosen is null
        if (chosenFilledSprite == null && starSprites.Length > 0)
            chosenFilledSprite = starSprites[0];

        // Apply sprites to slots: filled -> chosen sprite + enabled, empty -> disabled
        for (int i = 0; i < slotCount; i++)
        {
            var img = starImgs[i];
            if (img == null) continue;

            if (i < filled)
            {
                if (chosenFilledSprite != null)
                {
                    img.sprite = chosenFilledSprite;
                    img.enabled = true;
                }
                else
                {
                    img.enabled = false;
                    img.sprite = null;
                }
            }
            else
            {
                // Empty slots are disabled (no sprite)
                img.enabled = false;
                img.sprite = null;
            }
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