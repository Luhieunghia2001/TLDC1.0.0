using UnityEngine;

public struct PetFinalStats
{
    public int HP;
    public int AtkPhy;
    public int AtkMag;
    public int DefPhy;
    public int DefMag;
    public int Speed;
}

public static class PetStatsCalculator
{
    /// <summary>
    /// Hàm tính toán ĐỘNG toàn bộ chỉ số của Pet dựa trên Level, Sao, Tầng và BaseStats
    /// </summary>
    public static PetFinalStats GetFinalStats(PetModel pet, PetBaseSO baseData)
    {
        PetFinalStats stats = new PetFinalStats();
        if (pet == null || baseData == null) return stats;

        // 1. CORE STATS: Tính từ Level (Ví dụ mặc định mỗi Level tăng 5% sức mạnh gốc)
        float levelBonus = 1f + ((pet.level - 1) * 0.05f);
        
        float hp = baseData.baseHP * levelBonus;
        float atkP = baseData.baseAtkPhy * levelBonus;
        float atkM = baseData.baseAtkMag * levelBonus;
        float defP = baseData.baseDefPhy * levelBonus;
        float defM = baseData.baseDefMag * levelBonus;
        float spd = baseData.baseSpeed; // Tốc độ thường không tăng theo Level để tránh mất cân bằng turn-based

        // 2. STAR STATS: Cộng dồn chỉ số từ các mốc Thăng Sao
        if (baseData.progressionTable != null)
        {
            // Nếu Pet 3 sao -> Tính mốc lên 2 sao (i=1) và mốc lên 3 sao (i=2)
            for (int i = 1; i < pet.star; i++) 
            {
                var node = baseData.progressionTable.GetStarCost(i);
                if (node != null)
                {
                    hp = (hp * node.hpMultiplier) + node.bonusHP;
                    atkP = (atkP * node.atkMultiplier) + node.bonusAtk;
                    atkM = (atkM * node.atkMultiplier) + node.bonusAtk;
                    defP = (defP * node.defMultiplier) + node.bonusDef;
                    defM = (defM * node.defMultiplier) + node.bonusDef;
                }
            }

            // 3. REALM STATS: Cộng dồn chỉ số từ các mốc Thăng Tầng
            for (int i = 1; i < pet.realm; i++)
            {
                var node = baseData.progressionTable.GetRealmCost(i);
                if (node != null)
                {
                    hp = (hp * node.hpMultiplier) + node.bonusHP;
                    atkP = (atkP * node.atkMultiplier) + node.bonusAtk;
                    atkM = (atkM * node.atkMultiplier) + node.bonusAtk;
                    defP = (defP * node.defMultiplier) + node.bonusDef;
                    defM = (defM * node.defMultiplier) + node.bonusDef;
                }
            }
        }

        // 4. GEAR STATS: (Dành cho tương lai khi làm Trang Bị)
        // Lấy danh sách trang bị của Pet này từ Database
        // Quét vòng lặp qua từng món đồ và cộng thêm
        // hp = (hp * Tổng_Percent_HP_TrangBị) + Tổng_Flat_HP_TrangBị;

        // Lưu kết quả cuối cùng
        stats.HP = Mathf.RoundToInt(hp);
        stats.AtkPhy = Mathf.RoundToInt(atkP);
        stats.AtkMag = Mathf.RoundToInt(atkM);
        stats.DefPhy = Mathf.RoundToInt(defP);
        stats.DefMag = Mathf.RoundToInt(defM);
        stats.Speed = Mathf.RoundToInt(spd);

        return stats;
    }
}
