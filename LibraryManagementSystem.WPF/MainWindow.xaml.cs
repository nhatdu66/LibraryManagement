using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using LibraryManagementSystem.WPF.ViewModels;
using LibraryManagementSystem.WPF.Views;

namespace LibraryManagementSystem.WPF
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			Loaded += (sender, e) =>
			{
				if (tabMain != null)
				{
					foreach (TabItem tab in tabMain.Items)
					{
						if (tab.Header?.ToString().Contains("Đăng nhập") == true)
						{
							if (tab.Content is LoginView loginView)
							{
								var vm = App.ServiceProvider.GetRequiredService<LoginViewModel>();
								loginView.DataContext = vm;
							}
							break;
						}
					}
				}
			};
		}
	}
}