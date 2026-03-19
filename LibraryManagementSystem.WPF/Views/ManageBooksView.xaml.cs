using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LibraryManagementSystem.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagementSystem.WPF.Views
{
	public partial class ManageBooksView : UserControl
	{
		public ManageBooksView()
		{
			InitializeComponent();

			if (App.ServiceProvider != null)
			{
				DataContext = App.ServiceProvider.GetRequiredService<BookCatalogViewModel>();
				// Hoặc nếu bạn muốn ViewModel riêng sau này thì tạo mới ManageBooksViewModel
			}
		}
	}
}
