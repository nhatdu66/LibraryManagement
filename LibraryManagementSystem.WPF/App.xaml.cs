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
		public static IServiceProvider ServiceProvider { get; private set; }  // Chỉ khai báo 1 lần, public static

		public IConfiguration Configuration { get; private set; }

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

			Configuration = builder.Build();

			var services = new ServiceCollection();

			// DbContext
			services.AddDbContext<LibraryDbContext>(options =>
				options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

			// UnitOfWork & Repositories (scoped vì cần context riêng mỗi scope)
			services.AddScoped<IUnitOfWork, UnitOfWork>();
			services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
			services.AddScoped<IBookWorkRepository, BookWorkRepository>();
			services.AddScoped<IBookCopyRepository, BookCopyRepository>();
			services.AddScoped<IBorrowTransactionRepository, BorrowTransactionRepository>();
			services.AddScoped<IBorrowRequestRepository, BorrowRequestRepository>();
			services.AddScoped<IReaderRepository, ReaderRepository>();
			services.AddScoped<IEmployeeRepository, EmployeeRepository>();
			services.AddScoped<IRoleRepository, RoleRepository>();

			// Services
			services.AddScoped<IAuthService, AuthService>();
			services.AddScoped<IBookService, BookService>();
			services.AddScoped<IBorrowService, BorrowService>();
			services.AddScoped<IReaderAccountService, ReaderAccountService>();
			services.AddScoped<IReaderService, ReaderService>();
			services.AddScoped<IEmployeeAccountService, EmployeeAccountService>();
			services.AddScoped<IEmployeeService, EmployeeService>();
			services.AddScoped<IRoleService, RoleService>();

			// ViewModels (transient vì mỗi instance riêng)
			services.AddTransient<MainViewModel>();
			services.AddTransient<LoginViewModel>();
			services.AddTransient<BookCatalogViewModel>();
			services.AddTransient<BorrowViewModel>();
			services.AddTransient<MyAccountViewModel>();

			// MainWindow vẫn transient
			services.AddTransient<MainWindow>();

			ServiceProvider = services.BuildServiceProvider();

			try
			{
				var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
				mainWindow.DataContext = ServiceProvider.GetRequiredService<MainViewModel>();
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