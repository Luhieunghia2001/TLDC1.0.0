using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryUIItem : MonoBehaviour
{
    [SerializeField] private Image iconImg;
    [SerializeField] private Image tierImg; // Hiển thị khung/nhãn Tier của vật phẩm
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
            
            // Nếu là trang bị, ẩn Text hiển thị số lượng đi
            if (quantityTxt != null)
            {
                if (baseInfo.type == ItemType.Equipment)
                {
                    quantityTxt.gameObject.SetActive(false);
                }
                else
                {
                    quantityTxt.gameObject.SetActive(true);
                    quantityTxt.text = itemData.quantity.ToString();
                }
            }

            if (tierImg != null)
            {
                if (baseInfo.type == ItemType.Equipment && InventoryManager.Instance != null)
                {
                    Sprite s = InventoryManager.Instance.GetTierSprite(baseInfo.tier);
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
