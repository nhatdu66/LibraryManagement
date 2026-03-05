using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using LibraryManagementSystem.Services.DTOs;
using LibraryManagementSystem.Services.Interfaces;

using System.Windows;

namespace LibraryManagementSystem.WPF
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			// Không cần inject service nữa, vì ViewModel sẽ được inject qua DI
			// Nếu cần set DataContext thủ công cho MainWindow, có thể làm ở đây
			// Nhưng tốt nhất để App.xaml.cs xử lý
		}
	}
}