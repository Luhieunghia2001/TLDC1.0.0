using UnityEngine;

public enum SkillType { Active, Passive }

public enum SkillTrigger
{
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

[CreateAssetMenu(fileName = "New Skill", menuName = "Pet System/Skill")]
public class PetSkillSO : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    public string skillID;
    public string skillName;
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

    [Header("Visuals")]
    public string animationTrigger = "Attack";
    public GameObject vfxPrefab;
}
