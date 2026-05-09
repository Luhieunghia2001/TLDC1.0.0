using UnityEngine;
using Postgrest.Attributes;
using Postgrest.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

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

    [Column("hp")]
    public int hp { get; set; }

    [Column("atk_phy")]
    public int atkPhy { get; set; }

    [Column("atk_mag")]
    public int atkMag { get; set; }

    [Column("def_phy")]
    public int defPhy { get; set; }

    [Column("def_mag")]
    public int defMag { get; set; }

    [Column("speed")]
    public int speed { get; set; }

    [Column("level")]
    public int level { get; set; }

    [Column("current_exp")]
    public int currentExp { get; set; }
}

public class PetManager : MonoBehaviour
{
    public static PetManager Instance { get; private set; }

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
            
            return response.Models;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lỗi lấy danh sách Pet: " + e.Message);
            return new List<PetModel>();
        }
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
            hp = baseData.baseHP,
            atkPhy = baseData.baseAtkPhy,
            atkMag = baseData.baseAtkMag,
            defPhy = baseData.baseDefPhy,
            defMag = baseData.baseDefMag,
            speed = baseData.baseSpeed,
            level = 1,
            currentExp = 0
        };

        await SupabaseManager.Instance.Client.From<PetModel>().Insert(newPet);
        Debug.Log($"<color=green>Đã lưu Pet vào DB:</color> {baseData.speciesName} (ID: {baseData.petBaseID})");
    }
}
