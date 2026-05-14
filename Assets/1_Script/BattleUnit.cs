using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUnit : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TMP_Text nameTxt; // Dùng TMP_Text để nhận cả UGUI và TextMeshPro thường
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
        if (hpSlider != null && battlePet != null)
        {
            float targetValue = (float)battlePet.currentHP / battlePet.stats.HP;
            hpSlider.value = targetValue; 
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
