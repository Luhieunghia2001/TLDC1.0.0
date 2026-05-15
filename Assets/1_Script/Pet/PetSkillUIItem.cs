using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PetSkillUIItem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImg;
    [SerializeField] private Image tierImg; // Hiển thị khung S, A, B...

    [Header("Tier Sprites")]
    [SerializeField] private Sprite[] tierSprites; // Danh sách ảnh tương ứng S, A, B, C, D

    public void Setup(PetSkillSO skill)
    {
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
