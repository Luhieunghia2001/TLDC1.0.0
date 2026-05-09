using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PanelManager : MonoBehaviour
{
    [System.Serializable]
    public class PanelTab
    {
        public string name;      // Tên gợi nhớ (không bắt buộc)
        public Button tabButton; // Nút bấm để mở
        public GameObject panel; // Bảng tương ứng để hiện
    }

    [Header("Settings")]
    [SerializeField] private List<PanelTab> tabs;
    [SerializeField] private bool closeAllOnStart = true;
    [SerializeField] private int defaultTabIndex = 0; // Tab mặc định mở khi vào game

    private void Awake()
    {
        // Tự động gán sự kiện cho tất cả các nút trong danh sách
        for (int i = 0; i < tabs.Count; i++)
        {
            int index = i; // Cần tạo biến tạm để tránh lỗi closure trong loop
            if (tabs[i].tabButton != null)
            {
                tabs[i].tabButton.onClick.AddListener(() => OpenPanel(index));
            }
        }
    }

    private void Start()
    {
        if (closeAllOnStart)
        {
            CloseAllPanels();
            if (tabs.Count > defaultTabIndex) OpenPanel(defaultTabIndex);
        }
    }

    public void OpenPanel(int index)
    {
        // Tắt tất cả các bảng trước
        CloseAllPanels();

        // Bật bảng được chọn
        if (index >= 0 && index < tabs.Count)
        {
            if (tabs[index].panel != null)
            {
                tabs[index].panel.SetActive(true);
                
                // (Tùy chọn) Đổi màu nút bấm để người chơi biết đang ở tab nào
                UpdateTabButtonVisuals(index);
            }
        }
    }

    public void CloseAllPanels()
    {
        foreach (var tab in tabs)
        {
            if (tab.panel != null) tab.panel.SetActive(false);
        }
    }

    private void UpdateTabButtonVisuals(int activeIndex)
    {
        // Bạn có thể thêm logic đổi màu/icon của nút ở đây nếu muốn
    }
}
