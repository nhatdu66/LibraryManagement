using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;

namespace LibraryManagementSystem.Services.DTOs
{
	public class BorrowTransactionDto
	{
		/// <summary>
		/// ID giao dịch mượn
		/// </summary>
		public int BorrowId { get; set; }

		/// <summary>
		/// ID độc giả
		/// </summary>
		public int ReaderId { get; set; }

		/// <summary>
		/// Tên độc giả
		/// </summary>
		public string ReaderFullName { get; set; } = string.Empty;

		/// <summary>
		/// ID nhân viên xử lý (check-out)
		/// </summary>
		public int EmployeeId { get; set; }

		/// <summary>
		/// Tên nhân viên
		/// </summary>
		public string EmployeeFullName { get; set; } = string.Empty;

		/// <summary>
		/// ID request liên kết (nếu mượn từ request)
		/// </summary>
		public int? RequestId { get; set; }

		/// <summary>
		/// Ngày mượn
		/// </summary>
		public DateTime BorrowDate { get; set; }

		/// <summary>
		/// Trạng thái (Borrowed, PartiallyReturned, FullyReturned)
		/// </summary>
		public string Status { get; set; } = string.Empty;

		/// <summary>
		/// Danh sách chi tiết giao dịch (bản sao cụ thể, ngày đến hạn, ngày trả, phạt)
		/// </summary>
		public List<BorrowTransactionDetailDto> Details { get; set; } = new List<BorrowTransactionDetailDto>();
	}

	/// <summary>
	/// Chi tiết từng dòng trong transaction
	/// </summary>
	public class BorrowTransactionDetailDto
	{
		public int BorrowDetailId { get; set; }
		public int CopyId { get; set; }
		public string Title { get; set; } = string.Empty;
		public DateTime DueDate { get; set; }
		public DateTime? ReturnDate { get; set; }
		public string ItemStatus { get; set; } = string.Empty;
		public decimal FineAmount { get; set; }
		public string? ConditionNote { get; set; }
	}
}
