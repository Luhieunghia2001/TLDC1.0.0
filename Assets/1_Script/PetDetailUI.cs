using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PetDetailUI : MonoBehaviour
{
    public static PetDetailUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI petNameTxt;
    [SerializeField] private TextMeshProUGUI typeTxt; // Hiện: "Loại: Vật Lý" hoặc "Loại: Ma Pháp"
    [SerializeField] private TextMeshProUGUI levelTxt;

    [Header("Stats Texts")]
    [SerializeField] private TextMeshProUGUI hpTxt;
    [SerializeField] private TextMeshProUGUI atkPhyTxt;
    [SerializeField] private TextMeshProUGUI atkMagTxt;
    [SerializeField] private TextMeshProUGUI defPhyTxt;
    [SerializeField] private TextMeshProUGUI defMagTxt;
    [SerializeField] private TextMeshProUGUI speedTxt;

    [Header("Buttons")]
    [SerializeField] private Button closeBtn;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        panel.SetActive(false);
        closeBtn.onClick.AddListener(() => panel.SetActive(false));
    }

    public void Open(PetModel pet)
    {
        panel.SetActive(true);

        petNameTxt.text = pet.petName;
        levelTxt.text = "Level: " + pet.level;

        // Hiển thị Kiểu Tấn Công (Dựa vào petType trong Database)
        string vietnameseType = (pet.petType.ToLower() == "physical") ? "Vật Lý" : "Ma Pháp";
        typeTxt.text = "Hệ: " + vietnameseType;

        hpTxt.text = "HP: " + pet.hp;
        atkPhyTxt.text = "ATK Vật Lý: " + pet.atkPhy;
        atkMagTxt.text = "ATK Ma Pháp: " + pet.atkMag;
        defPhyTxt.text = "THỦ Vật Lý: " + pet.defPhy;
        defMagTxt.text = "THỦ Ma Pháp: " + pet.defMag;
        speedTxt.text = "Tốc Độ: " + pet.speed;
    }
}
