using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DungeonConfig", menuName = "Dungeon/Config")]
public class DungeonConfigSO : ScriptableObject
{
    [Header("Dungeon Info")]
    public string dungeonID;
    public string dungeonName;
    public int staminaCost = 10;
    public int maxDailyAttempts = 10; 

    [Header("Enemy Configuration")]
    public List<EnemyPetConfig> enemyTeam;

    [Header("Rewards")]
    public int goldReward;
    public int expReward;
    public List<ItemReward> potentialDrops;
}

[System.Serializable]
public struct ItemReward
{
    public ItemBaseSO item;
    public int quantity;
    [Range(0, 100)] public int dropChance; // Chuyển sang thang điểm 100 (Phần trăm %)
}

[System.Serializable]
public struct EnemyPetConfig
{
    public string petBaseId; // ID để tra cứu trong PetManager
    public int level;
    public int star;
    public int realm;
}