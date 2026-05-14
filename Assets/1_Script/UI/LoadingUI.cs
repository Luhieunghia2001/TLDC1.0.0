using UnityEngine;

public class LoadingUI : MonoBehaviour
{
    public static LoadingUI Instance { get; private set; }

    [SerializeField] private GameObject loadingPanel; // Cái bảng đen mờ bao phủ toàn màn hình

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);

        Hide(); // Mặc định ẩn đi
    }

    public void Show()
    {
        if (loadingPanel != null) loadingPanel.SetActive(true);
    }

    public void Hide()
    {
        if (loadingPanel != null) loadingPanel.SetActive(false);
    }
}
