using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using LibraryManagementSystem.Services.DTOs;
using LibraryManagementSystem.Services.Interfaces;
using LibraryManagementSystem.WPF.Helpers;

namespace LibraryManagementSystem.WPF.ViewModels
{
	public class ManageBooksViewModel : ObservableObject
	{
		private readonly IBookService _bookService;

		private ObservableCollection<BookWorkDto> _books = new ObservableCollection<BookWorkDto>();
		private string _searchKeyword = string.Empty;
		private string _statusMessage = "Đang tải danh sách sách...";

		public ObservableCollection<BookWorkDto> Books
		{
			get => _books;
			set => SetProperty(ref _books, value);
		}

		public string SearchKeyword
		{
			get => _searchKeyword;
			set
			{
				SetProperty(ref _searchKeyword, value);
				// Tự động tìm kiếm khi gõ (debounce nếu muốn sau)
				_ = SearchBooksAsync();
			}
		}

		public string StatusMessage
		{
			get => _statusMessage;
			set => SetProperty(ref _statusMessage, value);
		}

		public ICommand SearchCommand { get; }
		public ICommand RefreshCommand { get; }

		public ManageBooksViewModel(IBookService bookService)
		{
			_bookService = bookService ?? throw new ArgumentNullException(nameof(bookService));

			SearchCommand = new RelayCommand(async _ => await SearchBooksAsync());
			RefreshCommand = new RelayCommand(async _ => await LoadAllBooksAsync());

			// Load ban đầu
			_ = LoadAllBooksAsync();
		}

		private async Task LoadAllBooksAsync()
		{
			try
			{
				StatusMessage = "Đang tải toàn bộ sách...";
				var allBooks = await _bookService.GetAllBookWorksAsync();
				Books = new ObservableCollection<BookWorkDto>(allBooks);
				StatusMessage = $"Tổng cộng: {Books.Count} tác phẩm.";
			}
			catch (Exception ex)
			{
				StatusMessage = $"Lỗi tải danh sách: {ex.Message}";
			}
		}

		private async Task SearchBooksAsync()
		{
			try
			{
				StatusMessage = "Đang tìm kiếm...";

				if (string.IsNullOrWhiteSpace(SearchKeyword))
				{
					await LoadAllBooksAsync();
					return;
				}

				// Hiện tại SearchBooksAsync chỉ hỗ trợ keyword, sau này có thể mở rộng thêm authorId, categoryId, seriesId
				var results = await _bookService.SearchBooksAsync(SearchKeyword, null, null, null);
				Books = new ObservableCollection<BookWorkDto>(results);
				StatusMessage = $"Tìm thấy: {Books.Count} kết quả.";
			}
			catch (Exception ex)
			{
				StatusMessage = $"Lỗi tìm kiếm: {ex.Message}";
			}
		}

		// Các command sau này bạn sẽ thêm vào đây (ví dụ):
		// public ICommand AddBookCommand { get; }
		// public ICommand EditBookCommand { get; }
		// public ICommand DeleteBookCommand { get; }
	}
}
