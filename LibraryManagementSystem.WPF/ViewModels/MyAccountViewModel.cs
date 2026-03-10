using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Input;
using LibraryManagementSystem.Services.Interfaces;
using LibraryManagementSystem.WPF.Helpers;

namespace LibraryManagementSystem.WPF.ViewModels
{
	public class MyAccountViewModel : ObservableObject
	{
		private readonly IReaderAccountService _readerService;
		private readonly IEmployeeAccountService _employeeService;

		private string _fullName = "";
		private string _email = "";
		private string _statusMessage = "";

		public string FullName
		{
			get => _fullName;
			set => SetProperty(ref _fullName, value);
		}

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

		public ICommand LoadProfileCommand { get; }

		public MyAccountViewModel(IReaderAccountService readerService, IEmployeeAccountService employeeService)
		{
			_readerService = readerService;
			_employeeService = employeeService;

			LoadProfileCommand = new RelayCommand(LoadProfile);
		}

		private async void LoadProfile(object parameter)
		{
			// Tạm thời giả định Reader (sau này lấy từ login state)
			try
			{
				var profile = await _readerService.GetReaderByIdAsync(1); // Thay 1 bằng ReaderId thực tế
				if (profile != null)
				{
					FullName = profile.FullName;
					Email = profile.Email;
					StatusMessage = "Đã load profile thành công";
				}
			}
			catch (Exception ex)
			{
				StatusMessage = $"Lỗi: {ex.Message}";
			}
		}
	}
}
