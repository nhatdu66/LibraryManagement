using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;

namespace LibraryManagementSystem.Services.DTOs
{
	public class LoginResponseDto
	{
		/// <summary>
		/// ID của tài khoản (EmployeeId hoặc ReaderId)
		/// </summary>
		public int UserId { get; set; }

		/// <summary>
		/// Loại tài khoản (Employee hoặc Reader)
		/// </summary>
		public string AccountType { get; set; } = string.Empty;

		/// <summary>
		/// Tên đầy đủ (FullName)
		/// </summary>
		public string FullName { get; set; } = string.Empty;

		/// <summary>
		/// Tên role (Administrator, Staff, Librarian, Reader)
		/// </summary>
		public string RoleName { get; set; } = string.Empty;

		/// <summary>
		/// Ngày hết hạn (nếu là Reader)
		/// </summary>
		public DateTime? ExpiredDate { get; set; }

		/// <summary>
		/// Token JWT (nếu dùng authentication token)
		/// </summary>
		public string? Token { get; set; }

		/// <summary>
		/// Thời gian hết hạn token (nếu dùng)
		/// </summary>
		public DateTime? TokenExpiration { get; set; }

		/// <summary>
		/// Thông báo thành công
		/// </summary>
		public string Message { get; set; } = "Login thành công!";
	}
}