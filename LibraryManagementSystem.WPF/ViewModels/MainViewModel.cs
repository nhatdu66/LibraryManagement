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

		public ICommand LogoutCommand { get; }

		public MainViewModel(LoginViewModel loginVM, IBorrowService borrowService, IAuthService authService)
		{
			LoginVM = loginVM ?? throw new ArgumentNullException(nameof(loginVM));

			// Khởi tạo BorrowVM
			BorrowVM = new BorrowViewModel(borrowService, authService);

			LogoutCommand = new RelayCommand(ExecuteLogout);

			// Theo dõi login success
			LoginVM.PropertyChanged += LoginVM_PropertyChanged;

			Debug.WriteLine("[DEBUG] MainViewModel initialized and subscribed to LoginVM");
		}

		private void LoginVM_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(LoginVM.LoginSuccessTriggered) && LoginVM.LoginSuccessTriggered)
			{
				IsLoggedIn = true;
				WelcomeMessage = $"Chào mừng {LoginVM.LoginSuccessFullName}";
				SelectedTabIndex = 1; // Chuyển sang tab Danh sách sách
				LoginVM.ClearLoginSuccessTriggered();

				Debug.WriteLine($"[DEBUG] Login success detected - IsLoggedIn set to true, Welcome: {WelcomeMessage}");
			}
		}

		private void ExecuteLogout(object parameter)
		{
			IsLoggedIn = false;
			WelcomeMessage = "Chưa đăng nhập";
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