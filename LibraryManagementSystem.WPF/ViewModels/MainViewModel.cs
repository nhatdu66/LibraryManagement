using LibraryManagementSystem.WPF.Helpers;
using LibraryManagementSystem.WPF.ViewModels;

namespace LibraryManagementSystem.WPF.ViewModels  // Đảm bảo namespace đúng
{
	public class MainViewModel : ObservableObject
	{
		private string _currentTab = "Login";
		public string CurrentTab
		{
			get => _currentTab;
			set => SetProperty(ref _currentTab, value);
		}

		private bool _isLoggedIn;
		public bool IsLoggedIn
		{
			get => _isLoggedIn;
			set => SetProperty(ref _isLoggedIn, value);
		}

		private string _userFullName = "Chưa đăng nhập";
		public string UserFullName
		{
			get => _userFullName;
			set => SetProperty(ref _userFullName, value);
		}

		private string _userRole = "";
		public string UserRole
		{
			get => _userRole;
			set => SetProperty(ref _userRole, value);
		}

		private readonly LoginViewModel _loginVM;

		public MainViewModel(LoginViewModel loginVM)
		{
			_loginVM = loginVM ?? throw new ArgumentNullException(nameof(loginVM));

			// Subscribe to PropertyChanged của LoginViewModel
			_loginVM.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == nameof(LoginViewModel.LoginSuccessTriggered) && _loginVM.LoginSuccessTriggered)
				{
					IsLoggedIn = true;
					UserFullName = $"Đăng nhập: {_loginVM.LoginSuccessFullName}";
					UserRole = $"({_loginVM.LoginSuccessRoleName} - {_loginVM.LoginSuccessAccountType})";

					CurrentTab = "BookCatalog"; // Chuyển tab
					_loginVM.ClearLoginSuccessTriggered(); // Reset trigger
				}
			};
		}
	}
}