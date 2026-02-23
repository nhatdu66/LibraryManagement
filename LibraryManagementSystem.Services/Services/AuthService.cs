using System;
using System.Threading.Tasks;
using LibraryManagementSystem.Repositories.Interfaces;
using LibraryManagementSystem.Services.DTOs;
using LibraryManagementSystem.Services.Interfaces;
using LibraryManagementSystem.Data.Entities; // ← THÊM DÒNG NÀY
using BCrypt.Net; // ← THÊM DÒNG NÀY (nếu dùng BCrypt)

namespace LibraryManagementSystem.Services
{
	public class AuthService : IAuthService
	{
		private readonly IUnitOfWork _uow;

		public AuthService(IUnitOfWork uow)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
		}

		public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
		{
			if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
				throw new ArgumentException("Email và Password không được để trống");

			// Tìm Employee trước
			var employee = await _uow.EmployeeRepository.GetByEmailAsync(dto.Email);
			if (employee != null)
			{
				// Kiểm tra password (BCrypt hoặc plaintext tạm)
				if (BCrypt.Net.BCrypt.Verify(dto.Password, employee.PasswordHash)) // Nếu chưa có BCrypt → thay bằng dto.Password == employee.PasswordHash
				{
					return new LoginResponseDto
					{
						UserId = employee.EmployeeId,
						AccountType = "Employee",
						FullName = employee.FullName,
						RoleName = employee.Role.RoleName,
						Message = "Login Employee thành công"
					};
				}
			}

			// Tìm Reader nếu không phải Employee
			var reader = await _uow.ReaderRepository.GetByEmailAsync(dto.Email);
			if (reader != null)
			{
				if (BCrypt.Net.BCrypt.Verify(dto.Password, reader.PasswordHash)) // Nếu chưa có BCrypt → thay bằng dto.Password == reader.PasswordHash
				{
					return new LoginResponseDto
					{
						UserId = reader.ReaderId,
						AccountType = "Reader",
						FullName = reader.FullName,
						RoleName = "Reader",
						ExpiredDate = reader.ExpiredDate,
						Message = "Login Reader thành công"
					};
				}
			}

			throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng");
		}

		public async Task<RegisterResponseDto> RegisterReaderAsync(RegisterDto dto)
		{
			if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password) || string.IsNullOrWhiteSpace(dto.FullName))
				throw new ArgumentException("Email, Password và FullName không được để trống");

			if (await _uow.ReaderRepository.GetByEmailAsync(dto.Email) != null)
				throw new ArgumentException("Email đã tồn tại");

			var reader = new Reader
			{
				Email = dto.Email,
				PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password), // Nếu chưa có BCrypt → thay bằng dto.Password (plaintext tạm)
				FullName = dto.FullName,
				PhoneNumber = dto.PhoneNumber,
				Address = dto.Address,
				RegisterDate = DateTime.Now,
				ExpiredDate = dto.ExpiredDate ?? DateTime.Now.AddYears(2),
				ReaderStatus = "Active",
				CardNumber = "RD" + Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper()
			};

			await _uow.ReaderRepository.AddAsync(reader);
			await _uow.SaveChangesAsync();

			return new RegisterResponseDto
			{
				ReaderId = reader.ReaderId,
				CardNumber = reader.CardNumber,
				FullName = reader.FullName,
				Email = reader.Email,
				ExpiredDate = reader.ExpiredDate,
				Message = "Đăng ký Reader thành công"
			};
		}

		public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto)
		{
			if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password) || string.IsNullOrWhiteSpace(dto.FullName))
				throw new ArgumentException("Email, Password và FullName không được để trống");

			if (await _uow.EmployeeRepository.GetByEmailAsync(dto.Email) != null)
				throw new ArgumentException("Email đã tồn tại");

			var employee = new Employee
			{
				Email = dto.Email,
				PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password), // Nếu chưa có BCrypt → thay bằng dto.Password
				FullName = dto.FullName,
				RoleId = dto.RoleId,
				HireDate = dto.HireDate ?? DateTime.Now,
				Status = dto.Status ?? "Active"
			};

			await _uow.EmployeeRepository.AddAsync(employee);
			await _uow.SaveChangesAsync();

			return new EmployeeDto
			{
				EmployeeId = employee.EmployeeId,
				Email = employee.Email,
				FullName = employee.FullName,
				RoleName = (await _uow.RoleRepository.GetByIdAsync(employee.RoleId))?.RoleName ?? "Unknown",
				HireDate = employee.HireDate,
				Status = employee.Status
			};
		}
	}
}