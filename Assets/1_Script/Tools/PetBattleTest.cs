using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PetBattleTest : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string battleSceneName = "BattleScene"; 

    [Header("Enemy Settings")]
    [SerializeField] private string enemyPetBaseID = "pet_01";
    [SerializeField] private int enemyLevel = 10;
    [SerializeField] private int enemyCount = 3; // Số lượng quái bạn muốn xuất hiện

    [ContextMenu("Start Test Battle")]
    public void StartTestBattle()
    {
        if (PetTeamSelectionUI.Instance == null)
        {
            Debug.LogError("Không tìm thấy PetTeamSelectionUI!");
            return;
        }

        List<PetModel> allies = PetTeamSelectionUI.Instance.GetSelectedTeam();
        if (allies == null || allies.Count == 0)
        {
            Debug.LogWarning("Hãy chọn Pet cho người chơi trước!");
            return;
        }

        // Tạo danh sách kẻ địch dựa trên số lượng (enemyCount)
        List<PetModel> enemies = new List<PetModel>();
        for (int i = 0; i < enemyCount; i++)
        {
            enemies.Add(new PetModel {
                petBaseId = enemyPetBaseID,
                petName = "Wild " + enemyPetBaseID + " #" + (i + 1),
                level = enemyLevel,
                star = 1,
                tier = "D"
            });
        }

        // Lưu vào kho
        BattleDataStore.selectedAllies = new List<PetModel>(allies);
        BattleDataStore.selectedEnemies = enemies;

        // Chuyển Scene
        SceneManager.LoadScene(battleSceneName);
    }

    void Update()
    {
        bool startRequested = false;
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame) startRequested = true;
#else
        if (Input.GetKeyDown(KeyCode.B)) startRequested = true;
#endif
        if (startRequested) StartTestBattle();
    }
}
