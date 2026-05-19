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
    public async void StartTestBattle()
    {
        if (PetTeamSelectionUI.Instance == null)
        {
            Debug.LogError("Không tìm thấy PetTeamSelectionUI!");
            return;
        }

        List<PetModel> allies = PetTeamSelectionUI.Instance.GetSelectedTeam();
        if (LoadingUI.Instance != null) LoadingUI.Instance.Show();

        // Xóa ID cũ để đảm bảo không bị nhận trùng quà của trận trước
        BattleDataStore.currentBattleLogId = null;

        try
        {
            var charData = ResourceManager.Instance.GetCharacterData();
            if (charData == null) 
            {
                Debug.LogError("Chưa có dữ liệu nhân vật!");
                if (LoadingUI.Instance != null) LoadingUI.Instance.Hide();
                return;
            }

            var parameters = new Dictionary<string, object> { 
                { "p_character_id", charData.id },
                { "p_stamina_cost", 0 }, // Miễn phí stamina khi test
                { "p_gold_reward", 0 },
                { "p_exp_reward", 0 },
                { "p_potential_items", new List<object>() } // Không rơi đồ khi test nhanh
            };
            
            // Gọi RPC start_battle để nhận về ID trận đấu
            var response = await SupabaseManager.Instance.Client.Rpc<string>("start_battle", parameters);
            
            if (string.IsNullOrEmpty(response))
            {
                Debug.LogError("Không thể tạo trận đấu (Có thể hết Thể lực)!");
                if (LoadingUI.Instance != null) LoadingUI.Instance.Hide();
                return;
            }

            // Lưu ID trận đấu vào kho để dùng lúc kết thúc
            BattleDataStore.currentBattleLogId = response;
            
            // Cập nhật lại UI ngay lập tức để người chơi thấy Stamina bị trừ
            if (ResourceManager.Instance != null) await ResourceManager.Instance.SyncWithServer();
            
            Debug.Log($"<color=cyan>[BATTLE]</color> Đã tạo trận đấu thành công: {response}");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lỗi khi bắt đầu trận: " + e.Message);
            if (LoadingUI.Instance != null) LoadingUI.Instance.Hide();
            return;
        }
        if (LoadingUI.Instance != null) LoadingUI.Instance.Hide();

        // 2. Tạo danh sách kẻ địch dựa trên số lượng (enemyCount)
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
