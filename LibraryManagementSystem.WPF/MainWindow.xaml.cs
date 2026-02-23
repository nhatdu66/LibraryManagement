using System;
using System;
using System;
using System.Linq;
using System.Windows;
using System.Windows;
using System.Windows;
using System.Windows.Controls;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Data.Entities;
using LibraryManagementSystem.Repositories;
using LibraryManagementSystem.Services.DTOs;
using LibraryManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LibraryManagementSystem.WPF
{
	public partial class MainWindow : Window
	{
		private readonly IBookService _bookService;
		private readonly IAuthService _authService;

		public MainWindow(IBookService bookService, IAuthService authService)
		{
			InitializeComponent();

			_bookService = bookService;
			_authService = authService;

			// Load danh sách sách ngay khi mở app (test)
			LoadBooks();
		}

		private async void LoadBooks()
		{
			try
			{
				var books = await _bookService.GetAllBookWorksAsync();
				lvBooks.ItemsSource = books ?? new List<BookWorkDto>();
				MessageBox.Show($"Load thành công: {books?.Count() ?? 0} sách");
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Lỗi load sách chi tiết: {ex.Message}\nInner: {ex.InnerException?.Message}\nStack: {ex.StackTrace}");
			}
		}

		// Xử lý tìm kiếm (nếu người dùng gõ vào txtSearch)
		private async void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
		{
			string keyword = txtSearch.Text.Trim();
			if (string.IsNullOrWhiteSpace(keyword))
			{
				LoadBooks(); // Load lại tất cả nếu không có từ khóa
				return;
			}

			try
			{
				var books = await _bookService.SearchBooksAsync(keyword, null, null, null);
				lvBooks.ItemsSource = books;
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Lỗi tìm kiếm: {ex.Message}");
			}
		}

		// Xử lý nút Đăng Nhập (tạm thời, sau này bind Command)
		private async void BtnLogin_Click(object sender, RoutedEventArgs e)
		{
			string email = txtEmail.Text.Trim();
			string password = txtPassword.Password;

			if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
			{
				txtLoginStatus.Text = "Vui lòng nhập email và mật khẩu";
				return;
			}

			try
			{
				var response = await _authService.LoginAsync(new LoginDto { Email = email, Password = password });
				txtLoginStatus.Text = response.Message;
				txtUserInfo.Text = $"Đăng nhập: {response.FullName} ({response.RoleName})";

				// Enable các tab khác sau khi login thành công
				foreach (TabItem tab in (tabControl.Items))
				{
					if (tab.Header.ToString() != "Đăng Nhập")
						tab.IsEnabled = true;
				}
			}
			catch (Exception ex)
			{
				txtLoginStatus.Text = ex.Message;
			}
		}

		// Added missing search button click handler referenced from XAML
		private async void BtnSearch_Click(object sender, RoutedEventArgs e)
		{
			string keyword = txtSearch.Text.Trim();
			if (string.IsNullOrWhiteSpace(keyword))
			{
				LoadBooks();
				return;
			}

			try
			{
				var books = await _bookService.SearchBooksAsync(keyword, null, null, null);
				lvBooks.ItemsSource = books;
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Lỗi tìm kiếm: {ex.Message}");
			}
		}
	}
}