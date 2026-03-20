using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LibraryManagementSystem.Services.Interfaces;
using LibraryManagementSystem.WPF.Helpers;
using LibraryManagementSystem.WPF.Views;

namespace LibraryManagementSystem.WPF.ViewModels
{
	public class UpdateBorrowTransactionViewModel : ObservableObject
	{
		private readonly IBorrowService _borrowService;

		public int BorrowId { get; set; }
		public int AdditionalDays { get; set; } = 7;
		public string StatusMessage { get; set; } = string.Empty;

		public ICommand SaveCommand { get; }

		public UpdateBorrowTransactionViewModel(IBorrowService borrowService)
		{
			_borrowService = borrowService ?? throw new ArgumentNullException(nameof(borrowService));
			SaveCommand = new RelayCommand(async _ => await SaveAsync());
		}

		private async Task SaveAsync()
		{
			try
			{
				await _borrowService.ExtendDueDateAsync(BorrowId, AdditionalDays);
				StatusMessage = "Gia hạn thành công!";
				MessageBox.Show($"Đã gia hạn thêm {AdditionalDays} ngày cho giao dịch #{BorrowId}", "Thành công",
					MessageBoxButton.OK, MessageBoxImage.Information);

				Application.Current.Windows.OfType<UpdateBorrowTransactionWindow>()
					.FirstOrDefault()?.Close();
			}
			catch (Exception ex)
			{
				StatusMessage = $"Lỗi: {ex.Message}";
				MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}