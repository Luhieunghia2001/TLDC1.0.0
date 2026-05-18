using UnityEngine;

[CreateAssetMenu(fileName = "ScalableDamageEffect", menuName = "Pet System/Effects/Scalable Damage")]
public class ScalableDamageEffect : SkillEffect
{
    [Header("Hệ số nhân sát thương (%)")]
    public float atkPhyMult = 0f;
    public float atkMagMult = 0f;
    public float defPhyMult = 0f;
    public float defMagMult = 0f;
    public float speedMult = 0f;

    [Header("Hệ số nhân theo Máu (%)")]
    public float userMaxHpMult = 0f;
    public float userCurrentHpMult = 0f;
    public float targetMaxHpMult = 0f;
    public float targetCurrentHpMult = 0f;

    [Header("Cài đặt")]
    public bool isTrueDamage = false;
    public bool targetSelf = false;

    public override void Execute(BattlePet user, BattlePet target)
    {
        BattlePet recipient = targetSelf ? user : target;
        if (user == null || recipient == null) return;

        // 1. Tính toán tổng lực tấn công dựa trên các hệ số gán vào
        float totalPower = (user.stats.AtkPhy * atkPhyMult)
                         + (user.stats.AtkMag * atkMagMult)
                         + (user.stats.DefPhy * defPhyMult)
                         + (user.stats.DefMag * defMagMult)
                         + (user.stats.Speed * speedMult)
                         + (user.stats.HP * userMaxHpMult)
                         + (user.currentHP * userCurrentHpMult)
                         + (recipient.stats.HP * targetMaxHpMult)
                         + (recipient.currentHP * targetCurrentHpMult);

        int finalDamage = 0;

        if (isTrueDamage)
        {
            // Sát thương chuẩn: Không trừ thủ
            finalDamage = Mathf.RoundToInt(Mathf.Max(1, totalPower));
        }
        else
        {
            // Sát thương thông thường: Trừ đi phòng thủ tương ứng của kẻ địch
            float targetDef = (recipient.stats.DefPhy + recipient.stats.DefMag) / 2f;
            finalDamage = Mathf.RoundToInt(Mathf.Max(1, totalPower - (targetDef * 0.5f)));
        }

        // 2. Trừ máu mục tiêu
        recipient.TakeDamage(finalDamage);

        Debug.Log($"[Scalable Damage] {user.petData.petName} gây {finalDamage} sát thương lên {recipient.petData.petName}.");
    }

    public override string GetDescription()
    {
        string desc = "Sát thương scalable: ";
        var parts = new System.Collections.Generic.List<string>();
        
        if (atkPhyMult > 0) parts.Add($"{atkPhyMult * 100}% ATK Phý");
        if (atkMagMult > 0) parts.Add($"{atkMagMult * 100}% ATK Phép");
        if (defPhyMult > 0) parts.Add($"{defPhyMult * 100}% DEF Phý");
        if (defMagMult > 0) parts.Add($"{defMagMult * 100}% DEF Phép");
        if (speedMult > 0) parts.Add($"{speedMult * 100}% Tốc độ");
        if (userMaxHpMult > 0) parts.Add($"{userMaxHpMult * 100}% Max HP bản thân");
        if (targetMaxHpMult > 0) parts.Add($"{targetMaxHpMult * 100}% Max HP mục tiêu");
        
        if (parts.Count > 0)
        {
            desc += string.Join(", ", parts);
        }
        else
        {
            desc += "Không có hệ số";
        }
        
        if (isTrueDamage) desc += " (Sát thương chuẩn)";
        
        string targetText = targetSelf ? "bản thân" : "mục tiêu";
        desc += $" lên {targetText}";
        
        return desc;
    }
}
