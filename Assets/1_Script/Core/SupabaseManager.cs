using UnityEngine;
using Supabase;
using System.Threading.Tasks;

public class SupabaseManager : MonoBehaviour
{
    public static SupabaseManager Instance { get; private set; }

    [Header("Supabase Configuration")]
    [SerializeField] private string supabaseUrl = "https://kqatkzvkuwoosrwvgqki.supabase.co";
    [SerializeField] private string supabaseKey = "sb_publishable_TSBTSul_1kgxOOnpCaW1sA_44low4pd";

    public Client Client { get; private set; }

    private async void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            await InitializeSupabase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async Task InitializeSupabase()
    {
        try
        {
            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true
            };

            Client = new Client(supabaseUrl, supabaseKey, options);
            await Client.InitializeAsync();

            Debug.Log("<color=green><b>[Supabase]</b> Kết nối thành công!</color>");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"<color=red><b>[Supabase]</b> Lỗi khởi tạo: {e.Message}</color>");
        }
    }
}
