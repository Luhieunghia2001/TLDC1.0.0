using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AuthUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField accountInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button loginButton;
    [SerializeField] private TextMeshProUGUI statusTxt; // Thêm text thông báo

    private void Start()
    {
        registerButton.onClick.AddListener(OnRegisterClick);
        loginButton.onClick.AddListener(OnLoginClick);
        statusTxt.text = ""; // Xóa thông báo khi bắt đầu
    }

    private void ShowStatus(string message, bool isError = true)
    {
        statusTxt.text = message;
        statusTxt.color = isError ? Color.red : Color.green;
    }

    private async void OnRegisterClick()
    {
        string account = accountInput.text.Trim();
        string password = passwordInput.text;

        // Kiểm tra đầu vào
        if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(password))
        {
            ShowStatus("Vui lòng nhập đầy đủ Tài khoản và Mật khẩu!");
            return;
        }

        if (password.Length < 6)
        {
            ShowStatus("Mật khẩu phải có ít nhất 6 ký tự!");
            return;
        }

        string email = account.Contains("@") ? account : $"{account}@game.com";
        
        ShowStatus("Đang xử lý đăng ký...", false);
        registerButton.interactable = false;
        
        try 
        {
            await AuthManager.Instance.SignUpAndCreateAccount(email, password, account);
            // Nếu thành công, AuthManager sẽ tự chuyển scene, hoặc bạn có thể hiện thông báo ở đây
        }
        catch (System.Exception e)
        {
            ShowStatus(ParseError(e.Message));
        }
        finally
        {
            registerButton.interactable = true;
        }
    }

    private async void OnLoginClick()
    {
        string account = accountInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(password))
        {
            ShowStatus("Vui lòng nhập đầy đủ Tài khoản và Mật khẩu!");
            return;
        }

        string email = account.Contains("@") ? account : $"{account}@game.com";

        ShowStatus("Đang đăng nhập...", false);
        loginButton.interactable = false;

        try
        {
            await AuthManager.Instance.SignIn(email, password);
        }
        catch (System.Exception e)
        {
            ShowStatus(ParseError(e.Message));
        }
        finally
        {
            loginButton.interactable = true;
        }
    }

    // Hàm để dịch các lỗi tiếng Anh từ Supabase sang tiếng Việt dễ hiểu
    private string ParseError(string error)
    {
        if (error.Contains("Invalid login credentials")) return "Sai tài khoản hoặc mật khẩu!";
        if (error.Contains("User already registered")) return "Tài khoản này đã tồn tại!";
        if (error.Contains("Email not confirmed")) return "Vui lòng xác nhận Email!";
        return "Có lỗi xảy ra: " + error;
    }
}
