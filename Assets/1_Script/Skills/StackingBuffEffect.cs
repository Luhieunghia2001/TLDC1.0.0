using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "StackingBuffEffect", menuName = "Pet System/Effects/Stacking Buff")]
public class StackingBuffEffect : SkillEffect
{
    [Header("Cấu hình cộng dồn")]
    public string stackKey = "AtkStack"; // Định danh để phân biệt các loại cộng dồn khác nhau
    public int maxStacks = 5;

    [Header("Tỉ lệ tăng mỗi tầng (%)")]
    public float atkPhyAddPerStack = 0.05f;
    public float atkMagAddPerStack = 0f;
    public float defPhyAddPerStack = 0f;
    public float defMagAddPerStack = 0f;
    public float speedAddPerStack = 0f;

    [Header("Cài đặt")]
    public bool targetSelf = true;

    public override void Execute(BattlePet user, BattlePet target)
    {
        BattlePet recipient = targetSelf ? user : target;
        if (recipient == null) return;

        // Khởi tạo stack nếu chưa có
        if (!recipient.stackDict.ContainsKey(stackKey)) 
            recipient.stackDict[stackKey] = 0;

        if (recipient.stackDict[stackKey] < maxStacks)
        {
            recipient.stackDict[stackKey]++;
            
            // Tăng chỉ số dựa trên giá trị gốc của Pet (hoặc giá trị hiện tại tùy bạn muốn)
            recipient.stats.AtkPhy += Mathf.RoundToInt(recipient.stats.AtkPhy * atkPhyAddPerStack);
            recipient.stats.AtkMag += Mathf.RoundToInt(recipient.stats.AtkMag * atkMagAddPerStack);
            recipient.stats.DefPhy += Mathf.RoundToInt(recipient.stats.DefPhy * defPhyAddPerStack);
            recipient.stats.DefMag += Mathf.RoundToInt(recipient.stats.DefMag * defMagAddPerStack);
            recipient.stats.Speed += Mathf.RoundToInt(recipient.stats.Speed * speedAddPerStack);

            Debug.Log($"[STACK] {recipient.petData.petName} cộng dồn {stackKey}: {recipient.stackDict[stackKey]}/{maxStacks}");
        }
        else
        {
            Debug.Log($"[STACK] {recipient.petData.petName} đã đạt giới hạn cộng dồn {stackKey}.");
        }
    }

    public override string GetDescription()
    {
        string desc = $"Cộng dồn {stackKey} (tối đa {maxStacks} tầng): ";
        var parts = new System.Collections.Generic.List<string>();
        
        if (atkPhyAddPerStack > 0) parts.Add($"+{atkPhyAddPerStack * 100}% ATK Phý/tầng");
        if (atkMagAddPerStack > 0) parts.Add($"+{atkMagAddPerStack * 100}% ATK Phép/tầng");
        if (defPhyAddPerStack > 0) parts.Add($"+{defPhyAddPerStack * 100}% DEF Phý/tầng");
        if (defMagAddPerStack > 0) parts.Add($"+{defMagAddPerStack * 100}% DEF Phép/tầng");
        if (speedAddPerStack > 0) parts.Add($"+{speedAddPerStack * 100}% Tốc độ/tầng");
        
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
