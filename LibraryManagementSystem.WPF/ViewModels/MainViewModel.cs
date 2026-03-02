using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Windows.Input;
using LibraryManagementSystem.WPF.Helpers;

using System.Windows.Input;
using LibraryManagementSystem.WPF.Helpers;

namespace LibraryManagementSystem.WPF.ViewModels
{
	public class MainViewModel : ObservableObject
	{
		private string _currentTab = "Login"; // Tab mặc định
		private bool _isLoggedIn = false;
		private string _userFullName = "Chưa đăng nhập";
		private string _userRole = "";

		public string CurrentTab
		{
			get => _currentTab;
			set => SetProperty(ref _currentTab, value);
		}

		public bool IsLoggedIn
		{
			get => _isLoggedIn;
			set => SetProperty(ref _isLoggedIn, value);
		}

		public string UserFullName
		{
			get => _userFullName;
			set => SetProperty(ref _userFullName, value);
		}

		public string UserRole
		{
			get => _userRole;
			set => SetProperty(ref _userRole, value);
		}

		// Command để switch tab từ XAML
		public ICommand SwitchTabCommand { get; }

		public MainViewModel()
		{
			SwitchTabCommand = new RelayCommand<string>(SwitchTab);
		}

		private void SwitchTab(string tabName)
		{
			if (tabName == "Login" && IsLoggedIn)
				return; // Không cho quay lại Login nếu đã đăng nhập

			CurrentTab = tabName;
		}

		// Gọi khi login thành công từ LoginViewModel
		public void OnLoginSuccess(string fullName, string role)
		{
			IsLoggedIn = true;
			UserFullName = $"Đăng nhập: {fullName}";
			UserRole = $"({role})";
			CurrentTab = "BookCatalog"; // Chuyển sang tab sách sau login
		}
	}
}