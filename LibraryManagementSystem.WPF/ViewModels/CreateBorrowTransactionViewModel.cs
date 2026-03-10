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
using LibraryManagementSystem.WPF.Views;

namespace LibraryManagementSystem.WPF.ViewModels
{
	public class CreateBorrowTransactionViewModel : ObservableObject
	{
		private readonly IBorrowService _borrowService;
		private readonly IBookService _bookService;
		private readonly IReaderService _readerService;
		private readonly IAuthService _authService;

		// Tìm kiếm độc giả
		private string _searchReaderKeyword = "";
		public string SearchReaderKeyword
		{
			get => _searchReaderKeyword;
			set => SetProperty(ref _searchReaderKeyword, value);
		}

		public ObservableCollection<ReaderDto> ReaderSearchResults { get; } = new ObservableCollection<ReaderDto>();

		private ReaderDto? _selectedReader;
		public ReaderDto? SelectedReader
		{
			get => _selectedReader;
			set => SetProperty(ref _selectedReader, value);
		}

		// Tìm kiếm sách
		private string _searchBookKeyword = "";
		public string SearchBookKeyword
		{
			get => _searchBookKeyword;
			set => SetProperty(ref _searchBookKeyword, value);
		}

		public ObservableCollection<BookWorkDto> BookSearchResults { get; } = new ObservableCollection<BookWorkDto>();

		private BookWorkDto? _selectedBookWork;
		public BookWorkDto? SelectedBookWork
		{
			get => _selectedBookWork;
			set => SetProperty(ref _selectedBookWork, value);
		}

		// Danh sách chi tiết mượn tạm thời
		public ObservableCollection<BorrowDetailTemp> BorrowDetails { get; } = new ObservableCollection<BorrowDetailTemp>();

		private DateTime _borrowDate = DateTime.Today;
		public DateTime BorrowDate
		{
			get => _borrowDate;
			set => SetProperty(ref _borrowDate, value);
		}

		private int _borrowDurationDays = 14;
		public int BorrowDurationDays
		{
			get => _borrowDurationDays;
			set => SetProperty(ref _borrowDurationDays, value);
		}

		private string _statusMessage = "Nhập thông tin để tạo giao dịch...";
		public string StatusMessage
		{
			get => _statusMessage;
			set => SetProperty(ref _statusMessage, value);
		}

		// Commands
		public ICommand SearchReaderCommand { get; }
		public ICommand SelectReaderCommand { get; }
		public ICommand SearchBookCommand { get; }
		public ICommand AddBookCommand { get; }
		public ICommand SaveTransactionCommand { get; }

		public CreateBorrowTransactionViewModel(
			IBorrowService borrowService,
			IBookService bookService,
			IReaderService readerService,
			IAuthService authService)
		{
			_borrowService = borrowService ?? throw new ArgumentNullException(nameof(borrowService));
			_bookService = bookService ?? throw new ArgumentNullException(nameof(bookService));
			_readerService = readerService ?? throw new ArgumentNullException(nameof(readerService));
			_authService = authService ?? throw new ArgumentNullException(nameof(authService));

			SearchReaderCommand = new RelayCommand(_ => SearchReaders());
			SelectReaderCommand = new RelayCommand(o => SelectReader(o as ReaderDto));
			SearchBookCommand = new RelayCommand(_ => SearchBooks());
			AddBookCommand = new RelayCommand(_ => AddSelectedBook());
			SaveTransactionCommand = new RelayCommand(_ => SaveTransaction());
		}

		private async void SearchReaders()
		{
			if (string.IsNullOrWhiteSpace(SearchReaderKeyword) || SearchReaderKeyword.Length < 2)
			{
				StatusMessage = "Nhập ít nhất 2 ký tự để tìm độc giả.";
				return;
			}

			try
			{
				StatusMessage = "Đang tìm độc giả...";

				// Giả định IReaderService có method GetAllReadersAsync (hoặc SearchReadersAsync)
				// Nếu chưa có, bạn cần implement
				var allReaders = await _readerService.GetAllReadersAsync(); // Thay bằng method thực nếu có

				var results = allReaders
					.Where(r => r.FullName.Contains(SearchReaderKeyword, StringComparison.OrdinalIgnoreCase) ||
								r.Email.Contains(SearchReaderKeyword, StringComparison.OrdinalIgnoreCase) ||
								r.CardNumber.Contains(SearchReaderKeyword, StringComparison.OrdinalIgnoreCase))
					.Take(15)
					.ToList();

				ReaderSearchResults.Clear();
				foreach (var reader in results)
				{
					ReaderSearchResults.Add(reader);
				}

				StatusMessage = $"Tìm thấy {results.Count} độc giả.";
			}
			catch (Exception ex)
			{
				StatusMessage = $"Lỗi tìm độc giả: {ex.Message}";
			}
		}

