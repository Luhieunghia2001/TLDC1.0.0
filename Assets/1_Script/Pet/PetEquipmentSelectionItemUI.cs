using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PetEquipmentSelectionItemUI : MonoBehaviour
{
    [SerializeField] private Image iconImg;
    [SerializeField] private Image tierImg; // Hiển thị khung/nhãn Tier của trang bị
    [SerializeField] private TextMeshProUGUI nameTxt;
    [SerializeField] private TextMeshProUGUI equippedByTxt; // Sẽ hiện "Không" hoặc Tên Pet đang dùng
    [SerializeField] private Button actionBtn;

    public void Setup(ItemBaseSO itemBase, int quantity, string equippedPetName, System.Action onActionClicked)
    {
        if (itemBase != null)
        {
            if (iconImg != null) iconImg.sprite = itemBase.icon;
            if (nameTxt != null) nameTxt.text = itemBase.itemName + (quantity > 1 ? $" (x{quantity})" : "");
            
            SetTierImage(itemBase);

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

    private void SetTierImage(ItemBaseSO itemBase)
    {
        if (tierImg == null) return;

        if (itemBase != null && itemBase.type == ItemType.Equipment && InventoryManager.Instance != null)
        {
            Sprite s = InventoryManager.Instance.GetTierSprite(itemBase.tier);
            if (s != null)
            {
                tierImg.gameObject.SetActive(true);
                tierImg.sprite = s;
            }
            else
            {
                tierImg.gameObject.SetActive(false);
            }
        }
        else
        {
            tierImg.gameObject.SetActive(false);
        }
    }
}
