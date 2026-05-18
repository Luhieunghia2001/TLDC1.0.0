using UnityEngine;

public abstract class SkillEffect : ScriptableObject
{
    // User: Người dùng chiêu | Target: Đối thủ trúng chiêu
    public abstract void Execute(BattlePet user, BattlePet target);
    
    // Trả về mô tả của hiệu ứng
    public abstract string GetDescription();
}
