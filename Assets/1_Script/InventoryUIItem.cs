using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryUIItem : MonoBehaviour
{
    [SerializeField] private Image iconImg;
    [SerializeField] private TextMeshProUGUI quantityTxt;
    [SerializeField] private Button clickBtn;

    private InventoryModel data;
    private ItemBaseSO baseInfo;

    public void Setup(InventoryModel itemData)
    {
        this.data = itemData;
        this.baseInfo = InventoryManager.Instance.GetItemBaseByID(itemData.itemId);

        if (baseInfo != null)
        {
            iconImg.sprite = baseInfo.icon;
            quantityTxt.text = itemData.quantity.ToString();
        }

        clickBtn.onClick.RemoveAllListeners();
        clickBtn.onClick.AddListener(OnItemClick);
    }

    private void OnItemClick()
    {
        Debug.Log($"<color=orange>[Inventory]</color> Bạn vừa bấm vào: {baseInfo.itemName}");
        
        if (ItemDetailUI.Instance != null)
        {
            ItemDetailUI.Instance.Show(data, baseInfo);
        }
        else
        {
            Debug.LogError("[Inventory] Lỗi: Không tìm thấy ItemDetailUI Instance trong Scene!");
        }
    }
}
