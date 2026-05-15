using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUnit : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image hpFillImage; 
    [SerializeField] private Image hpBackImage; // Thanh nền (hiện màu của lớp máu bên dưới)
    [SerializeField] private Sprite[] hpBarSprites;
    [SerializeField] private TMP_Text nameTxt; 
    [SerializeField] private TMP_Text levelTxt;

    [Header("Damage Text Prefab")]
    [SerializeField] private GameObject damageTextPrefab; 
    [SerializeField] private Transform damageSpawnPoint; 

    private BattlePet battlePet;

    public void Setup(BattlePet pet)
    {
        this.battlePet = pet;

        if (nameTxt != null) nameTxt.text = pet.petData.petName;
        if (levelTxt != null) levelTxt.text = "Lv." + pet.petData.level;

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
                return;
            }

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
}
