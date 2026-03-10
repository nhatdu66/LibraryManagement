using LibraryManagementSystem.WPF.ViewModels;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LibraryManagementSystem.WPF.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }

        // Hiện form đăng ký
        private void ShowRegisterForm(object sender, RoutedEventArgs e)
        {
            RegisterPanel.Visibility = Visibility.Visible;
        }

        // Kiểm tra dữ liệu trước khi đăng ký
        private bool ValidateRegisterForm()
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text) ||
                string.IsNullOrWhiteSpace(txtPhone.Text) ||
                string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin đăng ký!");
                return false;
            }

            if (!Regex.IsMatch(txtPhone.Text, @"^[0-9]{9,11}$"))
            {
                MessageBox.Show("Số điện thoại không hợp lệ!");
                return false;
            }

            return true;
        }

        // Chặn đăng ký nếu form sai
        private void CheckRegisterBeforeSubmit(object sender, MouseButtonEventArgs e)
        {
            if (!ValidateRegisterForm())
            {
                e.Handled = true;
            }
        }
    }
}