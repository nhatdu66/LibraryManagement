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
		private string _loginSuccessRoleName = string.Empty;
		private string _loginSuccessAccountType = string.Empty;
		private int _loginSuccessUserId;

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

		public string LoginSuccessRoleName
		{
			get => _loginSuccessRoleName;
			set => SetProperty(ref _loginSuccessRoleName, value);
		}

		public string LoginSuccessAccountType
		{
			get => _loginSuccessAccountType;
			set => SetProperty(ref _loginSuccessAccountType, value);
		}

		public int LoginSuccessUserId
		{
			get => _loginSuccessUserId;
			set => SetProperty(ref _loginSuccessUserId, value);
		}

		public ICommand LoginCommand { get; }

		public LoginViewModel(IAuthService authService)
		{
			_authService = authService;
			LoginCommand = new RelayCommand(LoginAsync);
		}

		private async void LoginAsync(object parameter)
		{
			try
			{
				string password = string.Empty;

				// Lấy mật khẩu từ PasswordBox (CommandParameter)
				if (parameter is PasswordBox passwordBox)
				{
					password = passwordBox.Password;
				}

				Debug.WriteLine($"[DEBUG] Login called - Email='{Email}', Password length={password.Length}");

				if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(password))
				{
					StatusMessage = "Lỗi: Email và Password không được để trống";
					return;
				}

				StatusMessage = "Đang đăng nhập...";

				var dto = new LoginDto
				{
					Email = Email.Trim(),
					Password = password,
					AccountType = IsReaderLogin ? "Reader" : "Employee"
				};

				var result = await _authService.LoginAsync(dto);

				if (result.Success)
				{
					LoginSuccessFullName = result.FullName;
					LoginSuccessRoleName = result.RoleName ?? "Unknown";
					LoginSuccessAccountType = result.AccountType ?? "Unknown";
					LoginSuccessUserId = result.UserId;

					Debug.WriteLine($"[DEBUG] LOGIN SUCCESS - UserId: {LoginSuccessUserId}, Type: {LoginSuccessAccountType}");

					// Trigger login success
					LoginSuccessTriggered = true;
					StatusMessage = "Đăng nhập thành công!";
				}
				else
				{
					StatusMessage = result.Message ?? "Đăng nhập thất bại";
				}
			}
			catch (Exception ex)
			{
				StatusMessage = "Lỗi: " + ex.Message;
				Debug.WriteLine("[DEBUG] Login exception: " + ex.Message);
			}
		}

		public void ClearLoginSuccessTriggered()
		{
			LoginSuccessTriggered = false;
		}
	}
}