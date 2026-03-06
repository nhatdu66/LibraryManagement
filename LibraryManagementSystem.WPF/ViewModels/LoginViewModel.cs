using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LibraryManagementSystem.Services.DTOs;
using LibraryManagementSystem.Services.Interfaces;
using LibraryManagementSystem.WPF.Helpers;
using System.Diagnostics;

namespace LibraryManagementSystem.WPF.ViewModels
{
	public class LoginViewModel : ObservableObject
	{
		private readonly IAuthService _authService;

		private string _email = string.Empty;
		private string _statusMessage = string.Empty;
		private bool _isReaderLogin = true;
		private bool _isEmployeeLogin = false;

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
			set
			{
				SetProperty(ref _isReaderLogin, value);
				if (value) IsEmployeeLogin = false;
			}
		}

		public bool IsEmployeeLogin
		{
			get => _isEmployeeLogin;
			set
			{
				SetProperty(ref _isEmployeeLogin, value);
				if (value) IsReaderLogin = false;
			}
		}

		public ICommand LoginCommand { get; }

		public string LoginSuccessFullName { get; private set; }
		public string LoginSuccessRoleName { get; private set; }
		public string LoginSuccessAccountType { get; private set; }

		private bool _loginSuccessTriggered;
		public bool LoginSuccessTriggered
		{
			get => _loginSuccessTriggered;
			set => SetProperty(ref _loginSuccessTriggered, value);
		}

		public LoginViewModel(IAuthService authService)
		{
			_authService = authService;
			LoginCommand = new RelayCommand<object>(ExecuteLogin);
			Debug.WriteLine($"[DEBUG] LoginViewModel created - Hash: {GetHashCode()}");
		}

		private async void ExecuteLogin(object parameter)
		{
			if (parameter is not PasswordBox pb) return;

			try
			{
				StatusMessage = "Đang đăng nhập...";

				var dto = new LoginDto
				{
					Email = Email.Trim(),
					Password = pb.Password,
					AccountType = IsReaderLogin ? "Reader" : "Employee"
				};

				var result = await _authService.LoginAsync(dto);

				StatusMessage = result.Message;

				if (result.UserId != 0)
				{
					pb.Clear();
					LoginSuccessFullName = result.FullName ?? "Unknown";
					LoginSuccessRoleName = result.RoleName ?? "Unknown";
					LoginSuccessAccountType = result.AccountType ?? "Unknown";

					Debug.WriteLine($"[DEBUG] LOGIN SUCCESS - Setting Trigger = true (Hash: {GetHashCode()})");
					LoginSuccessTriggered = true;

					StatusMessage = "Đăng nhập thành công!";
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