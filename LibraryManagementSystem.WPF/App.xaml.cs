using System;
using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Repositories;
using LibraryManagementSystem.Repositories.Interfaces;
using LibraryManagementSystem.Services;
using LibraryManagementSystem.Services.Interfaces;
using LibraryManagementSystem.WPF.ViewModels;

namespace LibraryManagementSystem.WPF
{
	public partial class App : Application
	{
		public static IServiceProvider ServiceProvider { get; private set; }

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

			var configuration = builder.Build();

			var services = new ServiceCollection();

			// DbContext
			services.AddDbContext<LibraryDbContext>(options =>
				options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

			// UnitOfWork + Repositories
			services.AddScoped<IUnitOfWork, UnitOfWork>();
			services.AddScoped<IRoleRepository, RoleRepository>();
			services.AddScoped<IEmployeeRepository, EmployeeRepository>();
			services.AddScoped<IReaderRepository, ReaderRepository>();
			services.AddScoped<IBookWorkRepository, BookWorkRepository>();
			services.AddScoped<IBorrowRequestRepository, BorrowRequestRepository>();
			services.AddScoped<IBorrowTransactionRepository, BorrowTransactionRepository>();
			services.AddScoped<IBookCopyRepository, BookCopyRepository>();

			// Services
			services.AddScoped<IAuthService, AuthService>();
			services.AddScoped<IBookService, BookService>();
			services.AddScoped<IBorrowService, BorrowService>();
			services.AddScoped<IEmployeeAccountService, EmployeeAccountService>();
			services.AddScoped<IReaderAccountService, ReaderAccountService>();
			services.AddScoped<IRoleService, RoleService>();
			services.AddScoped<IEmployeeService, EmployeeService>();
			services.AddScoped<IReaderService, ReaderService>();

			// ViewModels
			services.AddTransient<LoginViewModel>();
			services.AddTransient<MainViewModel>();
			services.AddTransient<BookCatalogViewModel>();
			services.AddTransient<BorrowViewModel>();
			services.AddTransient<MyAccountViewModel>();

			// MainWindow
			services.AddTransient<MainWindow>();

			ServiceProvider = services.BuildServiceProvider();

			try
			{
				var mainVM = ServiceProvider.GetRequiredService<MainViewModel>();
				var mainWindow = new MainWindow
				{
					DataContext = mainVM  // Set DataContext là MainViewModel
				};
				mainWindow.Show();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Lỗi khởi tạo ứng dụng:\n{ex.Message}\nInner: {ex.InnerException?.Message}",
								"Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}