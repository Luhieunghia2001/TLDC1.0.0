using UnityEngine;

[CreateAssetMenu(fileName = "OncePerBattleEffect", menuName = "Pet System/Effects/Once Per Battle Damage")]
public class OncePerBattleEffect : SkillEffect
{
    [Header("Cấu hình sát thương (%)")]
    public float maxHpPercent = 0.5f; 
    public float currentHpPercent = 0f; 
    
    [Header("Định danh")]
    [Tooltip("Dùng key này để đảm bảo mỗi Skill chỉ kích hoạt 1 lần duy nhất trên 1 mục tiêu")]
    public string uniqueEffectKey = "FirstStrike50"; 

    public override void Execute(BattlePet user, BattlePet target)
    {
        if (target == null) return;

        // Nếu mục tiêu đã bị dính key này rồi thì bỏ qua
        if (target.stackDict.ContainsKey(uniqueEffectKey)) 
        {
            return;
        }

        // Tính sát thương
        float damage = (target.stats.HP * maxHpPercent) + (target.currentHP * currentHpPercent);
        int finalDamage = Mathf.RoundToInt(damage);

        // Gây sát thương chuẩn
        target.currentHP -= finalDamage;
        if (target.currentHP < 0) target.currentHP = 0;

        // Đánh dấu đã sử dụng
        target.stackDict[uniqueEffectKey] = 1;

        Debug.Log($"[ONCE] {target.petData.petName} bị giảm {finalDamage} máu! (Chỉ 1 lần duy nhất)");
    }
}
