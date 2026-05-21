using UnityEngine;
using Postgrest.Attributes;
using Postgrest.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

[Table("user_pets")]
public class PetModel : BaseModel
{
    [PrimaryKey("id", true)] // Đổi thành true để Unity tự gửi ID lên
    [Column("id")]
    public string id { get; set; }

    [Column("user_id")]
    public string userId { get; set; }

    [Column("pet_name")]
    public string petName { get; set; }

    [Column("element")]
    public string element { get; set; }

    [Column("pet_type")]
    public string petType { get; set; }

    [Column("tier")]
    public string tier { get; set; }

    [Column("pet_base_id")]
    public string petBaseId { get; set; }

    [Column("level")]
    public int level { get; set; }

    [Column("current_exp")]
    public int currentExp { get; set; }

    [Column("star")]
    public int star { get; set; } // Số sao hiện tại

    [Column("realm")]
    public int realm { get; set; } // Tầng hiện tại

    [Column("helmet_id")]
    public string helmetId { get; set; }

    [Column("helmet_enhancement_level")]
    public int helmetEnhancementLevel { get; set; }

    [Column("armor_id")]
    public string armorId { get; set; }

    [Column("armor_enhancement_level")]
    public int armorEnhancementLevel { get; set; }

    [Column("weapon_id")]
    public string weaponId { get; set; }

    [Column("weapon_enhancement_level")]
    public int weaponEnhancementLevel { get; set; }

    [Column("boots_id")]
    public string bootsId { get; set; }

    [Column("boots_enhancement_level")]
    public int bootsEnhancementLevel { get; set; }

    [Column("wings_id")]
    public string wingsId { get; set; }

    [Column("wings_enhancement_level")]
    public int wingsEnhancementLevel { get; set; }

    [Column("amulet_id")]
    public string amuletId { get; set; }

    [Column("amulet_enhancement_level")]
    public int amuletEnhancementLevel { get; set; }
}

[System.Serializable]
public class PetServerStats
{
    public int hp;
    public int atk_phy;
    public int atk_mag;
    public int def_phy;
    public int def_mag;
    public int speed;
    public int combat_power;

    public PetFinalStats ToFinalStats()
    {
        return new PetFinalStats
        {
            HP = hp,
            AtkPhy = atk_phy,
            AtkMag = atk_mag,
            DefPhy = def_phy,
            DefMag = def_mag,
            Speed = speed
        };
    }
}

public class PetManager : MonoBehaviour
{
    public static PetManager Instance { get; private set; }

    // --- GLOBAL STATE & EVENTS (Observer Pattern) ---
    public PetModel CurrentPet { get; private set; }
    
    // Đội hình Pet hiện tại (Dùng chung cho toàn game)
    public List<PetModel> SelectedTeam = new List<PetModel>();
    public const int MAX_TEAM_SIZE = 5;

    // Sự kiện bắn ra khi người dùng chọn một con Pet MỚI
    public event System.Action<PetModel> OnPetSelected; 
    
    // Sự kiện bắn ra khi Pet ĐANG CHỌN được cập nhật chỉ số (lên cấp, cộng exp...)
    public event System.Action<PetModel> OnPetStatsUpdated;

    public void SelectPet(PetModel pet)
    {
        CurrentPet = pet;
        OnPetSelected?.Invoke(pet);
    }

    public void NotifyPetStatsUpdated(PetModel updatedPet)
    {
        if (CurrentPet != null && CurrentPet.id == updatedPet.id)
        {
            CurrentPet = updatedPet;
            OnPetStatsUpdated?.Invoke(updatedPet);
        }
    }
    // ------------------------------------------------

    [Header("Pet Database")]
    [SerializeField] private List<PetBaseSO> allPetBases; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public PetBaseSO GetPetBaseByID(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return allPetBases.Find(x => x.petBaseID == id);
    }

    public async Task<List<PetModel>> GetMyPets()
    {
        try
        {
            var response = await SupabaseManager.Instance.Client
                .From<PetModel>()
                .Get();
            var list = response.Models;

            // Sắp xếp theo Level (giảm dần), sau đó theo Tier (giảm dần từ SSS -> D)
            list.Sort((a, b) =>
            {
                if (a.level != b.level)
                    return b.level.CompareTo(a.level); // Level cao hơn đứng trước
                
                int tierA = GetTierValue(a.tier);
                int tierB = GetTierValue(b.tier);
                return tierB.CompareTo(tierA); // Tier cao hơn đứng trước
            });

            return list;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lỗi lấy danh sách Pet: " + e.Message);
            return new List<PetModel>();
        }
    }

    public async Task<PetServerStats> GetPetFinalStatsFromServer(string petId)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_pet_id", petId }
        };

        return await GetPetStatsRpc("get_pet_final_stats", parameters);
    }

    public async Task<PetServerStats> GetPetFinalStatsPreviewFromServer(string petId, int level, int star, int realm)
    {
        var parameters = new Dictionary<string, object>
        {
            { "p_pet_id", petId },
            { "p_level", level },
            { "p_star", star },
            { "p_realm", realm }
        };

        return await GetPetStatsRpc("get_pet_final_stats_preview", parameters);
    }

    private async Task<PetServerStats> GetPetStatsRpc(string rpcName, Dictionary<string, object> parameters)
    {
        var response = await SupabaseManager.Instance.Client.Rpc(rpcName, parameters);
        var token = JToken.Parse(response.Content);
        var row = token is JArray array ? array.First : token;
        if (row == null) return null;

        return new PetServerStats
        {
            hp = row.Value<int?>("hp") ?? 0,
            atk_phy = row.Value<int?>("atk_phy") ?? 0,
            atk_mag = row.Value<int?>("atk_mag") ?? 0,
            def_phy = row.Value<int?>("def_phy") ?? 0,
            def_mag = row.Value<int?>("def_mag") ?? 0,
            speed = row.Value<int?>("speed") ?? 0,
            combat_power = row.Value<int?>("combat_power") ?? 0
        };
    }

    public async Task CreateNewPet(PetBaseSO baseData)
    {
        var newPet = new PetModel
        {
            id = System.Guid.NewGuid().ToString(), // Tự tạo ID duy nhất ở Unity
            userId = AuthManager.Instance.CurrentUserId,
            petBaseId = baseData.petBaseID,
            petName = baseData.speciesName,
            element = baseData.element.ToString().ToLower(),
            petType = baseData.attackType.ToString().ToLower(),
            tier = baseData.defaultTier.ToString(),
            level = 1,
            currentExp = 0,
            star = 1,
            realm = 1
        };

        await SupabaseManager.Instance.Client.From<PetModel>().Insert(newPet);
        Debug.Log($"<color=green>Đã lưu Pet vào DB:</color> {baseData.speciesName} (ID: {baseData.petBaseID})");
    }

    // Hàm phụ trợ để so sánh Tier
    private int GetTierValue(string tier)
    {
        if (string.IsNullOrEmpty(tier)) return 0;
        switch (tier.ToUpper())
        {
            case "SSS": return 6;
            case "SS": return 5;
            case "S": return 4;
            case "A": return 3;
            case "B": return 2;
            case "C": return 1;
            case "D": return 0;
            default: return 0;
        }
    }
}
