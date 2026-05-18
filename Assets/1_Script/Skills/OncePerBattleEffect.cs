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

    public override string GetDescription()
    {
        string desc = "Sát thương 1 lần/trận: ";
        var parts = new System.Collections.Generic.List<string>();
        
        if (maxHpPercent > 0) parts.Add($"{maxHpPercent * 100}% Max HP");
        if (currentHpPercent > 0) parts.Add($"{currentHpPercent * 100}% HP hiện tại");
        
        if (parts.Count > 0)
        {
            desc += string.Join(", ", parts);
        }
        else
        {
            desc += "Không có hệ số";
        }
        
        string targetText = targetSelf ? "bản thân" : "mục tiêu";
        desc += $" lên {targetText} (chỉ kích hoạt 1 lần)";
        
        return desc;
    }
}
