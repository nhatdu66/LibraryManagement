using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;

namespace LibraryManagementSystem.Services.DTOs
{
	public class UpdateBorrowDetailDto
	{
		public int BorrowDetailId { get; set; }
		public DateTime DueDate { get; set; }
		public string? ItemStatus { get; set; }   // cho phép đổi trạng thái từng cuốn
	}
}
