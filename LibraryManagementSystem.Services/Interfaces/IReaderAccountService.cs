using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryManagementSystem.Services.DTOs;

namespace LibraryManagementSystem.Services.Interfaces
{
	public interface IReaderAccountService
	{
		/// <summary>
		/// Lấy thông tin độc giả theo ID
		/// </summary>
		Task<ReaderDto> GetReaderByIdAsync(int readerId);

		/// <summary>
		/// Lấy tất cả độc giả (Admin/Staff/Librarian xem danh sách)
		/// </summary>
		Task<IEnumerable<ReaderDto>> GetAllReadersAsync();

		/// <summary>
		/// Cập nhật thông tin độc giả (Reader tự update profile hoặc Admin/Staff/Librarian update)
		/// </summary>
		Task UpdateReaderAsync(int readerId, UpdateReaderDto dto);

		/// <summary>
		/// Xóa hoặc deactivate độc giả (Admin/Staff/Librarian)
		/// </summary>
		Task DeleteReaderAsync(int readerId);

		/// <summary>
		/// Staff/Librarian tạo Reader
		/// </summary>
		Task<ReaderDto> CreateReaderAsync(CreateReaderDto dto);
	}
}
