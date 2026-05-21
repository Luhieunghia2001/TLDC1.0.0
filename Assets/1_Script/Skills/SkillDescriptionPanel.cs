using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillDescriptionPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImg;
    [SerializeField] private Image tierImg; // Hiển thị khung/nhãn Tier của Skill
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI statsText;

    [Header("Sprites Database")]
    [SerializeField] private Sprite[] tierSprites; // Mảng chứa Sprite Tier từ D đến SSS (7 phần tử)

    private PetSkillSO currentSkill;

    public void UpdateSkillInfo(PetSkillSO skill)
    {
        currentSkill = skill;

        if (skill == null)
        {
            ClearPanel();
            return;
        }

        // Hiển thị icon
        if (iconImg != null && skill.icon != null)
        {
            iconImg.sprite = skill.icon;
            iconImg.enabled = true;
        }

        // Hiển thị tên skill
        if (skillNameText != null)
        {
            skillNameText.text = skill.skillName;
        }

        // Hiển thị tier bằng Image
        SetTierImage(skill.tier);

        // Hiển thị mô tả động
        if (descriptionText != null)
        {
            descriptionText.text = skill.GetFormattedDescription();
        }

        // Hiển thị các chỉ số chi tiết
        if (statsText != null)
        {
            statsText.text = GenerateStatsText(skill);
        }
    }

    private void SetTierImage(SkillTier tier)
    {
        if (tierImg == null) return;

        if (tierSprites != null && tierSprites.Length > 0)
        {
            int index = (int)tier;
            index = Mathf.Clamp(index, 0, tierSprites.Length - 1);
            tierImg.gameObject.SetActive(true);
            tierImg.sprite = tierSprites[index];
        }
        else
        {
            tierImg.gameObject.SetActive(false);
        }
    }

    private string GenerateStatsText(PetSkillSO skill)
    {
        string stats = "";

        // Loại skill
        stats += $"Loại: {skill.skillType}\n";

        // Trigger
        if (skill.trigger != SkillTrigger.None)
        {
            stats += $"Kích hoạt: {skill.trigger}\n";
        }

        // Tỷ lệ kích hoạt
        if (skill.procChance < 100)
        {
            stats += $"Tỷ lệ: {skill.procChance}%\n";
        }

        // Hệ số nhân
        if (skill.valueScale != 1.0f)
        {
            string scaleType = skill.scaleFromMaxHP ? "% Max HP" : "% ATK";
            stats += $"Sức mạnh: {skill.valueScale * 100}{scaleType}\n";
        }

        // Hồi chiêu
        if (skill.cooldownTurns > 0)
        {
            stats += $"Hồi chiêu: {skill.cooldownTurns} lượt\n";
        }

        // Ưu tiên
        if (skill.priority != 0)
        {
            stats += $"Ưu tiên: {skill.priority}\n";
        }

        return stats.Trim();
    }

    private void ClearPanel()
    {
        if (iconImg != null) iconImg.enabled = false;
        if (skillNameText != null) skillNameText.text = "";
        if (tierImg != null) tierImg.gameObject.SetActive(false);
        if (descriptionText != null) descriptionText.text = "";
        if (statsText != null) statsText.text = "";
    }

    // Gọi method này khi bạn muốn cập nhật lại mô tả (ví dụ: sau khi thay đổi chỉ số trong SO)
    public void RefreshDescription()
    {
        if (currentSkill != null)
        {
            UpdateSkillInfo(currentSkill);
        }
    }

#if UNITY_EDITOR
    // Tự động cập nhật khi thay đổi trong Editor
    private void OnValidate()
    {
        if (currentSkill != null && Application.isPlaying)
        {
            RefreshDescription();
        }
    }
#endif
}

