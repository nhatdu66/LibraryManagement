// Full code cho App.xaml.cs (đổi AddDbContext thành Transient)
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Repositories;
using LibraryManagementSystem.Repositories.Interfaces;
using LibraryManagementSystem.Services;
using LibraryManagementSystem.Services.Interfaces;
using LibraryManagementSystem.WPF.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagementSystem.WPF
{
	public partial class App : Application
	{
		public static IServiceProvider ServiceProvider { get; private set; }

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			var services = new ServiceCollection();

			// Configuration
			var configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.Build();

			// DbContext - Đổi thành Transient để mỗi operation có instance DbContext riêng, tránh concurrency
			services.AddDbContext<LibraryDbContext>(options =>
				options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")),
				ServiceLifetime.Transient);  // ← Key fix concurrency

			// UnitOfWork & Repositories
			services.AddTransient<IUnitOfWork, UnitOfWork>();
			services.AddTransient<IRoleRepository, RoleRepository>();
			services.AddTransient<IEmployeeRepository, EmployeeRepository>();
			services.AddTransient<IReaderRepository, ReaderRepository>();
			services.AddTransient<IBookWorkRepository, BookWorkRepository>();
			services.AddTransient<IBorrowRequestRepository, BorrowRequestRepository>();
			services.AddTransient<IBorrowTransactionRepository, BorrowTransactionRepository>();
			services.AddTransient<IBookCopyRepository, BookCopyRepository>();

			// Services
			//services.AddTransient<IAuthService, AuthService>();
			services.AddSingleton<IAuthService, AuthService>();
			services.AddTransient<IBookService, BookService>();
			services.AddTransient<IBorrowService, BorrowService>();
			services.AddTransient<IEmployeeAccountService, EmployeeAccountService>();
			services.AddTransient<IEmployeeService, EmployeeService>();
			services.AddTransient<IReaderAccountService, ReaderAccountService>();
			services.AddTransient<IReaderService, ReaderService>();
			services.AddTransient<IRoleService, RoleService>();

			// ViewModels
			services.AddTransient<LoginViewModel>();
			services.AddTransient<BookCatalogViewModel>();
			services.AddTransient<BorrowViewModel>();
			services.AddTransient<MyAccountViewModel>();
			services.AddTransient<ManageAccountsViewModel>();
			services.AddTransient<MainViewModel>();
			services.AddTransient<CreateBorrowTransactionViewModel>();
			services.AddTransient<ManageBooksViewModel>();

			// MainWindow
			services.AddTransient<MainWindow>();

			ServiceProvider = services.BuildServiceProvider();

			try
			{
				var mainVM = ServiceProvider.GetRequiredService<MainViewModel>();
				var mainWindow = new MainWindow
				{
					DataContext = mainVM
				};
				mainWindow.Show();
			}
			catch (Exception ex)
			{
				// Optional: Thay bằng logging nếu cần
				Debug.WriteLine($"Lỗi khởi tạo ứng dụng: {ex.Message} - Inner: {ex.InnerException?.Message}");
				// Hoặc giữ im lặng, hoặc hiển thị thông báo khác trong UI
			}
		}
	}
}
