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
		public IServiceProvider ServiceProvider { get; private set; }
		public IConfiguration Configuration { get; private set; }

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			// Đọc appsettings.json
			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

			Configuration = builder.Build();

			// Setup DI
			var services = new ServiceCollection();

			// Register DbContext - DÙNG TRANSIENT để tránh lỗi concurrent operation
			// Register DbContext - Transient để tránh concurrent error
			services.AddDbContext<LibraryDbContext>(
	options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")),
	ServiceLifetime.Transient);

			// Register UnitOfWork
			services.AddScoped<LibraryManagementSystem.Repositories.Interfaces.IUnitOfWork,
							   LibraryManagementSystem.Repositories.UnitOfWork>();

			// Register Services
			services.AddScoped<IAuthService, AuthService>();
			services.AddScoped<IReaderAccountService, ReaderAccountService>();
			services.AddScoped<IEmployeeAccountService, EmployeeAccountService>();
			services.AddScoped<IBookService, BookService>();
			services.AddScoped<IBorrowService, BorrowService>();
			services.AddScoped<IReaderService, ReaderService>();
			services.AddScoped<IEmployeeService, EmployeeService>();
			services.AddScoped<IRoleService, RoleService>();

			// Register MainWindow và ViewModels
			services.AddTransient<MainWindow>();
			services.AddTransient<MainViewModel>();
			services.AddTransient<LoginViewModel>();

			ServiceProvider = services.BuildServiceProvider();

			try
			{
				// Resolve và show MainWindow
				var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
				mainWindow.Show();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Lỗi khởi tạo MainWindow: {ex.Message}\nInner: {ex.InnerException?.Message}");
			}
		}
	}
}