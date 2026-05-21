using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PetEquipmentSelectionItemUI : MonoBehaviour
{
    [SerializeField] private Image iconImg;
    [SerializeField] private TextMeshProUGUI nameTxt;
    [SerializeField] private TextMeshProUGUI equippedByTxt; // Sẽ hiện "Không" hoặc Tên Pet đang dùng
    [SerializeField] private Button actionBtn;

    public void Setup(ItemBaseSO itemBase, int quantity, string equippedPetName, System.Action onActionClicked)
    {
        if (itemBase != null)
        {
            if (iconImg != null) iconImg.sprite = itemBase.icon;
            if (nameTxt != null) nameTxt.text = itemBase.itemName + (quantity > 1 ? $" (x{quantity})" : "");
            
            if (equippedByTxt != null)
            {
                if (string.IsNullOrEmpty(equippedPetName) || equippedPetName.ToLower() == "không")
                {
                    equippedByTxt.text = "Đang dùng: Không";
                }
                else
                {
                    equippedByTxt.text = $"Đang dùng: {equippedPetName}";
                }
            }
        }

        if (actionBtn != null)
        {
            actionBtn.onClick.RemoveAllListeners();
            actionBtn.onClick.AddListener(() => onActionClicked?.Invoke());
        }
    }
}
