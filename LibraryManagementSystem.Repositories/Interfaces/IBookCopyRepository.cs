using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LibraryManagementSystem.Data.Entities;

namespace LibraryManagementSystem.Repositories.Interfaces
{
	public interface IBookCopyRepository : IGenericRepository<BookCopy>
	{
		// Nếu cần thêm method đặc biệt cho BookCopy (tạm thời dùng chung Generic là đủ)
		// Ví dụ: Task<IEnumerable<BookCopy>> GetAvailableCopiesByEditionIdAsync(int editionId);
	}
}
