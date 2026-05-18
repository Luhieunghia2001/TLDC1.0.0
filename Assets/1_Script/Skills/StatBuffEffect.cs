using UnityEngine;

[CreateAssetMenu(fileName = "StatBuffEffect", menuName = "Pet System/Effects/Stat Buff")]
public class StatBuffEffect : SkillEffect
{
    [Header("Tỉ lệ tăng chỉ số (%)")]
    public float atkPhyAddPercent = 0f;
    public float atkMagAddPercent = 0f;
    public float defPhyAddPercent = 0f;
    public float defMagAddPercent = 0f;
    public float speedAddPercent = 0f;

    [Header("Cài đặt")]
    public bool targetSelf = true;

    public override void Execute(BattlePet user, BattlePet target)
    {
        BattlePet recipient = targetSelf ? user : target;
        if (recipient == null) return;

        // Tăng chỉ số trực tiếp vào stats hiện tại trong trận đấu
        recipient.stats.AtkPhy += Mathf.RoundToInt(recipient.stats.AtkPhy * atkPhyAddPercent);
        recipient.stats.AtkMag += Mathf.RoundToInt(recipient.stats.AtkMag * atkMagAddPercent);
        recipient.stats.DefPhy += Mathf.RoundToInt(recipient.stats.DefPhy * defPhyAddPercent);
        recipient.stats.DefMag += Mathf.RoundToInt(recipient.stats.DefMag * defMagAddPercent);
        recipient.stats.Speed += Mathf.RoundToInt(recipient.stats.Speed * speedAddPercent);

        Debug.Log($"[BUFF] {recipient.petData.petName} đã được tăng chỉ số: Atk +{atkPhyAddPercent}%, Speed +{speedAddPercent}%...");
    }

    public override string GetDescription()
    {
        string desc = "Tăng chỉ số: ";
        var parts = new System.Collections.Generic.List<string>();
        
        if (atkPhyAddPercent > 0) parts.Add($"+{atkPhyAddPercent * 100}% ATK Phý");
        if (atkMagAddPercent > 0) parts.Add($"+{atkMagAddPercent * 100}% ATK Phép");
        if (defPhyAddPercent > 0) parts.Add($"+{defPhyAddPercent * 100}% DEF Phý");
        if (defMagAddPercent > 0) parts.Add($"+{defMagAddPercent * 100}% DEF Phép");
        if (speedAddPercent > 0) parts.Add($"+{speedAddPercent * 100}% Tốc độ");
        
        if (parts.Count > 0)
        {
            desc += string.Join(", ", parts);
        }
        else
        {
            desc += "Không có hệ số";
        }
        
        string targetText = targetSelf ? "bản thân" : "mục tiêu";
        desc += $" cho {targetText}";
        
        return desc;
    }
}
