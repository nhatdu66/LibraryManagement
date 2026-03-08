using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

			// Optional: Thay bằng logging
			Debug.WriteLine("BorrowViewModel đã được khởi tạo!");

			RefreshTransactionsCommand = new RelayCommand(async _ => await LoadTransactionsAsync());

			_ = LoadTransactionsAsync();
		}

		private async Task LoadTransactionsAsync()
		{
			// Optional: Thay bằng logging
			Debug.WriteLine("Bắt đầu LoadTransactionsAsync");

			try
			{
				StatusMessage = "Đang tải danh sách giao dịch...";

				var transactions = await _borrowService.GetAllBorrowTransactionsAsync().ConfigureAwait(false);

				int count = transactions?.Count() ?? 0;
				// Optional: Debug.WriteLine($"Load được {count} giao dịch. Nếu 0 → DB có thể trống hoặc query sai.");

				var list = transactions?.ToList() ?? new List<BorrowTransactionDto>();

				BorrowTransactions = new ObservableCollection<BorrowTransactionDto>(list);

				TransactionCount = BorrowTransactions.Count;
				StatusMessage = $"Đã tải {TransactionCount} giao dịch.";

				if (TransactionCount == 0)
				{
					// Optional: Debug.WriteLine("Không có giao dịch nào trong DB. Hãy kiểm tra bảng BorrowTransaction bằng SSMS hoặc thêm data test.");
				}
			}
			catch (Exception ex)
			{
				StatusMessage = $"Lỗi: {ex.Message}";
				// Optional: Thay bằng logging
				Debug.WriteLine($"Lỗi chi tiết khi load: {ex.Message}\nStackTrace: {ex.StackTrace}");
			}
		}
	}
}