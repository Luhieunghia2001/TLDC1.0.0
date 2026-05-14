using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PetUIItem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image tierImg;
    [SerializeField] private Image iconImg;
    [SerializeField] private TextMeshProUGUI levelTxt;
    [SerializeField] private Image realmImg;
    [SerializeField] private Button clickBtn; 
    [SerializeField] private GameObject content; // Chứa Icon, Level, Sao...

    [Header("Star UI (5 images)")]
    [SerializeField] private Image[] starImgs;

    [Header("Sprites Database")]
    [SerializeField] private Sprite[] tierSprites;
    [SerializeField] private Sprite[] realmSprites;
    [SerializeField] private Sprite[] starSprites;

    private PetModel petData;

    private void Start()
    {
        if (PetManager.Instance != null)
        {
            PetManager.Instance.OnPetStatsUpdated += OnGlobalPetUpdated;
        }
    }

    private void OnDestroy()
    {
        if (PetManager.Instance != null)
        {
            PetManager.Instance.OnPetStatsUpdated -= OnGlobalPetUpdated;
        }
    }

    private void OnGlobalPetUpdated(PetModel updatedPet)
    {
        // Nếu UI này đang hiển thị đúng con Pet vừa được nâng cấp
        if (petData != null && petData.id == updatedPet.id)
        {
            // Cập nhật lại data mới nhất để không bị lỗi "chỉ số ảo" khi click vào
            this.petData = updatedPet;
            
            // Cập nhật lại giao diện (Level, Sao, Tầng)
            if (levelTxt != null) levelTxt.text = "" + petData.level;
            SetTierImage(petData.tier);
            SetRealmImage(petData.realm);
            SetStarImages(petData.star);
        }
    }

    public void Setup(PetModel pet, System.Action customAction = null)
    {
        this.petData = pet;

        if (content != null) content.SetActive(true);

        // 1. Icon & Level
        var baseInfo = PetManager.Instance.GetPetBaseByID(pet.petBaseId);
        if (baseInfo != null && iconImg != null) iconImg.sprite = baseInfo.icon;
        if (levelTxt != null) levelTxt.text = "Lv." + pet.level;

        // 2. Tier & Realm & Stars
        SetTierImage(pet.tier);
        SetRealmImage(pet.realm);
        SetStarImages(pet.star);

        // 3. Gán sự kiện Click
        if (clickBtn != null)
        {
            var img = clickBtn.GetComponent<Image>();
            if (img != null) img.raycastTarget = true;

            clickBtn.onClick.RemoveAllListeners();
            if (customAction != null)
            {
                clickBtn.onClick.AddListener(() => customAction.Invoke());
            }
            else
            {
                clickBtn.onClick.AddListener(OnShowDetail);
            }
        }
    }

    public void SetEmpty(bool isEmpty, System.Action onEmptyClick = null)
    {
        if (content != null) content.SetActive(!isEmpty);
        
        if (isEmpty && clickBtn != null)
        {
            clickBtn.onClick.RemoveAllListeners();
            if (onEmptyClick != null)
            {
                clickBtn.onClick.AddListener(() => onEmptyClick.Invoke());
            }
        }
    }

    private void SetTierImage(string tier)
    {
        if (tierImg == null || tierSprites == null || tierSprites.Length == 0) return;
        int index = 0;
        switch ((tier ?? "").ToUpper())
        {
            case "D": index = 0; break;
            case "C": index = 1; break;
            case "B": index = 2; break;
            case "A": index = 3; break;
            case "S": index = 4; break;
            case "SS": index = 5; break;
            case "SSS": index = 6; break;
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
        if (starImgs == null || starImgs.Length == 0 || starSprites == null || starSprites.Length == 0) return;
        int filled = star % 5;
        if (filled == 0 && star > 0) filled = 5;
        int tierIndex = Mathf.Clamp((star - 1) / 5, 0, starSprites.Length - 1);
        Sprite s = starSprites[tierIndex];

        for (int i = 0; i < starImgs.Length; i++)
        {
            if (starImgs[i] == null) continue;
            starImgs[i].enabled = (i < filled);
            if (i < filled) starImgs[i].sprite = s;
        }
    }

    private void OnShowDetail()
    {
        if (PetDetailUI.Instance != null) PetDetailUI.Instance.Open(petData);
    }
}