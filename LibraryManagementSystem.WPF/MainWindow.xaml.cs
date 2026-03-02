using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using LibraryManagementSystem.Services.DTOs;
using LibraryManagementSystem.Services.Interfaces;

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

			LoadBooks(); // Load danh sách sách khi mở app
		}

		private async void LoadBooks()
		{
			try
			{
				var books = await _bookService.GetAllBookWorksAsync();
				lvBooks.ItemsSource = books ?? new List<BookWorkDto>();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Lỗi load sách: {ex.Message}");
			}
		}

		private async void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
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
				var response = await _authService.LoginAsync(new LoginDto
				{
					Email = email,
					Password = password
				});

				txtLoginStatus.Text = response.Message;

				if (response.UserId != 0) // Login thành công
				{
					txtUserInfo.Text = $"Đăng nhập: {response.FullName} ({response.RoleName})";

					// Enable tất cả tab trừ tab Đăng Nhập
					for (int i = 1; i < tabControl.Items.Count; i++)
					{
						((TabItem)tabControl.Items[i]).IsEnabled = true;
					}

					// Chuyển sang tab Danh Sách Sách
					tabControl.SelectedIndex = 1;

					// Hiện nút Đăng xuất
					btnLogout.Visibility = Visibility.Visible;

					MessageBox.Show($"Đăng nhập thành công!\nChào {response.FullName} ({response.RoleName})",
									"Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
				}
			}
			catch (Exception ex)
			{
				txtLoginStatus.Text = ex.Message;
			}
		}

		// Nút Đăng xuất
		private void BtnLogout_Click(object sender, RoutedEventArgs e)
		{
			// Reset trạng thái đăng nhập
			txtUserInfo.Text = "Chưa đăng nhập";
			txtLoginStatus.Text = "Đã đăng xuất thành công";

			// Disable các tab (chỉ giữ tab Đăng Nhập)
			for (int i = 1; i < tabControl.Items.Count; i++)
			{
				((TabItem)tabControl.Items[i]).IsEnabled = false;
			}

			// Chuyển về tab Đăng Nhập
			tabControl.SelectedIndex = 0;

			// Ẩn nút Đăng xuất
			btnLogout.Visibility = Visibility.Collapsed;

			// Xóa input đăng nhập cũ (tùy chọn)
			txtEmail.Clear();
			txtPassword.Clear();

			MessageBox.Show("Đã đăng xuất thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
		}

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