using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using LibraryManagementSystem.Services.DTOs;
using LibraryManagementSystem.Services.Interfaces;
using LibraryManagementSystem.WPF.Helpers;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using LibraryManagementSystem.Services.DTOs;
using LibraryManagementSystem.Services.Interfaces;
using LibraryManagementSystem.WPF.Helpers;

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using LibraryManagementSystem.Services.DTOs;
using LibraryManagementSystem.Services.Interfaces;
using LibraryManagementSystem.WPF.Helpers;

namespace LibraryManagementSystem.WPF.ViewModels
{
	public class BookCatalogViewModel : ObservableObject
	{
		private readonly IBookService _bookService;

		private ObservableCollection<BookWorkDto> _books = new ObservableCollection<BookWorkDto>();
		private string _searchKeyword = string.Empty;
		private string _statusMessage = string.Empty;

		public ObservableCollection<BookWorkDto> Books
		{
			get => _books;
			set => SetProperty(ref _books, value);
		}

		public string SearchKeyword
		{
			get => _searchKeyword;
			set => SetProperty(ref _searchKeyword, value);
		}

		public string StatusMessage
		{
			get => _statusMessage;
			set => SetProperty(ref _statusMessage, value);
		}

		public ICommand SearchCommand { get; }
		public ICommand LoadAllCommand { get; }

		public BookCatalogViewModel(IBookService bookService)
		{
			_bookService = bookService ?? throw new ArgumentNullException(nameof(bookService));

			SearchCommand = new RelayCommand(_ => SearchBooks());
			LoadAllCommand = new RelayCommand(_ => LoadAllBooks());

			// Load dữ liệu async khi ViewModel được tạo
			_ = LoadAllBooksAsync();  // fire-and-forget
		}

		private async Task LoadAllBooksAsync()
		{
			try
			{
				StatusMessage = "Đang tải danh sách sách...";
				var allBooks = await _bookService.GetAllBookWorksAsync();
				Books = new ObservableCollection<BookWorkDto>(allBooks);
				StatusMessage = $"Tải thành công: {Books.Count} sách.";
			}
			catch (Exception ex)
			{
				StatusMessage = $"Lỗi tải sách: {ex.Message}";
			}
		}

		private async void LoadAllBooks()
		{
			await LoadAllBooksAsync();
		}

		private async void SearchBooks()
		{
			try
			{
				StatusMessage = "Đang tìm kiếm...";
				if (string.IsNullOrWhiteSpace(SearchKeyword))
				{
					await LoadAllBooksAsync();
					return;
				}

				var results = await _bookService.SearchBooksAsync(SearchKeyword, null, null, null);
				Books = new ObservableCollection<BookWorkDto>(results);
				StatusMessage = $"Tìm thấy: {Books.Count} kết quả.";
			}
			catch (Exception ex)
			{
				StatusMessage = $"Lỗi tìm kiếm: {ex.Message}";
			}
		}
	}
}