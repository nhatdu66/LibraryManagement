using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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

			RefreshTransactionsCommand = new RelayCommand(async _ => await LoadTransactionsAsync());

			_ = LoadTransactionsAsync();
		}

		private async Task LoadTransactionsAsync()
		{
			try
			{
				StatusMessage = "Đang tải danh sách giao dịch...";

				// Tạm thời lấy tất cả (sau này lọc theo reader nếu cần)
				var transactions = await _borrowService.GetReaderBorrowHistoryAsync(0); // 0 = all, sửa sau

				var list = transactions.ToList();

				// Tạo chuỗi chi tiết ngay trong ViewModel (không cần thêm property vào DTO)
				var enhancedList = list.Select(t => new
				{
					Transaction = t,
					DetailsString = string.Join(" | ", t.Details.Select(d =>
						$"{d.Title} (Due: {d.DueDate:dd/MM/yyyy}) {(d.ReturnDate.HasValue ? "✓ Trả" : "Chưa trả")}"))
				}).ToList();

				// Gán vào ObservableCollection (giữ nguyên DTO gốc)
				BorrowTransactions = new ObservableCollection<BorrowTransactionDto>(enhancedList.Select(x => x.Transaction));

				// Để hiển thị DetailsString trong XAML, ta sẽ dùng Binding với Converter hoặc MultiBinding (cách đơn giản dưới đây)
				// Nhưng để nhanh → tạm thời ta sẽ sửa XAML dùng StringFormat + Converter (xem phần XAML sửa)

				TransactionCount = BorrowTransactions.Count;
				StatusMessage = $"Đã tải {TransactionCount} giao dịch.";
			}
			catch (Exception ex)
			{
				StatusMessage = $"Lỗi: {ex.Message}";
			}
		}
	}
}