using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Cộng tiền (Vàng và Kim cương)
    public async Task AddCurrency(int goldAmount, int diamondAmount)
    {
        var parameters = new Dictionary<string, object> { 
            { "p_id", AuthManager.Instance.CurrentUserId },
            { "p_gold", goldAmount },
            { "p_diamond", diamondAmount }
        };
        await SupabaseManager.Instance.Client.Rpc("add_currency", parameters);
        await ResourceManager.Instance.SyncWithServer(); // Cập nhật lại UI sau khi cộng
    }

    // Tiêu tiền (Trả về true nếu thành công, false nếu không đủ tiền)
    public async Task<bool> SpendCurrency(int goldAmount, int diamondAmount)
    {
        var parameters = new Dictionary<string, object> { 
            { "p_id", AuthManager.Instance.CurrentUserId },
            { "p_gold", goldAmount },
            { "p_diamond", diamondAmount }
        };
        var result = await SupabaseManager.Instance.Client.Rpc<bool>("spend_currency", parameters);
        
        if (result) await ResourceManager.Instance.SyncWithServer();
        return result;
    }

    // Cộng EXP
    public async Task AddExp(int amount)
    {
        var parameters = new Dictionary<string, object> { 
            { "p_id", AuthManager.Instance.CurrentUserId },
            { "p_amount", amount }
        };
        await SupabaseManager.Instance.Client.Rpc("add_exp", parameters);
        await ResourceManager.Instance.SyncWithServer();
    }
}
