using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DungeonUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI staminaTxt;
    [SerializeField] private TextMeshProUGUI attemptsTxt; // Đổi từ powerTxt sang attemptsTxt
    [SerializeField] private TextMeshProUGUI goldRewardTxt; // Thêm TextMeshProUGUI cho Gold
    [SerializeField] private TextMeshProUGUI expRewardTxt;  // Thêm TextMeshProUGUI cho EXP
    [SerializeField] private Button startBtn;

    [Header("Rewards Preview")]
    [SerializeField] private Transform rewardContainer;
    [SerializeField] private GameObject rewardItemPrefab;

    private DungeonConfigSO currentConfig;
    public string dungeonID; // Thêm trường dungeonID

    public void Setup(string id) // Thay đổi tham số đầu vào thành dungeonID
    {
        Debug.Log($"[DungeonUI] Setup được gọi với dungeonID: {id}");
        dungeonID = id;
        currentConfig = DungeonManager.Instance.GetDungeonByID(dungeonID); // Lấy DungeonConfigSO từ DungeonManager

        if (currentConfig == null)
        {
            Debug.LogError($"Không tìm thấy DungeonConfigSO với ID: {dungeonID}");
            // Thêm log để biết UI bị tắt
            gameObject.SetActive(false); // Tắt UI nếu không tìm thấy cấu hình
            return;
        }
        Debug.Log($"[DungeonUI] Đã tìm thấy DungeonConfigSO: {currentConfig.dungeonName} (ID: {currentConfig.dungeonID})");

        staminaTxt.text = currentConfig.staminaCost.ToString();
        goldRewardTxt.text = currentConfig.goldReward.ToString(); // Hiển thị Gold
        expRewardTxt.text = currentConfig.expReward.ToString();   // Hiển thị EXP

        // Giả sử hiện tại ta chưa có hệ thống lưu lượt đánh trong ngày ở DB, 
        // tôi sẽ để tạm là hiển thị full lượt. Bạn có thể thay số này bằng data từ DungeonManager/ResourceManager.
        int remainingAttempts = currentConfig.maxDailyAttempts; 
        attemptsTxt.text = $"{remainingAttempts}/{currentConfig.maxDailyAttempts}";

        // Kiểm tra điều kiện lượt khiêu chiến
        if (remainingAttempts <= 0)
        {
            startBtn.interactable = false;
            attemptsTxt.color = Color.red; // Đổi màu đỏ nếu hết lượt
        }
        else
        {
            startBtn.interactable = true;
            attemptsTxt.color = Color.white;
        }

        Debug.Log($"[DungeonUI] Cập nhật UI: ID: {id}, Năng lượng tiêu hao: {currentConfig.staminaCost}, Lượt: {remainingAttempts}/{currentConfig.maxDailyAttempts}");

        DisplayRewards(currentConfig);

        startBtn.onClick.RemoveAllListeners();
        startBtn.onClick.AddListener(OnStartBattle);
    }

    private void DisplayRewards(DungeonConfigSO config)
    {
        if (rewardContainer == null || rewardItemPrefab == null) return;

        // Xóa các item cũ trong list preview
        foreach (Transform child in rewardContainer)
            Destroy(child.gameObject);

        if (config.potentialDrops == null) return;

        foreach (var drop in config.potentialDrops)
        {
            if (drop.item == null) continue;
            Debug.Log($"[DungeonUI] Hiển thị phần thưởng: {drop.quantity}x {drop.item.itemName}");

            GameObject itemGo = Instantiate(rewardItemPrefab, rewardContainer);
            if (itemGo.TryGetComponent<RewardItemUI>(out var rewardUI))
            {
                rewardUI.Setup(drop.item.icon, drop.quantity);
            }
        }
    }

    private void OnStartBattle()
    {
        if (currentConfig == null) return;

        // Chuyển đổi dữ liệu từ Config sang PetModel để BattleManager hiểu
        List<PetModel> enemies = new List<PetModel>();
        foreach (var cfg in currentConfig.enemyTeam)
        {
            var baseData = PetManager.Instance.GetPetBaseByID(cfg.petBaseId);
            enemies.Add(new PetModel
            {
                petBaseId = cfg.petBaseId,
                petName = baseData != null ? baseData.speciesName : "Kẻ địch",
                level = cfg.level,
                star = cfg.star,
                realm = cfg.realm
            });
        }

        // Đưa vào Store và chuyển scene
        BattleDataStore.selectedEnemies = enemies;
        // Ví dụ: UnityEngine.SceneManagement.SceneManager.LoadScene("BattleScene");
        Debug.Log($"Bắt đầu ải {currentConfig.dungeonName} với {enemies.Count} kẻ địch.");
    }
}