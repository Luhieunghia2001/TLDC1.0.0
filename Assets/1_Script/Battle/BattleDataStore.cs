using System.Collections.Generic;

public static class BattleDataStore
{
    public static List<PetModel> selectedAllies = new List<PetModel>();
    public static List<PetModel> selectedEnemies = new List<PetModel>();
    public static string currentBattleLogId; // Lưu mã trận đấu để nhận thưởng bảo mật
}
