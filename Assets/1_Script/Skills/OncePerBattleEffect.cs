using UnityEngine;

[CreateAssetMenu(fileName = "OncePerBattleEffect", menuName = "Pet System/Effects/Once Per Battle Damage")]
public class OncePerBattleEffect : SkillEffect
{
    [Header("Cấu hình sát thương (%)")]
    public float maxHpPercent = 0.5f; 
    public float currentHpPercent = 0f; 
    
    [Header("Cài đặt")]
    public string uniqueEffectKey = "FirstStrike50"; 
    public bool targetSelf = false;

    public override void Execute(BattlePet user, BattlePet target)
    {
        BattlePet recipient = targetSelf ? user : target;
        if (recipient == null) return;

        // Nếu mục tiêu đã bị dính key này rồi thì bỏ qua
        if (recipient.stackDict.ContainsKey(uniqueEffectKey)) 
        {
            return;
        }

        // Tính sát thương
        float damage = (recipient.stats.HP * maxHpPercent) + (recipient.currentHP * currentHpPercent);
        int finalDamage = Mathf.RoundToInt(damage);

        // Gây sát thương chuẩn
        recipient.TakeDamage(finalDamage);

        // Đánh dấu đã sử dụng
        recipient.stackDict[uniqueEffectKey] = 1;

        Debug.Log($"[ONCE] {recipient.petData.petName} bị giảm {finalDamage} máu! (Chỉ 1 lần duy nhất)");
    }
}
