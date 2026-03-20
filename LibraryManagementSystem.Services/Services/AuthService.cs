using System;
using System.Threading.Tasks;
using LibraryManagementSystem.Repositories.Interfaces;
using LibraryManagementSystem.Services.DTOs;
using LibraryManagementSystem.Services.Interfaces;
using LibraryManagementSystem.Data.Entities;
using System.Diagnostics;

namespace LibraryManagementSystem.Services
{
	public class AuthService : IAuthService
	{
		private readonly IUnitOfWork _uow;
		private LoginResponseDto? _currentUser;
		public AuthService(IUnitOfWork uow)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			//_uow = uow;
		}
		public bool IsAuthenticated => _currentUser != null;
		public int? CurrentUserId => _currentUser?.UserId;
		public string? CurrentFullName => _currentUser?.FullName;
		public string? CurrentRoleName => _currentUser?.RoleName;
		public string? CurrentAccountType => _currentUser?.AccountType;

		public void SetCurrentUser(LoginResponseDto response)
		{
			if (response == null || !response.Success)
				throw new ArgumentException("Response không hợp lệ");

			_currentUser = response;
		}

		public void Logout()
		{
			_currentUser = null;
		}

		public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
		{
			Debug.WriteLine($"[LOGIN ATTEMPT] Email: {dto.Email}, Type: {dto.AccountType ?? "Auto detect"}, Pass length: {dto.Password?.Length ?? 0}");

			// 1. Kiểm tra Employee trước
			var employee = await _uow.EmployeeRepository.GetByEmailAsync(dto.Email);
			if (employee != null)
			{
				Debug.WriteLine($"[EMPLOYEE FOUND] ID: {employee.EmployeeId}, Email: {dto.Email}, PasswordHash in DB: '{employee.PasswordHash}' (length: {employee.PasswordHash?.Length ?? 0})");

				if (employee.PasswordHash == dto.Password)
				{
					Debug.WriteLine($"[EMPLOYEE LOGIN SUCCESS] Email: {dto.Email}");

					var role = await _uow.RoleRepository.GetByIdAsync(employee.RoleId);

					return new LoginResponseDto
					{
						Success = true,
						Message = "Login Employee thành công",
						UserId = employee.EmployeeId,
						AccountType = "Employee",
						FullName = employee.FullName,
						RoleName = role?.RoleName ?? "Unknown"
					};
				}
				else
				{
					Debug.WriteLine($"[EMPLOYEE PASSWORD MISMATCH] Email: {dto.Email} | Input: '{dto.Password}' | DB: '{employee.PasswordHash}'");
					return new LoginResponseDto { Success = false, Message = "Mật khẩu không đúng (Nhân viên)" };
				}
			}

			// 2. Kiểm tra Reader
			var reader = await _uow.ReaderRepository.GetByEmailAsync(dto.Email);
			if (reader != null)
			{
				Debug.WriteLine($"[READER FOUND] ID: {reader.ReaderId}, Email: {dto.Email}, PasswordHash in DB: '{reader.PasswordHash}' (length: {reader.PasswordHash?.Length ?? 0})");

				if (reader.PasswordHash == dto.Password)
				{
					Debug.WriteLine($"[READER LOGIN SUCCESS] Email: {dto.Email}");

					return new LoginResponseDto
					{
						Success = true,
						Message = "Login Reader thành công",
						UserId = reader.ReaderId,
						AccountType = "Reader",
						FullName = reader.FullName,
						RoleName = "Reader",
						ExpiredDate = reader.ExpiredDate
					};
				}
				else
				{
					Debug.WriteLine($"[READER PASSWORD MISMATCH] Email: {dto.Email} | Input: '{dto.Password}' | DB: '{reader.PasswordHash}'");
					return new LoginResponseDto { Success = false, Message = "Mật khẩu không đúng (Độc giả)" };
				}
			}

			Debug.WriteLine($"[LOGIN FAIL - User not found] Email: {dto.Email}");
			return new LoginResponseDto { Success = false, Message = "Không tìm thấy tài khoản" };
		}

		public async Task<RegisterResponseDto> RegisterReaderAsync(RegisterDto dto)
		{
			var existingReader = await _uow.ReaderRepository.GetByEmailAsync(dto.Email);
			if (existingReader != null)
			{
				throw new InvalidOperationException("Email đã được đăng ký.");
			}

			var reader = new Reader
			{
				Email = dto.Email,
				PasswordHash = dto.Password,           // plaintext → lưu thẳng vào cột PasswordHash
				FullName = dto.FullName,
				PhoneNumber = dto.PhoneNumber,
				Address = dto.Address,
				RegisterDate = DateTime.Now,
				ExpiredDate = dto.ExpiredDate ?? DateTime.Now.AddYears(2),
				ReaderStatus = "Active",
				CardNumber = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper()
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
				Message = "Đăng ký tài khoản Reader thành công!"
			};
		}

		public async Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto)
		{
			var existing = await _uow.EmployeeRepository.GetByEmailAsync(dto.Email);
			if (existing != null)
			{
				throw new InvalidOperationException("Email đã tồn tại.");
			}

			var role = await _uow.RoleRepository.GetByIdAsync(dto.RoleId);
			if (role == null)
			{
				throw new KeyNotFoundException("Không tìm thấy vai trò.");
			}

			var employee = new Employee
			{
				Email = dto.Email,
				PasswordHash = dto.Password,           // plaintext → lưu thẳng vào cột PasswordHash
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
				RoleName = role.RoleName ?? "Unknown",
				HireDate = employee.HireDate,
				Status = employee.Status
			};
		}
	}
}