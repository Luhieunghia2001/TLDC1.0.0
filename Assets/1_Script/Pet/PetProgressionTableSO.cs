using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Progression Table", menuName = "GameData/Pet Progression Table")]
public class PetProgressionTableSO : ScriptableObject
{
    [Header("Bảng Nâng Sao (Star Up)")]
    [Tooltip("Phần tử 0 = Lên 2 sao, Phần tử 1 = Lên 3 sao...")]
    public List<ProgressionNode> starNodes;

    [Header("Bảng Thăng Tầng (Realm/Breakthrough)")]
    [Tooltip("Phần tử 0 = Lên Tầng 2, Phần tử 1 = Lên Tầng 3...")]
    public List<ProgressionNode> realmNodes;

    // Hàm tiện ích để UI tự động lấy thông tin chi phí
    public ProgressionNode GetStarCost(int currentStar)
    {
        // Ví dụ: Đang 1 sao -> Lấy phần tử số 0 (Chi phí để lên 2 sao)
        if (currentStar >= 1 && currentStar <= starNodes.Count)
            return starNodes[currentStar - 1];
        return null;
    }

    public ProgressionNode GetRealmCost(int currentRealm)
    {
        if (currentRealm >= 1 && currentRealm <= realmNodes.Count)
            return realmNodes[currentRealm - 1];
        return null;
    }
}

[System.Serializable]
public class ProgressionNode
{
    [Header("Chi phí nâng cấp")]
    public int goldCost;
    public List<ItemRequirement> requiredItems;

    [Header("Chỉ số cộng thẳng (Tùy chọn)")]
    public int bonusHP;
    public int bonusAtk;
    public int bonusDef;

    [Header("Chỉ số nhân thêm (VD: 1.2 = Tăng 20% Base)")]
    public float hpMultiplier = 1.0f;
    public float atkMultiplier = 1.0f;
    public float defMultiplier = 1.0f;
}

[System.Serializable]
public class ItemRequirement
{
    public ItemBaseSO item;
    public int quantity;
}
