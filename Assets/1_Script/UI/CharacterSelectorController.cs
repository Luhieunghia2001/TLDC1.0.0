using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharacterSelectorController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button maleBtn;
    [SerializeField] private Button femaleBtn;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Button confirmBtn;
    [Header("Starter Pets")]
    [SerializeField] private PetBaseSO maleStarterPet;
    [SerializeField] private PetBaseSO femaleStarterPet;

    private string selectedGender = ""; // "male" hoặc "female"

    private void Start()
    {
        maleBtn.onClick.AddListener(() => SelectGender("male"));
        femaleBtn.onClick.AddListener(() => SelectGender("female"));
        confirmBtn.onClick.AddListener(OnConfirmClick);
        
        confirmBtn.interactable = false; // Chỉ cho bấm khi đã chọn giới tính và nhập tên
    }

    private void SelectGender(string gender)
    {
        selectedGender = gender;
        
        if (gender == "male")
        {
            maleBtn.interactable = false; // Đã chọn thì khóa lại
            femaleBtn.interactable = true;
        }
        else
        {
            maleBtn.interactable = true;
            femaleBtn.interactable = false;
        }

        ValidateForm();
    }

    public void OnNameChanged() // Gọi hàm này trong Event "On Value Changed" của InputField
    {
        ValidateForm();
    }

    private void ValidateForm()
    {
        confirmBtn.interactable = !string.IsNullOrEmpty(selectedGender) && !string.IsNullOrEmpty(nameInput.text);
    }

    private async void OnConfirmClick()
    {
        string characterName = nameInput.text;
        string userId = AuthManager.Instance.CurrentUserId;

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("Lỗi: Không tìm thấy ID người dùng!");
            return;
        }

        confirmBtn.interactable = false;
        Debug.Log($"Đang tạo nhân vật {characterName} ({selectedGender})...");

        try
        {
            var newCharacter = new CharacterModel
            {
                id = userId,
                characterName = characterName,
                gender = selectedGender,
                level = 1,
                currentExp = 0,
                gold = 1000,
                diamond = 100,
                energy = 240,
                stamina = 120,
                lastRegenTime = System.DateTime.UtcNow
            };

            await SupabaseManager.Instance.Client.From<CharacterModel>().Insert(newCharacter);
            
            Debug.Log("<color=green>Tạo nhân vật thành công!</color>");

            // Nạp dữ liệu vào ResourceManager ngay lập tức để MainGame có dữ liệu hiện UI
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.Initialize(newCharacter);
            }

            // Tặng Pet tân thủ dựa theo giới tính
            if (PetManager.Instance != null)
            {
                PetBaseSO starter = (selectedGender == "male") ? maleStarterPet : femaleStarterPet;
                if (starter != null)
                {
                    await PetManager.Instance.CreateNewPet(starter);
                }
            }

            SceneManager.LoadScene("MainGame"); // Chuyển vào game chính
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lỗi tạo nhân vật: " + e.Message);
            confirmBtn.interactable = true;
        }
    }
}
