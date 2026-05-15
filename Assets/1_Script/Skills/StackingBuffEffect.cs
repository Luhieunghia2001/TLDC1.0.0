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

    public override void Execute(BattlePet user, BattlePet target)
    {
        BattlePet recipient = target != null ? target : user;
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
}
