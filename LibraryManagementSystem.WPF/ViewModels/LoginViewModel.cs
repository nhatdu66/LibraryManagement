using System;
using System.Threading.Tasks;
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
		public string Email
		{
			get => _email;
			set => SetProperty(ref _email, value);
		}

		private string _password = string.Empty;
		public string Password
		{
			get => _password;
			set => SetProperty(ref _password, value);
		}

		private string _statusMessage = string.Empty;
		public string StatusMessage
		{
			get => _statusMessage;
			set => SetProperty(ref _statusMessage, value);
		}

		private bool _isReaderLogin = true;
		public bool IsReaderLogin
		{
			get => _isReaderLogin;
			set => SetProperty(ref _isReaderLogin, value);
		}

		public ICommand LoginCommand { get; }

		public string LoginSuccessFullName { get; private set; } = "";
		public string LoginSuccessRoleName { get; private set; } = "";
		public string LoginSuccessAccountType { get; private set; } = "";
		private bool _loginSuccessTriggered;
		public bool LoginSuccessTriggered
		{
			get => _loginSuccessTriggered;
			private set => SetProperty(ref _loginSuccessTriggered, value);
		}

		public LoginViewModel(IAuthService authService)
		{
			_authService = authService ?? throw new ArgumentNullException(nameof(authService));
			LoginCommand = new RelayCommand(async _ => await ExecuteLoginAsync());
		}

		private async Task ExecuteLoginAsync()
		{
			if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
			{
				StatusMessage = "Vui lòng nhập email và mật khẩu";
				return;
			}

			try
			{
				StatusMessage = "Đang đăng nhập...";

				var loginDto = new LoginDto
				{
					Email = Email.Trim(),
					Password = Password,
					AccountType = IsReaderLogin ? "Reader" : "Employee"
				};

				var result = await _authService.LoginAsync(loginDto);

				if (result.Success)
				{
					LoginSuccessFullName = result.FullName ?? "Unknown";
					LoginSuccessRoleName = result.RoleName ?? "Unknown";
					LoginSuccessAccountType = result.AccountType ?? "Unknown";
					LoginSuccessTriggered = true;

					StatusMessage = $"Đăng nhập {loginDto.AccountType.ToLower()} thành công!";
					Password = "";
				}
				else
				{
					StatusMessage = result.Message ?? "Email hoặc mật khẩu không đúng";
				}
			}
			catch (Exception ex)
			{
				StatusMessage = $"Lỗi: {ex.Message}";
			}
		}

		public void ClearLoginSuccessTriggered()
		{
			LoginSuccessTriggered = false;
		}
	}
}