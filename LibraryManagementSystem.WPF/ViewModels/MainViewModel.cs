// Full code cho MainViewModel.cs (thêm MessageBox debug)
using System;
using System.Windows;
using System.Windows.Input;
using LibraryManagementSystem.WPF.Helpers;
using LibraryManagementSystem.WPF.ViewModels;
using System.Diagnostics;
using LibraryManagementSystem.Services.Interfaces;

namespace LibraryManagementSystem.WPF.ViewModels
{
	public class MainViewModel : ObservableObject
	{
		public LoginViewModel LoginVM { get; }
		public BorrowViewModel BorrowVM { get; }

		private int _selectedTabIndex = 0;
		public int SelectedTabIndex
		{
			get => _selectedTabIndex;
			set => SetProperty(ref _selectedTabIndex, value);
		}

		private bool _isLoggedIn;
		public bool IsLoggedIn
		{
			get => _isLoggedIn;
			set => SetProperty(ref _isLoggedIn, value);
		}

		private string _welcomeMessage = "Chưa đăng nhập";
		public string WelcomeMessage
		{
			get => _welcomeMessage;
			set => SetProperty(ref _welcomeMessage, value);
		}
		private string _roleDisplay = string.Empty;
		public string RoleDisplay
		{
			get => _roleDisplay;
			set => SetProperty(ref _roleDisplay, value);
		}
		public ICommand LogoutCommand { get; }

		public MainViewModel(IAuthService authService, IBorrowService borrowService, IBookService bookService)
		{
			LoginVM = new LoginViewModel(authService);
			BorrowVM = new BorrowViewModel(borrowService, authService);

			LogoutCommand = new RelayCommand(ExecuteLogout);

			LoginVM.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == nameof(LoginVM.LoginSuccessTriggered))
				{
					CheckLoginSuccess();
				}
			};
		}

		private void CheckLoginSuccess()
		{
			if (LoginVM.LoginSuccessTriggered)
			{
				MessageBox.Show("Login success detected in MainViewModel! Proceeding to update UI.",
								"Debug Info", MessageBoxButton.OK, MessageBoxImage.Information);

				IsLoggedIn = true;
				WelcomeMessage = $"Chào mừng {LoginVM.LoginSuccessFullName}";

				// === LOGIC ĐÚNG Ý BẠN: Reader thì hiện ngay, Employee thì lấy RoleName ===
				if (LoginVM.LoginSuccessAccountType?.Trim().Equals("Reader", StringComparison.OrdinalIgnoreCase) == true)
				{
					RoleDisplay = "Bạn là Reader";
				}
				else // Employee
				{
					string role = LoginVM.LoginSuccessRoleName?.Trim() ?? "Staff";

					RoleDisplay = role switch
					{
						"Administrator" => "Bạn là Admin",
						"Librarian" => "Bạn là Librarian",
						"Staff" => "Bạn là Staff",
						_ => "Bạn là " + role
					};
				}

				SelectedTabIndex = 1;
				LoginVM.ClearLoginSuccessTriggered();

				Debug.WriteLine($"[DEBUG] Hiển thị Role: {RoleDisplay} (AccountType={LoginVM.LoginSuccessAccountType}, RoleName={LoginVM.LoginSuccessRoleName})");
			}
		}

		private void ExecuteLogout(object parameter)
		{
			IsLoggedIn = false;
			WelcomeMessage = "Chưa đăng nhập";
			RoleDisplay = string.Empty;
			SelectedTabIndex = 0;
			LoginVM.Email = "";
			LoginVM.StatusMessage = "";
			LoginVM.LoginSuccessTriggered = false;
			LoginVM.LoginSuccessFullName = "";
			LoginVM.LoginSuccessUserId = 0;

			Debug.WriteLine("[DEBUG] Logout executed - IsLoggedIn set to false");
		}
	}
}