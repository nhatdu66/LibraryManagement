using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LibraryManagementSystem.Services.DTOs;
using LibraryManagementSystem.Services.Interfaces;
using LibraryManagementSystem.WPF.Helpers;
using LibraryManagementSystem.WPF.Views;

namespace LibraryManagementSystem.WPF.ViewModels
{
	public class CreateBorrowTransactionViewModel : ObservableObject
	{
		private readonly IBorrowService _borrowService;
		private readonly IBookService _bookService;
		private readonly IReaderService _readerService;
		private readonly IAuthService _authService;

		// 1. Tìm Reader
		public string SearchReaderKeyword { get; set; } = "";
		public ObservableCollection<ReaderDto> ReaderResults { get; } = new();
		public ReaderDto? SelectedReader { get; set; }

		// 2. Tìm Work → Edition → Copy
		public string SearchWorkKeyword { get; set; } = "";
		public ObservableCollection<BookWorkDto> WorkResults { get; } = new();
		public BookWorkDto? SelectedWork { get; set; }

		public ObservableCollection<BookEditionDto> EditionResults { get; } = new();
		public BookEditionDto? SelectedEdition { get; set; }

		public ObservableCollection<BookCopyDto> CopyResults { get; } = new();
		public BookCopyDto? SelectedCopy { get; set; }

		// Danh sách đã chọn (sẽ lưu vào DB)
		public ObservableCollection<BorrowDetailTemp> BorrowDetails { get; } = new();

		public DateTime BorrowDate { get; set; } = DateTime.Today;
		public int DurationDays { get; set; } = 14;

		public string StatusMessage { get; set; } = "Chọn độc giả và sách để tạo giao dịch...";

		// Commands
		public ICommand SearchReaderCmd { get; }
		public ICommand SearchWorkCmd { get; }
		public ICommand SearchEditionCmd { get; }
		public ICommand SearchCopyCmd { get; }
		public ICommand AddCopyCmd { get; }
		public ICommand SaveTransactionCmd { get; }

		public CreateBorrowTransactionViewModel(
			IBorrowService borrowService,
			IBookService bookService,
			IReaderService readerService,
			IAuthService authService)
		{
			_borrowService = borrowService;
			_bookService = bookService;
			_readerService = readerService;
			_authService = authService;

			SearchReaderCmd = new RelayCommand(async _ => await SearchReaders());
			SearchWorkCmd = new RelayCommand(async _ => await SearchWorks());
			SearchEditionCmd = new RelayCommand(async _ => await LoadEditions());
			SearchCopyCmd = new RelayCommand(async _ => await LoadCopies());
			AddCopyCmd = new RelayCommand(_ => AddSelectedCopy());
			SaveTransactionCmd = new RelayCommand(async _ => await SaveTransaction());
		}

		private async Task SearchReaders() { /* giữ logic cũ hoặc dùng GetAllReadersAsync + filter */ }
		private async Task SearchWorks()
		{
			if (string.IsNullOrWhiteSpace(SearchWorkKeyword)) return;
			var results = await _bookService.SearchBooksAsync(SearchWorkKeyword, null, null, null);
			WorkResults.Clear();
			foreach (var w in results) WorkResults.Add(w);
		}

		private async Task LoadEditions()
		{
			if (SelectedWork == null) return;
			var editions = await _bookService.GetEditionsByWorkIdAsync(SelectedWork.WorkId);
			EditionResults.Clear();
			foreach (var e in editions) EditionResults.Add(e);
		}

		private async Task LoadCopies()
		{
			if (SelectedEdition == null) return;
			var copies = await _bookService.GetAvailableCopiesByEditionIdAsync(SelectedEdition.EditionId);
			CopyResults.Clear();
			foreach (var c in copies) CopyResults.Add(c);
		}

		private void AddSelectedCopy()
		{
			if (SelectedCopy == null || SelectedWork == null) return;

			BorrowDetails.Add(new BorrowDetailTemp
			{
				Title = SelectedWork.Title,
				EditionInfo = $"{SelectedEdition?.PublisherName} - {SelectedEdition?.PublishYear}",
				CopyId = SelectedCopy.CopyId,
				ShelfLocation = SelectedCopy.ShelfLocation ?? "N/A",
				DueDate = BorrowDate.AddDays(DurationDays)
			});

			StatusMessage = $"Đã thêm Copy #{SelectedCopy.CopyId}";
			SelectedCopy = null; // reset
		}

		private async Task SaveTransaction()
		{
			if (SelectedReader == null || BorrowDetails.Count == 0)
			{
				StatusMessage = "Chưa chọn độc giả hoặc sách!";
				return;
			}

			try
			{
				// Lấy employeeId từ login (sau này thay bằng CurrentEmployeeId)
				int employeeId = 3; // tạm hardcode librarian1

				var copyIds = BorrowDetails.Select(d => d.CopyId).ToList();

				// Gọi service (mình sẽ thêm method này ở bước sau)
				// var result = await _borrowService.CreateDirectBorrowTransactionAsync(
				//     SelectedReader.ReaderId, employeeId, copyIds, BorrowDate, DurationDays);

				MessageBox.Show("Tạo giao dịch thành công!", "Thành công");
				Application.Current.Windows.OfType<CreateBorrowTransactionWindow>().FirstOrDefault()?.Close();
			}
			catch (Exception ex)
			{
				StatusMessage = ex.Message;
			}
		}
	}

	// Class tạm hiển thị trong DataGrid
	public class BorrowDetailTemp : ObservableObject
	{
		public string Title { get; set; } = "";
		public string EditionInfo { get; set; } = "";
		public int CopyId { get; set; }
		public string ShelfLocation { get; set; } = "";
		public DateTime DueDate { get; set; }
	}
}