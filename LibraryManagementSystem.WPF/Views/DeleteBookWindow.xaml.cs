using System;
using System.Windows;
using System.Windows.Controls;
using LibraryManagementSystem.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagementSystem.WPF.Views
{
    public partial class DeleteBookWindow : Window
    {
        private readonly int _workId;
        private TextBlock? tbInfo;

        public DeleteBookWindow(int workId)
        {
            _workId = workId;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Title = "Delete Book";
            Width = 500;
            Height = 220;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var grid = new Grid { Margin = new Thickness(12) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            grid.Children.Add(new TextBlock { Text = "Are you sure you want to delete this book?", FontWeight = FontWeights.Bold });

            tbInfo = new TextBlock { Margin = new Thickness(0,8,0,8), TextWrapping = TextWrapping.Wrap };
            Grid.SetRow(tbInfo, 1); grid.Children.Add(tbInfo);

            var sp = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var btnDelete = new Button { Content = "Delete", Width = 100, Margin = new Thickness(6) };
            btnDelete.Click += BtnDelete_Click;
            var btnCancel = new Button { Content = "Cancel", Width = 100, Margin = new Thickness(6) };
            btnCancel.Click += (s,e) => { this.DialogResult = false; this.Close(); };
            sp.Children.Add(btnDelete); sp.Children.Add(btnCancel);
            Grid.SetRow(sp, 2); grid.Children.Add(sp);

            Content = grid;

            Loaded += DeleteBookWindow_Loaded;
        }

        private async void DeleteBookWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            try
            {
                var svc = App.ServiceProvider.GetRequiredService<IBookService>();
                var dto = await svc.GetBookWorkByIdAsync(_workId);
                if (dto != null)
                {
                    tbInfo.Text = $"Title: {dto.Title}\nAuthors: {dto.AuthorsString}\nCategories: {dto.CategoriesString}";
                }
                else
                {
                    tbInfo.Text = "Book not found.";
                }
            }
            catch (Exception ex)
            {
                tbInfo.Text = $"Error loading book: {ex.Message}";
            }
        }

        private async void BtnDelete_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                var svc = App.ServiceProvider.GetRequiredService<IBookService>();
                await svc.DeleteBookWorkAsync(_workId);
                MessageBox.Show("Book deleted.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting book: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
