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

namespace LibraryManagementSystem.WPF.ViewModels
{
	public class BookCatalogViewModel : ObservableObject
	{
		private readonly IBookService _bookService;

		private ObservableCollection<BookWorkDto> _books = new ObservableCollection<BookWorkDto>();
		private string _searchKeyword = "";

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
				SearchBooks(); // Tự động tìm khi gõ
			}
		}

		public ICommand SearchCommand { get; }

		public BookCatalogViewModel(IBookService bookService)
		{
			_bookService = bookService;
			//SearchCommand = new RelayCommand(SearchBooks);
			SearchCommand = new RelayCommand(_ => SearchBooks());
			LoadAllBooks(); // Load ban đầu
		}

		private async void LoadAllBooks()
		{
			try
			{
				var allBooks = await _bookService.GetAllBookWorksAsync();
				Books = new ObservableCollection<BookWorkDto>(allBooks);
			}
			catch (Exception ex)
			{
				// Có thể show MessageBox ở đây
			}
		}

		private async void SearchBooks()
		{
			try
			{
				if (string.IsNullOrWhiteSpace(SearchKeyword))
				{
					LoadAllBooks();
					return;
				}

				var results = await _bookService.SearchBooksAsync(SearchKeyword, null, null, null);
				Books = new ObservableCollection<BookWorkDto>(results);
			}
			catch (Exception ex)
			{
				// Show lỗi
			}
		}
	}
}
