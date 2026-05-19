using UnityEngine;
using System.Threading.Tasks;
using Supabase.Gotrue;
using Postgrest.Attributes;
using Postgrest.Models;
using UnityEngine.SceneManagement;

// Model cho bảng 'account' (Tài khoản tổng)
[Table("account")]
public class AccountModel : BaseModel
{
    [PrimaryKey("id", false)] 
    [Column("id")]
    public string id { get; set; }

    [Column("username")]
    public string username { get; set; }
}

// Model cho bảng 'players' (Nhân vật trong game)
[Table("players")]
public class CharacterModel : BaseModel
{
    [PrimaryKey("id", false)]
    [Column("id")]
    public string id { get; set; }

    [Column("character_name")]
    public string character_name { get; set; }

    [Column("gender")]
    public string gender { get; set; }

    [Column("level")]
    public int level { get; set; }

    [Column("current_exp")]
    public int current_exp { get; set; }

    [Column("gold")]
    public int gold { get; set; }

    [Column("diamond")]
    public int diamond { get; set; }

    [Column("energy")]
    public int energy { get; set; }

    [Column("stamina")]
    public int stamina { get; set; }

    [Column("last_regen_time")]
    public System.DateTime last_regen_time { get; set; }
}

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }
    public string CurrentUserId { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public async Task SignUpAndCreateAccount(string email, string password, string usernameInput)
    {
        try
        {
            var session = await SupabaseManager.Instance.Client.Auth.SignUp(email, password);
            if (session?.User != null)
            {
                var newAccount = new AccountModel { id = session.User.Id, username = usernameInput };
                await SupabaseManager.Instance.Client.From<AccountModel>().Insert(newAccount);
                Debug.Log("<color=green>Đã tạo tài khoản thành công!</color>");
                await SignIn(email, password);
            }
        }
        catch (System.Exception e) 
        { 
            Debug.LogError("Lỗi Đăng ký: " + e.Message); 
            throw e; // Ném lỗi để UI bắt được
        }
    }

    public async Task SignIn(string email, string password)
    {
        try
        {
            var session = await SupabaseManager.Instance.Client.Auth.SignIn(email, password);
            CurrentUserId = session.User.Id;
            Debug.Log("<color=green>Đăng nhập thành công!</color>");
            await CheckPlayerCharacter();
        }
        catch (System.Exception e) 
        { 
            Debug.LogError("Lỗi Đăng nhập: " + e.Message); 
            throw e; // Ném lỗi để UI bắt được
        }
    }

    private async Task CheckPlayerCharacter()
    {
        var response = await SupabaseManager.Instance.Client
            .From<CharacterModel>()
            .Where(x => x.id == CurrentUserId)
            .Single();

        if (response == null)
        {
            Debug.Log("Chưa có nhân vật, chuyển đến scene CharacterSelector...");
            SceneManager.LoadScene("CharacterSelector");
        }
        else
        {
            // Log kiểm tra dữ liệu ngay khi login
            Debug.Log($"<color=green>[AuthManager]</color> Login thành công: {response.character_name}");
            Debug.Log($"[AuthManager] Data từ DB: Level: {response.level}, EXP: {response.current_exp}, Vàng: {response.gold}");
            
            // Khởi tạo trình quản lý Resource
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.Initialize(response);
            }

            SceneManager.LoadScene("MainGame");
        }
    }
}
