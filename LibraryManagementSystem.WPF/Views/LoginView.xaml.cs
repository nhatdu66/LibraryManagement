using System.Windows;
using System.Windows.Controls;
using LibraryManagementSystem.WPF.ViewModels;

namespace LibraryManagementSystem.WPF.Views
{
	public partial class LoginView : UserControl
	{
		public LoginView()
		{
			InitializeComponent();
		}

		private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
		{
			if (DataContext is LoginViewModel vm && sender is PasswordBox pb)
			{
				vm.Password = pb.Password;
			}
		}

		private void rbAccountType_Checked(object sender, RoutedEventArgs e)
		{
			if (DataContext is LoginViewModel vm && sender is RadioButton rb)
			{
				vm.IsReaderLogin = (rb == rbReader);
			}
		}
	}
}