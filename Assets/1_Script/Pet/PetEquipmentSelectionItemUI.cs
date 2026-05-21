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

    public void Setup(ItemBaseSO itemBase, ItemTemplateModel itemTemplate, int quantity, string equippedPetName, System.Action onActionClicked)
    {
        if (itemBase != null)
        {
            if (iconImg != null) iconImg.sprite = itemBase.icon;
            string displayName = itemTemplate != null ? itemTemplate.name : itemBase.itemName;
            if (nameTxt != null) nameTxt.text = displayName + (quantity > 1 ? $" (x{quantity})" : "");
            
            SetTierImage(itemBase, itemTemplate);

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

    private void SetTierImage(ItemBaseSO itemBase, ItemTemplateModel itemTemplate)
    {
        if (tierImg == null) return;

        if (itemBase != null && InventoryManager.Instance != null)
        {
            PetTier tierEnum = PetTier.D;
            if (itemTemplate != null) System.Enum.TryParse(itemTemplate.tier, true, out tierEnum);
            else tierEnum = itemBase.tier;

            Sprite s = InventoryManager.Instance.GetTierSprite(tierEnum);
            if (s != null)
            {
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
