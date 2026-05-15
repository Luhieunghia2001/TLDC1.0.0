using System.Collections.Generic;

[System.Serializable]
public class BattlePet
{
    public PetModel petData;
    public PetBaseSO baseData;
    public PetFinalStats stats;
    
    public int currentHP;
    public bool isDead => currentHP <= 0;
    public Dictionary<string, int> cooldownDict = new Dictionary<string, int>();
    public Dictionary<string, int> stackDict = new Dictionary<string, int>();

    public BattlePet(PetModel data, PetBaseSO baseData)
    {
        this.petData = data;
        this.baseData = baseData;
        // Tính toán chỉ số thực tế từ Level, Sao, Tầng
        this.stats = PetStatsCalculator.GetFinalStats(data, baseData);
        this.currentHP = stats.HP;
    }

    public void ReduceCooldowns()
    {
        List<string> keys = new List<string>(cooldownDict.Keys);
        foreach (var key in keys)
        {
            if (cooldownDict[key] > 0) cooldownDict[key]--;
        }
    }
}
