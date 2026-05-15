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
}
