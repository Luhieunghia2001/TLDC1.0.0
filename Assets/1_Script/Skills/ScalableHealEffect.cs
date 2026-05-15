using UnityEngine;

[CreateAssetMenu(fileName = "ScalableHealEffect", menuName = "Pet System/Effects/Scalable Heal")]
public class ScalableHealEffect : SkillEffect
{
    [Header("Hệ số hồi phục (%)")]
    public float maxHpMult = 0f;
    public float currentHpMult = 0f;
    public float flatAmount = 0f;

    public override void Execute(BattlePet user, BattlePet target)
    {
        // Với Skill hồi máu, thường user chính là target (tự hồi cho mình)
        BattlePet recipient = target != null ? target : user;
        if (recipient == null) return;

        float healAmount = (recipient.stats.HP * maxHpMult) 
                         + (recipient.currentHP * currentHpMult) 
                         + flatAmount;

        int finalHeal = Mathf.RoundToInt(healAmount);
        recipient.currentHP += finalHeal;
        
        if (recipient.currentHP > recipient.stats.HP) 
            recipient.currentHP = recipient.stats.HP;

        Debug.Log($"[HEAL] {recipient.petData.petName} được hồi {finalHeal} HP.");
    }
}
