using UnityEngine;
using System.Collections.Generic;

public enum SkillType { Active, Passive }

public enum SkillTrigger
{
    None,           // Không dùng trigger tự động (Dành cho chiêu Active)
    OnTurnStart,    // Bắt đầu lượt của mình
    OnAttack,       // Khi mình tấn công (Chủ động)
    OnAttacked,     // Khi bị kẻ địch tấn công (Bị động)
    OnKill,         // Khi giết được địch
    OnDeath,        // Khi mình bị hạ gục
    OnWaveStart     // Khi bắt đầu trận đấu
}

public enum SkillEffectType
{
    Damage,         // Gây sát thương
    Heal,           // Hồi máu
    Buff,           // Tăng chỉ số
    Debuff,         // Giảm chỉ số
    Shield          // Tạo giáp
}

public enum SkillTier { D, C, B, A, S, SS, SSS }

[CreateAssetMenu(fileName = "New Skill", menuName = "Pet System/Skill")]
public class PetSkillSO : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    public string skillID;
    public string skillName;
    public SkillTier tier = SkillTier.C;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Cấu hình Logic")]
    public SkillType skillType = SkillType.Active;
    public SkillTrigger trigger = SkillTrigger.OnTurnStart;
    public SkillEffectType effectType = SkillEffectType.Damage;

    [Header("Tỷ lệ & Chỉ số")]
    [Range(0, 100)]
    public float procChance = 100f; // Tỷ lệ kích hoạt (%)
    
    public float valueScale = 1.0f; // Hệ số nhân (VD: 1.2 = 120% ATK)
    public bool scaleFromMaxHP = false; // Nếu true, dùng % máu tối đa thay vì ATK

    [Header("Hồi chiêu (Dành cho Active)")]
    public int cooldownTurns = 0;
    public int priority = 0; // Độ ưu tiên sử dụng trong Auto (Số lớn ưu tiên trước)

    [Header("Skill Effects (New System)")]
    public List<SkillEffect> effects; // Danh sách các hiệu ứng của chiêu thức

    [Header("Visuals")]
    public string animationTrigger = "Attack";
    public GameObject vfxPrefab;

    // Tạo mô tả động dựa trên các chỉ số hiện tại
    public string GetFormattedDescription()
    {
        if (string.IsNullOrEmpty(description))
        {
            return GenerateAutoDescription();
        }
        return description;
    }

    private string GenerateAutoDescription()
    {
        string desc = "";

        // Thêm thông tin về trigger
        if (trigger != SkillTrigger.None)
        {
            desc += $"Kích hoạt khi: {GetTriggerName(trigger)}\n";
        }

        // Thêm thông tin về hiệu ứng
        desc += $"Hiệu ứng: {GetEffectTypeName(effectType)}\n";

        // Thêm thông tin về tỷ lệ
        if (procChance < 100)
        {
            desc += $"Tỷ lệ kích hoạt: {procChance}%\n";
        }

        // Thêm thông tin về giá trị
        if (valueScale != 1.0f)
        {
            string scaleType = scaleFromMaxHP ? "% Max HP" : "% ATK";
            desc += $"Sức mạnh: {valueScale * 100}{scaleType}\n";
        }

        // Thêm thông tin về hồi chiêu
        if (cooldownTurns > 0)
        {
            desc += $"Hồi chiêu: {cooldownTurns} lượt\n";
        }

        // Thêm thông tin về các effects
        if (effects != null && effects.Count > 0)
        {
            desc += $"\nHiệu ứng bổ sung ({effects.Count}):\n";
            foreach (var effect in effects)
            {
                if (effect != null)
                {
                    desc += $"- {effect.GetDescription()}\n";
                }
            }
        }

        return desc.Trim();
    }

    private string GetTriggerName(SkillTrigger trigger)
    {
        switch (trigger)
        {
            case SkillTrigger.OnTurnStart: return "Bắt đầu lượt";
            case SkillTrigger.OnAttack: return "Khi tấn công";
            case SkillTrigger.OnAttacked: return "Khi bị tấn công";
            case SkillTrigger.OnKill: return "Khi giết địch";
            case SkillTrigger.OnDeath: return "Khi bị hạ gục";
            case SkillTrigger.OnWaveStart: return "Bắt đầu trận đấu";
            default: return "Không";
        }
    }

    private string GetEffectTypeName(SkillEffectType type)
    {
        switch (type)
        {
            case SkillEffectType.Damage: return "Gây sát thương";
            case SkillEffectType.Heal: return "Hồi máu";
            case SkillEffectType.Buff: return "Tăng chỉ số";
            case SkillEffectType.Debuff: return "Giảm chỉ số";
            case SkillEffectType.Shield: return "Tạo giáp";
            default: return "Không xác định";
        }
    }
}
