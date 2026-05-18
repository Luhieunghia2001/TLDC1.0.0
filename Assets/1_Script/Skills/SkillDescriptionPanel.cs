using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillDescriptionPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImg;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI tierText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI statsText;

    [Header("Tier Colors")]
    [SerializeField] private Color tierDColor = Color.gray;
    [SerializeField] private Color tierCColor = Color.white;
    [SerializeField] private Color tierBColor = new Color(0.3f, 0.6f, 1f);
    [SerializeField] private Color tierAColor = new Color(0.3f, 1f, 0.5f);
    [SerializeField] private Color tierSColor = new Color(1f, 0.8f, 0.2f);
    [SerializeField] private Color tierSSColor = new Color(1f, 0.4f, 0.4f);
    [SerializeField] private Color tierSSSColor = new Color(1f, 0.2f, 0.8f);

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

        // Hiển thị tier
        if (tierText != null)
        {
            tierText.text = skill.tier.ToString();
            tierText.color = GetTierColor(skill.tier);
        }

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

    private Color GetTierColor(SkillTier tier)
    {
        switch (tier)
        {
            case SkillTier.D: return tierDColor;
            case SkillTier.C: return tierCColor;
            case SkillTier.B: return tierBColor;
            case SkillTier.A: return tierAColor;
            case SkillTier.S: return tierSColor;
            case SkillTier.SS: return tierSSColor;
            case SkillTier.SSS: return tierSSSColor;
            default: return Color.white;
        }
    }

    private void ClearPanel()
    {
        if (iconImg != null) iconImg.enabled = false;
        if (skillNameText != null) skillNameText.text = "";
        if (tierText != null) tierText.text = "";
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
