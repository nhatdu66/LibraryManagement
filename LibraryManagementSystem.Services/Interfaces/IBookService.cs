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
	public interface IBookService
	{
		Task<BookWorkDto> GetBookWorkByIdAsync(int workId);
		Task<IEnumerable<BookWorkDto>> GetAllBookWorksAsync();
		Task<BookWorkDto> CreateBookWorkAsync(CreateBookWorkDto dto);
		Task UpdateBookWorkAsync(UpdateBookWorkDto dto, int workId);
		Task DeleteBookWorkAsync(int workId);


		Task<IEnumerable<BookWorkDto>> SearchBooksAsync(string keyword, int? authorId, int? categoryId, int? seriesId);
	}
}
