using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance { get; private set; }

    [Header("Dungeon Database")]
    [SerializeField] private List<DungeonConfigSO> allDungeons;

    private DungeonConfigSO currentDungeon;
    private Dictionary<string, DungeonConfigSO> dungeonDictionary;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDungeonDictionary();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeDungeonDictionary()
    {
        dungeonDictionary = new Dictionary<string, DungeonConfigSO>();
        foreach (var dungeon in allDungeons)
        {
            if (dungeon != null && !string.IsNullOrEmpty(dungeon.dungeonID))
            {
                dungeonDictionary[dungeon.dungeonID] = dungeon;
            }
        }
    }

    public DungeonConfigSO GetDungeonByID(string dungeonID)
    {
        if (dungeonDictionary.TryGetValue(dungeonID, out var dungeon))
        {
            return dungeon;
        }
        Debug.LogWarning($"Không tìm thấy phó bản với ID: {dungeonID}");
        return null;
    }

    public List<DungeonConfigSO> GetAllDungeons()
    {
        return allDungeons;
    }

    public void SetCurrentDungeon(DungeonConfigSO dungeon)
    {
        currentDungeon = dungeon;
        Debug.Log($"Đã chọn phó bản: {dungeon.dungeonName}");
    }

    public DungeonConfigSO GetCurrentDungeon()
    {
        return currentDungeon;
    }

    public bool CanEnterDungeon(DungeonConfigSO dungeon)
    {
        // TODO: Kiểm tra energy, level, điều kiện khác
        return dungeon != null;
    }

    public void StartBattle(DungeonConfigSO dungeon)
    {
        if (!CanEnterDungeon(dungeon))
        {
            Debug.LogWarning("Không thể vào phó bản!");
            return;
        }

        Debug.Log($"Bắt đầu battle với phó bản: {dungeon.dungeonName}");
        Debug.Log($"Enemy count: {dungeon.enemyTeam.Count}");
        Debug.Log($"Stamina cost: {dungeon.staminaCost}");
        Debug.Log($"Gold reward: {dungeon.goldReward}");

        // TODO: Trừ energy, load battle scene, truyền enemy pets
        // SceneManager.LoadScene("BattleScene");
    }

    public async Task CompleteBattle(DungeonConfigSO dungeon, bool victory)
    {
        if (dungeon == null) return;

        if (victory)
        {
            Debug.Log($"<color=green>[Dungeon]</color> Hoàn thành phó bản {dungeon.dungeonName}!");
            foreach (var reward in dungeon.potentialDrops)
            {
                if (reward.item == null) continue;
            }
        }
        else
        {
            Debug.Log($"Thất bại phó bản {dungeon.dungeonName}");
        }
    }
}
