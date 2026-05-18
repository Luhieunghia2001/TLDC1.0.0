using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardItemUI : MonoBehaviour
{
    [SerializeField] private Image iconImg;
    [SerializeField] private TextMeshProUGUI qtyTxt;

    public void Setup(Sprite icon, int quantity)
    {
        if (iconImg != null) iconImg.sprite = icon;
        if (qtyTxt != null) qtyTxt.text = quantity > 1 ? quantity.ToString() : "";
    }

    public void Setup(Sprite icon, string customQtyText, Color textColor)
    {
        if (iconImg != null) iconImg.sprite = icon;
        if (qtyTxt != null)
        {
            qtyTxt.text = customQtyText;
            qtyTxt.color = textColor;
        }
    }
}