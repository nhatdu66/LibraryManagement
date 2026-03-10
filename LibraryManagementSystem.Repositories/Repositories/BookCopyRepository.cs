using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LibraryManagementSystem.Data;
using LibraryManagementSystem.Data.Entities;
using LibraryManagementSystem.Repositories.Interfaces;

namespace LibraryManagementSystem.Repositories
{
	public class BookCopyRepository : GenericRepository<BookCopy>, IBookCopyRepository
	{
		public BookCopyRepository(LibraryDbContext context) : base(context)
		{
		}
	}
}
