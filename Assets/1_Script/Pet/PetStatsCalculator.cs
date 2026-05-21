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

        // 4. GEAR STATS: Tính chỉ số trang bị
        int flatHP = 0;
        int flatAtkPhy = 0;
        int flatAtkMag = 0;
        int flatDefPhy = 0;
        int flatDefMag = 0;
        int flatSpeed = 0;

        float pctHP = 0f;
        float pctAtk = 0f;
        float pctSpeed = 0f;

        System.Action<string> applyEquip = (itemId) =>
        {
            if (string.IsNullOrEmpty(itemId)) return;
            
            // Lấy dữ liệu thực từ CSDL đã nạp vào Cache
            var item = InventoryManager.Instance.GetItemTemplateByID(itemId);
            if (item == null) return;

            flatHP += item.bonusHP;
            flatAtkPhy += item.bonusAtkPhy;
            flatAtkMag += item.bonusAtkMag;
            flatDefPhy += item.bonusDefPhy;
            flatDefMag += item.bonusDefMag;
            flatSpeed += item.bonusSpeed;

            pctHP += item.percentHP;
            pctAtk += item.percentAtk;
            pctSpeed += item.percentSpeed;
        };

        applyEquip(pet.helmetId);
        applyEquip(pet.armorId);
        applyEquip(pet.weaponId);
        applyEquip(pet.bootsId);
        applyEquip(pet.wingsId);
        applyEquip(pet.amuletId);

        // Áp dụng chỉ số cộng thêm từ trang bị theo chuẩn RPG chuyên nghiệp (miHoYo, Blizzard, Epic Seven...)
        // Chỉ số % chỉ nhân vào Chỉ số gốc (Base Stats), sau đó mới cộng chỉ số Flat (Flat Stats)
        hp = (hp * (1f + pctHP)) + flatHP;
        atkP = (atkP * (1f + pctAtk)) + flatAtkPhy;
        atkM = (atkM * (1f + pctAtk)) + flatAtkMag;
        defP = defP + flatDefPhy;
        defM = defM + flatDefMag;
        spd = (spd * (1f + pctSpeed)) + flatSpeed;

        // Lưu kết quả cuối cùng
        stats.HP = Mathf.RoundToInt(hp);
        stats.AtkPhy = Mathf.RoundToInt(atkP);
        stats.AtkMag = Mathf.RoundToInt(atkM);
        stats.DefPhy = Mathf.RoundToInt(defP);
        stats.DefMag = Mathf.RoundToInt(defM);
        stats.Speed = Mathf.RoundToInt(spd);

        return stats;
    }

    public static int CalculateCombatPower(PetFinalStats stats)
    {
        return Mathf.RoundToInt((stats.HP * 0.1f) + (stats.AtkPhy + stats.AtkMag) * 1.0f + (stats.DefPhy + stats.DefMag) * 0.8f + (stats.Speed * 1.5f));
    }
}
