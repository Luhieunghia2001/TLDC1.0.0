using UnityEngine;

[CreateAssetMenu(fileName = "DamageEffect", menuName = "GameData/SkillEffects/Damage")]
public class DamageEffect : SkillEffect
{
    public float multiplier = 1.0f; // Hệ số nhân sát thương

    public override void Execute(BattlePet user, BattlePet target)
    {
        float atk = (user.baseData.attackType == PetAttackType.Physical) ? user.stats.AtkPhy : user.stats.AtkMag;
        float def = (user.baseData.attackType == PetAttackType.Physical) ? target.stats.DefPhy : target.stats.DefMag;
        
        float rawDamage = atk * multiplier;
        int finalDamage = Mathf.Max(1, Mathf.RoundToInt(rawDamage - (def * 0.5f)));

        target.currentHP -= finalDamage;
        if (target.currentHP < 0) target.currentHP = 0;

        Debug.Log($"[SKILL] {user.petData.petName} gây {finalDamage} sát thương lên {target.petData.petName}");
    }
}
