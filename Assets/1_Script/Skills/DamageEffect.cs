using UnityEngine;

[CreateAssetMenu(fileName = "DamageEffect", menuName = "GameData/SkillEffects/Damage")]
public class DamageEffect : SkillEffect
{
    public float multiplier = 1.0f; // Hệ số nhân sát thương
    public bool targetSelf = false; // Mặc định là false để đánh kẻ địch

    public override void Execute(BattlePet user, BattlePet target)
    {
        BattlePet recipient = targetSelf ? user : target;
        if (user == null || recipient == null) return;

        float atk = (user.baseData.attackType == PetAttackType.Physical) ? user.stats.AtkPhy : user.stats.AtkMag;
        float def = (user.baseData.attackType == PetAttackType.Physical) ? recipient.stats.DefPhy : recipient.stats.DefMag;
        
        float rawDamage = atk * multiplier;
        int finalDamage = Mathf.Max(1, Mathf.RoundToInt(rawDamage - (def * 0.5f)));

        recipient.currentHP -= finalDamage;
        if (recipient.currentHP < 0) recipient.currentHP = 0;

        Debug.Log($"[SKILL] {user.petData.petName} gây {finalDamage} sát thương lên {recipient.petData.petName}");
    }
}
