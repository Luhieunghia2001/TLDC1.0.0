using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PetSkillUIItem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImg;
    [SerializeField] private Image tierImg; // Hiển thị khung S, A, B...
    [SerializeField] private Button skillButton; // Button để click mở panel

    [Header("Tier Sprites")]
    [SerializeField] private Sprite[] tierSprites; // Danh sách ảnh tương ứng S, A, B, C, D

    [Header("Description Panel")]
    [SerializeField] private GameObject descriptionPanelPrefab; // Prefab của panel mô tả
    [SerializeField] private Transform canvasTransform; // Canvas để instantiate panel

    private PetSkillSO currentSkill;
    private static SkillDescriptionPanel currentPanelInstance; // Static để chia sẻ giữa tất cả instances

    private void Awake()
    {
        // Tự động lấy Button component nếu chưa gán
        if (skillButton == null)
        {
            skillButton = GetComponent<Button>();
        }

        // Đăng ký sự kiện click
        if (skillButton != null)
        {
            skillButton.onClick.AddListener(OnSkillClicked);
        }
    }

    private void OnDestroy()
    {
        if (skillButton != null)
        {
            skillButton.onClick.RemoveListener(OnSkillClicked);
        }
    }

    private void OnSkillClicked()
    {
        if (descriptionPanelPrefab == null || currentSkill == null)
        {
            Debug.LogWarning("Description Panel Prefab hoặc Skill chưa được gán!");
            return;
        }

        // Nếu đã có instance đang hiển thị, đóng nó đi
        if (currentPanelInstance != null)
        {
            Destroy(currentPanelInstance.gameObject);
            currentPanelInstance = null;
            return;
        }

        // Instantiate prefab
        GameObject panelObj = Instantiate(descriptionPanelPrefab, canvasTransform);
        currentPanelInstance = panelObj.GetComponent<SkillDescriptionPanel>();

        if (currentPanelInstance != null)
        {
            currentPanelInstance.UpdateSkillInfo(currentSkill);
        }
        else
        {
            Debug.LogError("Prefab không có component SkillDescriptionPanel!");
            Destroy(panelObj);
        }
    }

    public void Setup(PetSkillSO skill)
    {
        currentSkill = skill;

        if (skill == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        // 1. Hiển thị Icon Skill
        if (iconImg != null)
        {
            iconImg.sprite = skill.icon;
        }

        // 2. Hiển thị Sprite Tier dựa trên Enum index
        // SkillTier: D=0, C=1, B=2, A=3, S=4, SS=5, SSS=6
        if (tierImg != null && tierSprites != null)
        {
            int tierIndex = (int)skill.tier;
            if (tierIndex < tierSprites.Length)
            {
                tierImg.sprite = tierSprites[tierIndex];
            }
        }
    }
}
