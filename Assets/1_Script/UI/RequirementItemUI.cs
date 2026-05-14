using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RequirementItemUI : MonoBehaviour
{
    [SerializeField] private Image iconImg;
    [SerializeField] private TextMeshProUGUI qtyTxt;

    public void Setup(Sprite icon, int currentQty, int requiredQty)
    {
        if (iconImg != null) iconImg.sprite = icon;
        
        if (qtyTxt != null)
        {
            qtyTxt.text = $"{currentQty}/{requiredQty}";

            // Đổi màu đỏ nếu thiếu nguyên liệu, màu xanh nếu đủ
            if (currentQty < requiredQty)
                qtyTxt.color = Color.red;
            else
                qtyTxt.color = Color.green;
        }
    }
}
