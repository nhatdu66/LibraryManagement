using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Data.Entities
{
	public class Employee
	{
		[Key]
		public int EmployeeId { get; set; }

		[Required]
		[StringLength(100)]
		[EmailAddress]
		public string Email { get; set; } = null!;

		[Required]
		[StringLength(255)]
		public string PasswordHash { get; set; } = null!;

		[Required]
		[StringLength(100)]
		public string FullName { get; set; } = null!;

		public int RoleId { get; set; }

		[ForeignKey(nameof(RoleId))]
		public virtual Role Role { get; set; } = null!;

		[Required]
		public DateTime HireDate { get; set; }

		[Required]
		[StringLength(20)]
		public string Status { get; set; } = null!;  // Active, Inactive, ...

		// Navigation properties
		public virtual ICollection<BorrowTransaction> BorrowTransactions { get; set; } = new List<BorrowTransaction>();
		public virtual ICollection<BorrowRequest> ApprovedBorrowRequests { get; set; } = new List<BorrowRequest>();
	}
}
