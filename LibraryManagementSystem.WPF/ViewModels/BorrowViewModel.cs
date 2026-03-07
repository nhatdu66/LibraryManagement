using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LibraryManagementSystem.Services.DTOs;
using LibraryManagementSystem.Services.Interfaces;
using LibraryManagementSystem.WPF.Helpers;

namespace LibraryManagementSystem.WPF.ViewModels
{
	public class BorrowViewModel : ObservableObject
	{
		private readonly IBorrowService _borrowService;
		private readonly IAuthService _authService;

		private ObservableCollection<BorrowTransactionDto> _borrowTransactions = new();
		private string _statusMessage = "Đang tải dữ liệu...";
		private int _transactionCount;

		public ObservableCollection<BorrowTransactionDto> BorrowTransactions
		{
			get => _borrowTransactions;
			set => SetProperty(ref _borrowTransactions, value);
		}

		public string StatusMessage
		{
			get => _statusMessage;
			set => SetProperty(ref _statusMessage, value);
		}

		public int TransactionCount
		{
			get => _transactionCount;
			set => SetProperty(ref _transactionCount, value);
		}

		public ICommand RefreshTransactionsCommand { get; }

		public BorrowViewModel(IBorrowService borrowService, IAuthService authService)
		{
			_borrowService = borrowService ?? throw new ArgumentNullException(nameof(borrowService));
			_authService = authService ?? throw new ArgumentNullException(nameof(authService));

			// Debug ngay đầu constructor
			MessageBox.Show("BorrowViewModel đã được khởi tạo!", "Debug - Constructor", MessageBoxButton.OK, MessageBoxImage.Information);

			RefreshTransactionsCommand = new RelayCommand(async _ => await LoadTransactionsAsync());

			_ = LoadTransactionsAsync();
		}

		private async Task LoadTransactionsAsync()
		{
			// Debug ngay đầu method
			MessageBox.Show("Bắt đầu LoadTransactionsAsync", "Debug - Start Load", MessageBoxButton.OK, MessageBoxImage.Information);

			try
			{
				StatusMessage = "Đang tải danh sách giao dịch...";

				var transactions = await _borrowService.GetAllBorrowTransactionsAsync().ConfigureAwait(false);

				int count = transactions?.Count() ?? 0;
				MessageBox.Show($"Load được {count} giao dịch.\nNếu 0 → DB có thể trống hoặc query sai.",
								"Debug - Kết quả Load", MessageBoxButton.OK, MessageBoxImage.Information);

				var list = transactions?.ToList() ?? new List<BorrowTransactionDto>();

				BorrowTransactions = new ObservableCollection<BorrowTransactionDto>(list);

				TransactionCount = BorrowTransactions.Count;
				StatusMessage = $"Đã tải {TransactionCount} giao dịch.";

				if (TransactionCount == 0)
				{
					MessageBox.Show("Không có giao dịch nào trong DB.\nHãy kiểm tra bảng BorrowTransaction bằng SSMS hoặc thêm data test.",
									"No Data", MessageBoxButton.OK, MessageBoxImage.Warning);
				}
			}
			catch (Exception ex)
			{
				StatusMessage = $"Lỗi: {ex.Message}";
				MessageBox.Show($"Lỗi chi tiết khi load:\n{ex.Message}\n\nStackTrace:\n{ex.StackTrace}",
								"Error Debug - Load Transactions", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}