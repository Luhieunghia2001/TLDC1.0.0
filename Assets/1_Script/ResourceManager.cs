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
        SyncWithServer(); // Đồng bộ ngay khi vào game
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
            SyncWithServer();
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

                // Cập nhật dữ liệu từ Server về Client
                currentCharacter.energy = serverData.energy;
                currentCharacter.stamina = serverData.stamina;
                currentCharacter.lastRegenTime = serverData.lastRegenTime;
                
                Debug.Log($"<color=yellow>[Server Sync]</color> Energy: {currentCharacter.energy}, Stamina: {currentCharacter.stamina}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Lỗi đồng bộ Resource: " + e.Message);
        }
    }

    // Khi người chơi thực hiện hành động tốn Energy, bạn cũng nên gọi một SQL Function khác
    // để trừ tiền trên Server thay vì trừ ở Client.
}
