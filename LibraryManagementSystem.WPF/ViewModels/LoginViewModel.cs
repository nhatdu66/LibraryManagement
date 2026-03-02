using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Windows;
using System.Windows.Input;
using LibraryManagementSystem.Services.DTOs;
using LibraryManagementSystem.Services.Interfaces;
using LibraryManagementSystem.WPF.Helpers;

using System;
using System.Windows;
using System.Windows.Input;
using LibraryManagementSystem.Services.DTOs;
using LibraryManagementSystem.Services.Interfaces;
using LibraryManagementSystem.WPF.Helpers;

namespace LibraryManagementSystem.WPF.ViewModels
{
	public class LoginViewModel : ObservableObject
	{
		private readonly IAuthService _authService;
		private readonly MainViewModel _mainViewModel;

		private string _email = "";
		private string _password = "";
		private string _statusMessage = "";

		public string Email
		{
			get => _email;
			set => SetProperty(ref _email, value);
		}

		public string Password
		{
			get => _password;
			set => SetProperty(ref _password, value);
		}

		public string StatusMessage
		{
			get => _statusMessage;
			set => SetProperty(ref _statusMessage, value);
		}

		public ICommand LoginCommand { get; }

		public LoginViewModel(IAuthService authService, MainViewModel mainViewModel)
		{
			_authService = authService;
			_mainViewModel = mainViewModel;

			LoginCommand = new RelayCommand(Login, CanLogin);
		}

		private bool CanLogin(object parameter)
		{
			return !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
		}

		private async void Login(object parameter)
		{
			try
			{
				var response = await _authService.LoginAsync(new LoginDto
				{
					Email = Email,
					Password = Password
				});

				StatusMessage = response.Message;

				if (response.UserId != 0) // Login thành công
				{
					_mainViewModel.OnLoginSuccess(response.FullName, response.RoleName);
				}
			}
			catch (Exception ex)
			{
				StatusMessage = $"Lỗi: {ex.Message}";
			}
		}
	}
}
