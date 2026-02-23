using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LibraryManagementSystem.Data;
using LibraryManagementSystem.Data.Entities;
using LibraryManagementSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Repositories
{
	public class BorrowTransactionRepository : GenericRepository<BorrowTransaction>, IBorrowTransactionRepository
	{
		public BorrowTransactionRepository(LibraryDbContext context) : base(context)
		{
		}

		/// <summary>
		/// Lấy tất cả BorrowTransaction của một Reader (lịch sử mượn)
		/// </summary>
		public async Task<IEnumerable<BorrowTransaction>> GetByReaderIdAsync(int readerId)
		{
			return await _dbSet
				.Where(t => t.ReaderId == readerId)
				.Include(t => t.Details).ThenInclude(d => d.BookCopy).ThenInclude(c => c.BookEdition).ThenInclude(e => e.BookWork)
				.OrderByDescending(t => t.BorrowDate)
				.ToListAsync();
		}

		/// <summary>
		/// Lấy tất cả BorrowTransaction đang mượn (chưa trả hết)
		/// </summary>
		public async Task<IEnumerable<BorrowTransaction>> GetActiveTransactionsAsync()
		{
			return await _dbSet
				.Where(t => t.Status == "Borrowed" || t.Status == "PartiallyReturned")
				.Include(t => t.Details).ThenInclude(d => d.BookCopy)
				.ToListAsync();
		}

		/// <summary>
		/// Lấy tất cả BorrowTransaction do một nhân viên xử lý
		/// </summary>
		public async Task<IEnumerable<BorrowTransaction>> GetByEmployeeIdAsync(int employeeId)
		{
			return await _dbSet
				.Where(t => t.EmployeeId == employeeId)
				.Include(t => t.Details)
				.ToListAsync();
		}

		/// <summary>
		/// Lấy chi tiết BorrowTransaction kèm Detail (eager loading)
		/// </summary>
		public async Task<BorrowTransaction?> GetTransactionWithDetailsAsync(int borrowId)
		{
			return await _dbSet
				.Include(t => t.Reader)
				.Include(t => t.Employee)
				.Include(t => t.Details).ThenInclude(d => d.BookCopy).ThenInclude(c => c.BookEdition).ThenInclude(e => e.BookWork)
				.FirstOrDefaultAsync(t => t.BorrowId == borrowId);
		}

		/// <summary>
		/// Lấy tất cả BorrowTransaction liên kết với một Request cụ thể
		/// </summary>
		public async Task<IEnumerable<BorrowTransaction>> GetFromRequestAsync(int requestId)
		{
			return await _dbSet
				.Where(t => t.RequestId == requestId)
				.Include(t => t.Details)
				.ToListAsync();
		}

		/// <summary>
		/// Lấy tất cả BorrowTransaction quá hạn (DueDate < Today và chưa trả)
		/// </summary>
		public async Task<IEnumerable<BorrowTransaction>> GetOverdueTransactionsAsync()
		{
			return await _dbSet
				.Where(t => t.Status == "Borrowed" && t.Details.Any(d => d.DueDate < DateTime.Today && d.ReturnDate == null))
				.Include(t => t.Details)
				.ToListAsync();
		}
	}
}
