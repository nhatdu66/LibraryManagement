using System;
using System.Windows.Input;
using LibraryManagementSystem.WPF.Helpers;
using LibraryManagementSystem.WPF.ViewModels;
using System.Diagnostics;

namespace LibraryManagementSystem.WPF.ViewModels
{
	public class MainViewModel : ObservableObject
	{
		public LoginViewModel LoginVM { get; }

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

		public MainViewModel(LoginViewModel loginVM)
		{
			LoginVM = loginVM;

			LogoutCommand = new RelayCommand(ExecuteLogout);

			LoginVM.PropertyChanged += (s, e) =>
			{
				Debug.WriteLine($"[DEBUG] PropertyChanged: {e.PropertyName}");

				if (e.PropertyName == nameof(LoginViewModel.LoginSuccessTriggered) && LoginVM.LoginSuccessTriggered)
				{
					IsLoggedIn = true;
					WelcomeMessage = $"Chào mừng {LoginVM.LoginSuccessFullName}";
					SelectedTabIndex = 1;           // chuyển sang tab Danh sách sách
					LoginVM.ClearLoginSuccessTriggered();
					Debug.WriteLine("[DEBUG] MainViewModel UPDATED SUCCESSFULLY!");
				}
			};

			Debug.WriteLine($"[DEBUG] MainViewModel subscribed to LoginVM");
		}

		private void ExecuteLogout(object parameter)
		{
			IsLoggedIn = false;
			WelcomeMessage = "Chưa đăng nhập";
			SelectedTabIndex = 0;
			LoginVM.Email = "";
			LoginVM.StatusMessage = "";
		}
	}
}