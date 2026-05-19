using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [Header("Settings")]
    public float syncInterval = 10f; // Cứ 10 giây đồng bộ với server 1 lần

    private CharacterModel currentCharacter;

    // Sự kiện được kích hoạt khi dữ liệu nhân vật được cập nhật từ server
    public event Action<CharacterModel> OnCharacterDataUpdated;
    private float timer = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public CharacterModel GetCharacterData() => currentCharacter;

    public void Initialize(CharacterModel character)
    {
        currentCharacter = character;
        _ = SyncWithServer(); // Đồng bộ ngay khi vào game (Dùng _ để bỏ qua warning await)
    }

    private void Update()
    {
        if (currentCharacter == null) return;

        // Ở Client, chúng ta có thể làm hiệu ứng tăng số "giả" để người chơi thấy mượt
        // Nhưng cứ sau syncInterval, chúng ta sẽ lấy con số chuẩn từ Server về
        timer += Time.deltaTime;
        if (timer >= syncInterval)
        {
            timer = 0f;
            _ = SyncWithServer();
        }
    }

    public async Task SyncWithServer()
    {
        try
        {
            // Gọi SQL Function 'sync_resources' trên Supabase
            var parameters = new Dictionary<string, object> { { "p_id", currentCharacter.id } };
            
            // Lưu ý: RPC trả về SETOF nên mình phải nhận dạng List
            var response = await SupabaseManager.Instance.Client.Rpc<List<CharacterModel>>("sync_resources", parameters);

            if (response != null && response.Count > 0)
            {
                var serverData = response[0]; // Lấy phần tử đầu tiên trong danh sách

                // Log kiểm tra dữ liệu trả về từ RPC sync_resources
                Debug.Log($"<color=yellow>[ResourceManager Sync]</color> Server trả về cho ID {serverData.id}: " +
                          $"EXP={serverData.current_exp}, Level={serverData.level}, Gold={serverData.gold}, " +
                          $"Stamina={serverData.stamina}");

                // Cập nhật dữ liệu từ Server về Client
                currentCharacter.energy = serverData.energy;
                currentCharacter.stamina = serverData.stamina;
                currentCharacter.last_regen_time = serverData.last_regen_time;
                currentCharacter.gold = serverData.gold;
                currentCharacter.diamond = serverData.diamond;
                currentCharacter.level = serverData.level;
                currentCharacter.current_exp = serverData.current_exp;

                // Kích hoạt sự kiện để các UI khác có thể cập nhật
                OnCharacterDataUpdated?.Invoke(currentCharacter);
                
                //Debug.Log($"<color=yellow>[Server Sync]</color> Gold: {currentCharacter.gold}, Energy: {currentCharacter.energy}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Lỗi đồng bộ Resource: " + e.Message);
        }
    }

    // Nhận thưởng sau trận đấu (Bảo mật)
    public async Task ClaimBattleReward(string battleLogId)
    {
        if (currentCharacter == null || string.IsNullOrEmpty(battleLogId)) return;

        Debug.Log($"<color=cyan>[REWARD]</color> Char: {currentCharacter.id} | Battle: {battleLogId}");

        if (LoadingUI.Instance != null) LoadingUI.Instance.Show();
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "p_character_id", currentCharacter.id },
                { "p_battle_log_id", battleLogId }
            };

            // Gọi hàm claim_reward trên server để kiểm tra và cộng tiền
            await SupabaseManager.Instance.Client.Rpc("claim_reward", parameters);
            Debug.Log("<color=green>Nhận thưởng thành công từ Server!</color>");
            
            // Sau khi nhận thưởng, đồng bộ lại dữ liệu mới nhất
            await SyncWithServer();
        }
        catch (Exception e)
        {
            Debug.LogError("Lỗi khi nhận thưởng bảo mật: " + e.Message);
        }
        if (LoadingUI.Instance != null) LoadingUI.Instance.Hide();
    }
}