		private void SelectReader(ReaderDto? reader)
		{
			if (reader == null) return;

			SelectedReader = reader;
			StatusMessage = $"Đã chọn độc giả: {reader.FullName} (Mã thẻ: {reader.CardNumber})";
		}

		private async void SearchBooks()
		{
			if (string.IsNullOrWhiteSpace(SearchBookKeyword))
			{
				StatusMessage = "Nhập từ khóa tìm sách.";
				return;
			}

			try
			{
				StatusMessage = "Đang tìm sách...";

				var results = await _bookService.SearchBooksAsync(SearchBookKeyword, null, null, null);

				BookSearchResults.Clear();
				foreach (var book in results)
				{
					BookSearchResults.Add(book);
				}

				StatusMessage = $"Tìm thấy {results.Count()} sách.";
			}
			catch (Exception ex)
			{
				StatusMessage = $"Lỗi tìm sách: {ex.Message}";
			}
		}

		private void AddSelectedBook()
		{
			if (SelectedBookWork == null)
			{
				StatusMessage = "Vui lòng chọn một sách.";
				return;
			}

			// Placeholder cho EditionInfo (vì BookWorkDto không có PublishYear)
			// Bạn có thể thay bằng AuthorsString hoặc thông tin khác có sẵn
			string editionInfo = SelectedBookWork.AuthorsString != null && SelectedBookWork.AuthorsString.Length > 0
				? $"Tác giả: {SelectedBookWork.AuthorsString}"
				: "Edition cơ bản";

			BorrowDetails.Add(new BorrowDetailTemp
			{
				Title = SelectedBookWork.Title,
				EditionInfo = editionInfo,
				CopyId = 0,              // Placeholder - sau này thay bằng CopyId thực
				ShelfLocation = "Kệ A-01" // Placeholder - sau này lấy từ BookCopy
			});

			StatusMessage = $"Đã thêm \"{SelectedBookWork.Title}\" vào danh sách.";

			// Reset sau khi thêm
			SelectedBookWork = null;
			SearchBookKeyword = "";
		}

		private async void SaveTransaction()
		{
			if (SelectedReader == null)
			{
				StatusMessage = "Vui lòng chọn độc giả.";
				return;
			}

			if (BorrowDetails.Count == 0)
			{
				StatusMessage = "Chưa có sách nào trong danh sách.";
				return;
			}

			try
			{
				StatusMessage = "Đang lưu giao dịch...";

				// Lấy employeeId của thủ thư hiện tại (cần implement lấy từ login/auth)
				// Tạm hard-code để test
				int employeeId = 1; // <-- THAY BẰNG CÁCH LẤY THỰC TẾ SAU NÀY

				// Tạo list copyIds (hiện tại placeholder)
				var copyIds = new List<int>(); // Sau này lấy từ BorrowDetails.CopyId thực

				// Gọi service tạo transaction (cần implement method này trong BorrowService)
				// var transaction = await _borrowService.CreateDirectBorrowTransactionAsync(
				//     SelectedReader.ReaderId, employeeId, BorrowDate, BorrowDate.AddDays(BorrowDurationDays), copyIds);

				// Placeholder thành công
				MessageBox.Show("Giao dịch đã được tạo thành công (placeholder).",
					"Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

				// Đóng window
				Application.Current.Windows.OfType<CreateBorrowTransactionWindow>().FirstOrDefault()?.Close();
			}
			catch (Exception ex)
			{
				StatusMessage = $"Lỗi khi tạo giao dịch: {ex.Message}";
				MessageBox.Show(StatusMessage, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}

	// Class tạm để hiển thị trong DataGrid
	public class BorrowDetailTemp : ObservableObject
	{
		public string Title { get; set; } = string.Empty;
		public string EditionInfo { get; set; } = string.Empty;
		public int CopyId { get; set; }
		public string ShelfLocation { get; set; } = string.Empty;
	}
}