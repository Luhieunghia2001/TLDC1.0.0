using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemDetailUI : MonoBehaviour
{
    public static ItemDetailUI Instance { get; private set; }

    [Header("Item Detail Panel")]
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private Image iconImg;
    [SerializeField] private Image tierImg; // Hiển thị khung/nhãn Tier của trang bị nếu có
    [SerializeField] private TextMeshProUGUI nameTxt;
    [SerializeField] private TextMeshProUGUI quantityTxt;
    [SerializeField] private TextMeshProUGUI descTxt;
    [SerializeField] private Button useBtn;
    [SerializeField] private Button sellBtn;
    [SerializeField] private Button closeBtn;
    [SerializeField] private GameObject inventoryPanel; // Kéo Canvas hoặc Panel túi đồ vào đây

    [Header("Confirm Sell Panel")]
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private TextMeshProUGUI confirmMsgTxt;
    [SerializeField] private TMP_InputField sellQtyInput; // Ô nhập số lượng
    [SerializeField] private Button plusBtn;              // Nút +
    [SerializeField] private Button minusBtn;             // Nút -
    [SerializeField] private Button confirmYesBtn;
    [SerializeField] private Button confirmNoBtn;

    private InventoryModel currentItem;
    private ItemBaseSO currentBase;
    private int currentSellQty = 10;

    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            Debug.Log("<color=white>[ItemDetail]</color> Instance đã được khởi tạo thành công!");
        }
        detailPanel.SetActive(false);
        confirmPanel.SetActive(false);

        closeBtn.onClick.AddListener(() => {
            detailPanel.SetActive(false);
            if (inventoryPanel != null)
                inventoryPanel.SetActive(false);
        });
        confirmNoBtn.onClick.AddListener(() => confirmPanel.SetActive(false));

        sellBtn.onClick.AddListener(OnSellClick);
        confirmYesBtn.onClick.AddListener(OnConfirmSell);

        // Sự kiện cho nút + và -
        plusBtn.onClick.AddListener(() => ChangeQty(1));
        minusBtn.onClick.AddListener(() => ChangeQty(-1));
        
        // Sự kiện khi người dùng tự nhập số vào ô
        sellQtyInput.onEndEdit.AddListener(OnInputQtyChanged);
        
        useBtn.onClick.AddListener(() => Debug.Log("Nút Sử dụng đang được phát triển..."));
    }

    public void Show(InventoryModel item, ItemBaseSO baseInfo)
    {
        Debug.Log($"<color=green>[ItemDetail]</color> Đang hiển thị bảng cho: {baseInfo.itemName}");
        currentItem = item;
        currentBase = baseInfo;

        detailPanel.SetActive(true);
        confirmPanel.SetActive(false);

        iconImg.sprite = baseInfo.icon;
        nameTxt.text = baseInfo.itemName;
        quantityTxt.text = "Số lượng: " + item.quantity;
        descTxt.text = baseInfo.description;

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

    private void OnSellClick()
    {
        // Thiết lập số lượng mặc định là 10 (hoặc tối đa nếu có ít hơn 10)
        currentSellQty = Mathf.Min(10, currentItem.quantity);
        UpdateQtyUI();

        confirmMsgTxt.text = $"Bạn muốn bán bao nhiêu {currentBase.itemName}?";
        confirmPanel.SetActive(true);
    }

    private void ChangeQty(int amount)
    {
        currentSellQty += amount;
        currentSellQty = Mathf.Clamp(currentSellQty, 1, currentItem.quantity);
        UpdateQtyUI();
    }

    private void OnInputQtyChanged(string text)
    {
        if (int.TryParse(text, out int val))
        {
            currentSellQty = Mathf.Clamp(val, 1, currentItem.quantity);
        }
        UpdateQtyUI();
    }

    private void UpdateQtyUI()
    {
        sellQtyInput.text = currentSellQty.ToString();
        // Cập nhật luôn câu thông báo để người chơi biết tổng tiền nhận được
        confirmMsgTxt.text = $"Bán {currentSellQty} {currentBase.itemName} nhận {currentSellQty * currentBase.sellPrice} Vàng?";
    }

    private async void OnConfirmSell()
    {
        if (currentItem != null && currentBase != null)
        {
            await InventoryManager.Instance.SellItem(currentBase.itemID, currentSellQty, currentBase.sellPrice);
        }
        
        confirmPanel.SetActive(false);
        detailPanel.SetActive(false);
    }
}
