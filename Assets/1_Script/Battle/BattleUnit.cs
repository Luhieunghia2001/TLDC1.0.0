using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BattleUnit : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image hpFillImage; 
    [SerializeField] private Image hpBackImage;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private Image element;
    [SerializeField] private Image avatar;
    [SerializeField] private Image tier;
    [SerializeField] private Sprite[] hpBarSprites;
    [SerializeField] private TMP_Text nameTxt; 
    [SerializeField] private TMP_Text levelTxt;

    [SerializeField] private Transform contenListPet;
    [SerializeField] private PetUIItem petUIItemPrefab;

    [Header("Sprites Databases")]
    [SerializeField] private Sprite[] elementSprites;
    [SerializeField] private Sprite[] tierSprites;

    [Header("Damage Text Prefab")]
    [SerializeField] private GameObject damageTextPrefab; 
    [SerializeField] private Transform damageSpawnPoint; 

    private BattlePet battlePet;
    private List<BattlePet> myTeam; // Lưu đội của unit này
    private List<PetUIItem> spawnedItems = new List<PetUIItem>();


    public void Setup(BattlePet pet, List<BattlePet> team = null)
    {
        this.battlePet = pet;
        this.myTeam = team;


        if (nameTxt != null) nameTxt.text = pet.petData.petName;
        if (levelTxt != null) levelTxt.text = "Lv." + pet.petData.level;

        // Cập nhật avatar, element, tier
        if (avatar != null && pet.baseData != null) avatar.sprite = pet.baseData.icon;
        SetElementIcon(pet);
        SetTierIcon(pet);

        // Khởi tạo danh sách Pet trong đội
        InitPetList(pet);

        UpdateHPUI();
    }

    public void UpdateHPUI()
    {
        if (hpFillImage != null && battlePet != null && hpBarSprites.Length > 0)
        {
            if (battlePet.currentHP <= 0)
            {
                hpFillImage.fillAmount = 0;
                if (hpBackImage != null) hpBackImage.gameObject.SetActive(false);
            }
            else
            {
                // 1. Tính toán lớp máu hiện tại
                // Layer 0: 1-100%, Layer 1: 101-200%, v.v.
                int layerIndex = (battlePet.currentHP - 1) / battlePet.stats.HP;
                
                // 2. Tính toán phần trăm của thanh hiện tại
                float currentLayerHP = battlePet.currentHP - (layerIndex * battlePet.stats.HP);
                float fillAmount = currentLayerHP / battlePet.stats.HP;

                // 3. Cập nhật Sprite và Fill
                // Dùng toán tử % để lặp lại danh sách Sprite nếu máu quá nhiều
                hpFillImage.sprite = hpBarSprites[layerIndex % hpBarSprites.Length];
                hpFillImage.fillAmount = fillAmount;

                // 4. Cập nhật thanh nền (màu của lớp dưới)
                if (hpBackImage != null)
                {
                    if (layerIndex > 0)
                    {
                        hpBackImage.gameObject.SetActive(true);
                        hpBackImage.sprite = hpBarSprites[(layerIndex - 1) % hpBarSprites.Length];
                    }
                    else
                    {
                        // Nếu là lớp cuối cùng (Layer 0) thì tắt nền hoặc để màu đen/xám
                        hpBackImage.gameObject.SetActive(false);
                    }
                }
            }
        }

        // Cập nhật text hiển thị máu
        if (hpText != null && battlePet != null)
        {
            hpText.text = $"{battlePet.currentHP} / {battlePet.stats.HP}";
        }

        // Cập nhật trạng thái danh sách Pet (Active / Dead)
        UpdatePetListStatus();
    }

    public void ShowDamageText(int amount)
    {
        if (damageTextPrefab != null)
        {
            // Nếu có damageSpawnPoint thì sinh tại đó, không thì sinh tại vị trí hiện tại
            Vector3 pos = damageSpawnPoint != null ? damageSpawnPoint.position : transform.position;
            GameObject txtGo = Instantiate(damageTextPrefab, pos, Quaternion.identity);
            
            // Tìm TMP_Text (hỗ trợ cả 3D Text và UI Text)
            var tmp = txtGo.GetComponentInChildren<TMP_Text>();
            if (tmp != null) tmp.text = "-" + amount;

            // Tự xóa sau 1 giây
            Destroy(txtGo, 1f);
        }
    }

    private void SetElementIcon(BattlePet pet)
    {
        if (element == null || elementSprites == null || elementSprites.Length == 0) return;

        string elStr = pet.petData != null ? pet.petData.element : null;
        if (string.IsNullOrEmpty(elStr) && pet.baseData != null)
        {
            elStr = pet.baseData.element.ToString();
        }

        if (!string.IsNullOrEmpty(elStr))
        {
            int index = 0;
            switch (elStr.ToLower())
            {
                case "fire": index = 0; break;
                case "water": index = 1; break;
                case "earth": index = 2; break;
                case "wind": index = 3; break;
            }
            if (index < elementSprites.Length)
            {
                element.sprite = elementSprites[index];
            }
        }
    }

    private void SetTierIcon(BattlePet pet)
    {
        if (tier == null || tierSprites == null || tierSprites.Length == 0) return;

        string tierStr = pet.petData != null ? pet.petData.tier : null;
        if (string.IsNullOrEmpty(tierStr) && pet.baseData != null)
        {
            tierStr = pet.baseData.defaultTier.ToString();
        }

        if (!string.IsNullOrEmpty(tierStr))
        {
            int index = 0;
            switch (tierStr.ToUpper())
            {
                case "D": index = 0; break;
                case "C": index = 1; break;
                case "B": index = 2; break;
                case "A": index = 3; break;
                case "S": index = 4; break;
                case "SS": index = 5; break;
                case "SSS": index = 6; break;
            }
            if (index < tierSprites.Length)
            {
                tier.sprite = tierSprites[index];
            }
        }
    }

    private void InitPetList(BattlePet activePet)
    {
        if (contenListPet == null || petUIItemPrefab == null || myTeam == null) return;

        foreach (Transform child in contenListPet)
            Destroy(child.gameObject);
        spawnedItems.Clear();

        foreach (var bp in myTeam)
        {
            PetUIItem item = Instantiate(petUIItemPrefab, contenListPet);
            item.Setup(bp.petData, bp == activePet);
            spawnedItems.Add(item);
        }
    }

    private void UpdatePetListStatus()
    {
        if (myTeam == null || battlePet == null || spawnedItems.Count == 0) return;

        for (int i = 0; i < myTeam.Count; i++)
        {
            if (i >= spawnedItems.Count) break;

            var bp = myTeam[i];
            var item = spawnedItems[i];

            if (item != null)
            {
                item.SetDead(bp.isDead);
            }
        }
    }
}

