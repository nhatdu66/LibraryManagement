// Full code cho LoginViewModel.cs (thêm MessageBox debug nếu login fail)
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LibraryManagementSystem.Services.DTOs;
using LibraryManagementSystem.Services.Interfaces;
using LibraryManagementSystem.WPF.Helpers;

namespace LibraryManagementSystem.WPF.ViewModels
{
	public class LoginViewModel : ObservableObject
	{
		private readonly IAuthService _authService;
        
        private string _email = string.Empty;
		private string _statusMessage = string.Empty;
		private bool _isReaderLogin = true;

		private bool _loginSuccessTriggered;
		private string _loginSuccessFullName = string.Empty;
		private int _loginSuccessUserId;
		private string _loginSuccessAccountType = string.Empty;
		private string _loginSuccessRoleName = string.Empty;

		public string Email
		{
			get => _email;
			set => SetProperty(ref _email, value);
		}

		public string StatusMessage
		{
			get => _statusMessage;
			set => SetProperty(ref _statusMessage, value);
		}

		public bool IsReaderLogin
		{
			get => _isReaderLogin;
			set => SetProperty(ref _isReaderLogin, value);
		}

		public bool LoginSuccessTriggered
		{
			get => _loginSuccessTriggered;
			set => SetProperty(ref _loginSuccessTriggered, value);
		}

		public string LoginSuccessFullName
		{
			get => _loginSuccessFullName;
			set => SetProperty(ref _loginSuccessFullName, value);
		}

		public int LoginSuccessUserId
		{
			get => _loginSuccessUserId;
			set => SetProperty(ref _loginSuccessUserId, value);
		}

		public string LoginSuccessAccountType
		{
			get => _loginSuccessAccountType;
			set => SetProperty(ref _loginSuccessAccountType, value);
		}

		public string LoginSuccessRoleName   // ← THÊM PROPERTY NÀY
		{
			get => _loginSuccessRoleName;
			set => SetProperty(ref _loginSuccessRoleName, value);
		}

		public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public LoginViewModel(IAuthService authService)
		{
			_authService = authService;

			LoginCommand = new RelayCommand(ExecuteLogin);
            RegisterCommand = new RelayCommand(ExecuteRegister);
        }

		private async void ExecuteLogin(object parameter)
		{
			if (parameter is not PasswordBox passwordBox)
			{
				StatusMessage = "Lỗi: Không lấy được trường mật khẩu.";
				return;
			}

			string password = passwordBox.Password?.Trim() ?? "";
			string email = Email?.Trim() ?? "";

			if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
			{
				StatusMessage = "Vui lòng nhập đầy đủ email và mật khẩu.";
				return;
			}

			try
			{
				StatusMessage = "Đang kiểm tra đăng nhập...";

				var dto = new LoginDto
				{
					Email = email,
					Password = password,
					AccountType = IsReaderLogin ? "Reader" : "Employee"
				};

				var result = await _authService.LoginAsync(dto);

				// Debug MessageBox để hiển thị CHI TIẾT result từ service (luôn hiện, dù success hay fail)
				MessageBox.Show(
					$"Kết quả từ AuthService.LoginAsync:\n" +
					$"Success: {result.Success}\n" +
					$"Message: {result.Message}\n" +
					$"UserId: {result.UserId}\n" +
					$"FullName: {result.FullName ?? "N/A"}\n" +
					$"AccountType: {result.AccountType ?? "N/A"}\n" +
					$"RoleName: {result.RoleName ?? "N/A"}",
					"Debug - Kết quả Login",
					MessageBoxButton.OK,
					result.Success ? MessageBoxImage.Information : MessageBoxImage.Warning
				);

				if (result.Success)
				{
					_authService.SetCurrentUser(result); 
					
					LoginSuccessFullName = result.FullName ?? "Unknown";
					LoginSuccessAccountType = result.AccountType ?? "Unknown";
					LoginSuccessUserId = result.UserId;
					LoginSuccessRoleName = result.RoleName ?? "Reader";

					Debug.WriteLine($"[DEBUG] LOGIN SUCCESS - UserId: {LoginSuccessUserId}, Type: {LoginSuccessAccountType}");

					// Trigger login success để MainViewModel nhận biết và thay đổi UI
					LoginSuccessTriggered = true;
					StatusMessage = "Đăng nhập thành công!";

					// Clear password field sau login thành công
					passwordBox.Password = "";
				}
				else
				{
					StatusMessage = result.Message ?? "Đăng nhập thất bại";
					// MessageBox debug bổ sung nếu fail
					MessageBox.Show($"Login failed: {result.Message}. Check DB for user/email: {Email}", "Debug Info", MessageBoxButton.OK, MessageBoxImage.Warning);
				}
			}
			catch (Exception ex)
			{
				StatusMessage = "Lỗi: " + ex.Message;
				Debug.WriteLine("[DEBUG] Login exception: " + ex.Message);
				// Debug MessageBox cho exception
				MessageBox.Show($"Exception during login: {ex.Message}. Stack: {ex.StackTrace}", "Debug Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public void ClearLoginSuccessTriggered()
		{
			LoginSuccessTriggered = false;
		}
        private async void ExecuteRegister(object parameter)
        {
            if (parameter is not PasswordBox passwordBox)
            {
                StatusMessage = "Không lấy được mật khẩu.";
                return;
            }

            string email = Email?.Trim() ?? "";
            string password = passwordBox.Password?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                StatusMessage = "Vui lòng nhập email và mật khẩu để đăng ký.";
                return;
            }

            try
            {
                StatusMessage = "Đang đăng ký...";

                var dto = new RegisterDto
                {
                    Email = email,
                    Password = password
                };

                // GỌI ĐÚNG HÀM THEO BÀI
                var result = await _authService.RegisterReaderAsync(dto);

                MessageBox.Show(
                $"Register Result\nMessage: {result.Message}",
                   "Debug Register",
                   MessageBoxButton.OK,
                   MessageBoxImage.Information
                );

                StatusMessage = result.Message ?? "Đăng ký xong.";

                passwordBox.Password = "";
            }
            catch (Exception ex)
            {
                StatusMessage = "Lỗi đăng ký: " + ex.Message;

                MessageBox.Show(
                    $"Exception during register:\n{ex.Message}",
                    "Register Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}