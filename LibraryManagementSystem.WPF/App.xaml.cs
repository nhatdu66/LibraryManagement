using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows;
using LibraryManagementSystem.WPF;
using Microsoft.EntityFrameworkCore;  // Để dùng DbContextOptionsBuilder
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration;
using LibraryManagementSystem.Repositories.Interfaces; // ← chỉ dùng cái này

// App.xaml.cs (giữ nguyên phần OnStartup bạn đã có, chỉ bổ sung nếu cần)
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Windows;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Windows;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Repositories;
using LibraryManagementSystem.Repositories.Interfaces;
using LibraryManagementSystem.Services;
using LibraryManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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

			// Register DbContext
			services.AddDbContext<LibraryDbContext>(options =>
				options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

			// Register UnitOfWork (use non-generic overload to avoid compile-time type constraint)
			services.AddScoped(
				typeof(LibraryManagementSystem.Repositories.Interfaces.IUnitOfWork),
				typeof(LibraryManagementSystem.Repositories.UnitOfWork)
			);

			// Register tất cả Services
			services.AddScoped<IAuthService, AuthService>();
			services.AddScoped<IReaderAccountService, ReaderAccountService>();
			services.AddScoped<IEmployeeAccountService, EmployeeAccountService>();
			services.AddScoped<IBookService, BookService>();
			services.AddScoped<IBorrowService, BorrowService>();
			services.AddScoped<IReaderService, ReaderService>();
			services.AddScoped<IEmployeeService, EmployeeService>();
			services.AddScoped<IRoleService, RoleService>();

			// Register MainWindow as transient (or scoped)
			services.AddTransient<MainWindow>();

			ServiceProvider = services.BuildServiceProvider();

			try
			{
				var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
				mainWindow.Show();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Lỗi khởi tạo MainWindow: {ex.Message}\nInner: {ex.InnerException?.Message}");
			}

			// create a scope so scoped services are resolved correctly
			/*using var scope = ServiceProvider.CreateScope();
			var mainWindow = scope.ServiceProvider.GetRequiredService<MainWindow>();
			mainWindow.Show();*/
		}
	}
}