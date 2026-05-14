using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PetBattleTest : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string battleSceneName = "BattleScene"; // Tên Scene trận đấu của bạn

    [Header("Enemy Test Settings")]
    [SerializeField] private string enemyPetBaseID = "pet_01";
    [SerializeField] private int enemyLevel = 10;
    [SerializeField] private int enemyStar = 1;
    [SerializeField] private string enemyTier = "D";

    [ContextMenu("Start Test Battle")]
    public void StartTestBattle()
    {
        // 1. Lấy đội hình từ UI
        if (PetTeamSelectionUI.Instance == null)
        {
            Debug.LogError("Không tìm thấy PetTeamSelectionUI trong Scene hiện tại!");
            return;
        }

        List<PetModel> allies = PetTeamSelectionUI.Instance.GetSelectedTeam();
        if (allies == null || allies.Count == 0)
        {
            Debug.LogWarning("Đội hình của bạn đang trống! Hãy chọn Pet trước.");
            return;
        }

        // 2. Lưu vào kho lưu trữ trung gian
        BattleDataStore.selectedAllies = new List<PetModel>(allies);
        
        List<PetModel> enemies = new List<PetModel>();
        enemies.Add(new PetModel {
            petBaseId = enemyPetBaseID,
            petName = "Test Enemy",
            level = enemyLevel,
            star = enemyStar,
            tier = enemyTier
        });
        BattleDataStore.selectedEnemies = enemies;

        // 3. Chuyển Scene
        Debug.Log($"<color=cyan>[TEST]</color> Đang chuyển sang Scene: {battleSceneName}...");
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
