using UnityEngine;

[CreateAssetMenu(fileName = "ScalableHealEffect", menuName = "Pet System/Effects/Scalable Heal")]
public class ScalableHealEffect : SkillEffect
{
    [Header("Hệ số hồi phục (%)")]
    public float maxHpMult = 0f;
    public float currentHpMult = 0f;
    public float flatAmount = 0f;

    [Header("Cài đặt")]
    public bool targetSelf = true; // Nếu true, sẽ hồi máu cho chính mình. Nếu false, hồi cho mục tiêu.

    public override void Execute(BattlePet user, BattlePet target)
    {
        // Xác định người nhận máu
        BattlePet recipient = targetSelf ? user : target;
        if (recipient == null) return;

        float healAmount = (recipient.stats.HP * maxHpMult) 
                         + (recipient.currentHP * currentHpMult) 
                         + flatAmount;

        int finalHeal = Mathf.RoundToInt(healAmount);
        recipient.Heal(finalHeal);

        Debug.Log($"[HEAL] {recipient.petData.petName} được hồi {finalHeal} HP.");
    }
}
