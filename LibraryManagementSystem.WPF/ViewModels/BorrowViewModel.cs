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
using LibraryManagementSystem.WPF.Views;
using Microsoft.Extensions.DependencyInjection;

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

		// Phần mới: Property cho item được chọn trong DataGrid
		private BorrowTransactionDto? _selectedTransaction;
		public BorrowTransactionDto? SelectedTransaction
		{
			get => _selectedTransaction;
			set
			{
				SetProperty(ref _selectedTransaction, value);
				CommandManager.InvalidateRequerySuggested(); // ← THÊM DÒNG NÀY
			}
		}

		// Phần mới: 3 command cho các nút Tạo / Cập nhật / Xóa
		public ICommand CreateDirectBorrowCommand { get; }
		public ICommand UpdateBorrowCommand { get; }
		public ICommand DeleteBorrowCommand { get; }

		public BorrowViewModel(IBorrowService borrowService, IAuthService authService)
		{
			_borrowService = borrowService ?? throw new ArgumentNullException(nameof(borrowService));
			_authService = authService ?? throw new ArgumentNullException(nameof(authService));

			// Optional: Thay bằng logging
			Debug.WriteLine("BorrowViewModel đã được khởi tạo!");

			RefreshTransactionsCommand = new RelayCommand(async _ => await LoadTransactionsAsync());

			// Phần mới: Khởi tạo 3 command
			CreateDirectBorrowCommand = new RelayCommand(_ => CreateDirectBorrow(), _ => CanManageBorrowTransactions());
			UpdateBorrowCommand = new RelayCommand(_ => UpdateSelectedBorrow(), _ => CanEditSelectedBorrow());
			DeleteBorrowCommand = new RelayCommand(_ => DeleteSelectedBorrow(), _ => CanDeleteSelectedBorrow());

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


		private async void UpdateSelectedBorrow()
		{
			if (SelectedTransaction == null)
			{
				StatusMessage = "Vui lòng chọn một giao dịch để cập nhật.";
				return;
			}

			var window = new UpdateBorrowTransactionWindow();
			var vm = App.ServiceProvider.GetRequiredService<UpdateBorrowTransactionViewModel>();
			vm.BorrowId = SelectedTransaction.BorrowId;
			window.DataContext = vm;

			// Load dữ liệu async
			await vm.LoadAsync();

			window.ShowDialog();

			// Refresh danh sách
			await LoadTransactionsAsync();
		}

		private async void DeleteSelectedBorrow()
		{
			if (SelectedTransaction == null)
			{
				StatusMessage = "Vui lòng chọn một giao dịch để xóa.";
				return;
			}

			var result = MessageBox.Show(
				$"Bạn có chắc muốn xóa giao dịch #{SelectedTransaction.BorrowId}?\nHành động này không thể hoàn tác.",
				"Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

			if (result == MessageBoxResult.Yes)
			{
				try
				{
					StatusMessage = $"Đang xóa giao dịch #{SelectedTransaction.BorrowId}...";
					await _borrowService.DeleteBorrowTransactionAsync(SelectedTransaction.BorrowId);
					StatusMessage = $"Đã xóa giao dịch #{SelectedTransaction.BorrowId} thành công!";
					await LoadTransactionsAsync();
				}
				catch (Exception ex)
				{
					StatusMessage = $"Lỗi xóa: {ex.Message}";
					MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		// Helper kiểm tra quyền / điều kiện enable nút
		private bool CanManageBorrowTransactions()
		{
			// Giả sử bạn có cách lấy role từ login (có thể cần thêm property RoleName hoặc dùng IAuthService)
			// Tạm thời cho phép Librarian/Staff/Admin
			return true; // Thay bằng logic thực tế: CurrentRole == "Librarian" || ...
		}

		private bool CanEditSelectedBorrow()
		{
			return SelectedTransaction != null &&
				   CanManageBorrowTransactions() &&
				   SelectedTransaction.Status != "FullyReturned" &&
				   SelectedTransaction.Status != "Cancelled";
		}

		private bool CanDeleteSelectedBorrow()
		{
			return SelectedTransaction != null &&
				   CanManageBorrowTransactions() &&
				   SelectedTransaction.Status == "Borrowed" &&
				   (SelectedTransaction.Details?.All(d => d.ReturnDate == null) ?? true);
		}
		private void CreateDirectBorrow()
		{
			var window = new CreateBorrowTransactionWindow();
			window.DataContext = App.ServiceProvider.GetRequiredService<CreateBorrowTransactionViewModel>();

			// Load dữ liệu async nếu cần
			// if (window.DataContext is CreateBorrowTransactionViewModel vm)
			// {
			//     _ = vm.LoadReadersAsync();   // ← COMMENT HOẶC XÓA DÒNG NÀY ĐỂ FIX LỖI
			// }

			window.ShowDialog();

			// Sau khi đóng window, refresh danh sách nếu cần
			_ = LoadTransactionsAsync();
		}
	}
}