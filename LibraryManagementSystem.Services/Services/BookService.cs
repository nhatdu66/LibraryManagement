using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryManagementSystem.Data.Entities;
using LibraryManagementSystem.Repositories.Interfaces;
using LibraryManagementSystem.Services.DTOs;
using LibraryManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Services
{
	public class BookService : IBookService
	{
		private readonly IUnitOfWork _uow;

		public BookService(IUnitOfWork uow)
		{
			_uow = uow;
		}

		public async Task<BookWorkDto> GetBookWorkByIdAsync(int workId)
		{
			var book = await _uow.DbContext.BookWorks
				.Include(b => b.WorkAuthors).ThenInclude(wa => wa.Author)
				.Include(b => b.WorkCategories).ThenInclude(wc => wc.Category)
				.Include(b => b.BookEditions).ThenInclude(e => e.BookCopies)
				.FirstOrDefaultAsync(b => b.WorkId == workId);

			if (book == null)
			{
				return null;
			}

			return new BookWorkDto
			{
				WorkId = book.WorkId,
				Title = book.Title,
				OriginalTitle = book.OriginalTitle,
				Summary = book.Summary,
				SeriesId = book.SeriesId,
				VolumeNumber = book.VolumeNumber,
				Authors = book.WorkAuthors?.Select(wa => wa.Author.AuthorName).ToList() ?? new List<string>(),
				Categories = book.WorkCategories?.Select(wc => wc.Category.CategoryName).ToList() ?? new List<string>(),
				AvailableCopies = book.BookEditions?.Sum(e => e.BookCopies?.Count(c => c.CirculationStatus == "Available") ?? 0) ?? 0,
				TotalCopies = book.BookEditions?.Sum(e => e.BookCopies?.Count ?? 0) ?? 0
			};
		}

		public async Task<IEnumerable<BookWorkDto>> GetAllBookWorksAsync()
		{
			var query = _uow.DbContext.BookWorks
				.Include(b => b.WorkAuthors).ThenInclude(wa => wa.Author)
				.Include(b => b.WorkCategories).ThenInclude(wc => wc.Category)
				.Include(b => b.BookEditions).ThenInclude(e => e.BookCopies);

			var books = await query.ToListAsync();

			return books.Select(book => new BookWorkDto
			{
				WorkId = book.WorkId,
				Title = book.Title,
				OriginalTitle = book.OriginalTitle,
				Summary = book.Summary,
				SeriesId = book.SeriesId,
				VolumeNumber = book.VolumeNumber,
				Authors = book.WorkAuthors?.Select(wa => wa.Author.AuthorName).ToList() ?? new List<string>(),
				Categories = book.WorkCategories?.Select(wc => wc.Category.CategoryName).ToList() ?? new List<string>(),
				AvailableCopies = book.BookEditions?.Sum(e => e.BookCopies?.Count(c => c.CirculationStatus == "Available") ?? 0) ?? 0,
				TotalCopies = book.BookEditions?.Sum(e => e.BookCopies?.Count ?? 0) ?? 0
			});
		}

		public async Task<IEnumerable<BookWorkDto>> SearchBooksAsync(string keyword, int? authorId, int? categoryId, int? seriesId)
		{
			var query = _uow.DbContext.BookWorks
				.Include(b => b.WorkAuthors).ThenInclude(wa => wa.Author)
				.Include(b => b.WorkCategories).ThenInclude(wc => wc.Category)
				.Include(b => b.BookEditions).ThenInclude(e => e.BookCopies)
				.AsQueryable();

			if (!string.IsNullOrWhiteSpace(keyword))
			{
				keyword = keyword.Trim().ToLower();
				query = query.Where(b =>
					b.Title.ToLower().Contains(keyword) ||
					(b.OriginalTitle != null && b.OriginalTitle.ToLower().Contains(keyword)) ||
					(b.Summary != null && b.Summary.ToLower().Contains(keyword))
				);
			}

			if (authorId.HasValue)
			{
				query = query.Where(b => b.WorkAuthors.Any(wa => wa.AuthorId == authorId.Value));
			}

			if (categoryId.HasValue)
			{
				query = query.Where(b => b.WorkCategories.Any(wc => wc.CategoryId == categoryId.Value));
			}

			if (seriesId.HasValue)
			{
				query = query.Where(b => b.SeriesId == seriesId.Value);
			}

			var books = await query.ToListAsync();

			return books.Select(book => new BookWorkDto
			{
				WorkId = book.WorkId,
				Title = book.Title,
				OriginalTitle = book.OriginalTitle,
				Summary = book.Summary,
				SeriesId = book.SeriesId,
				VolumeNumber = book.VolumeNumber,
				Authors = book.WorkAuthors?.Select(wa => wa.Author.AuthorName).ToList() ?? new List<string>(),
				Categories = book.WorkCategories?.Select(wc => wc.Category.CategoryName).ToList() ?? new List<string>(),
				AvailableCopies = book.BookEditions?.Sum(e => e.BookCopies?.Count(c => c.CirculationStatus == "Available") ?? 0) ?? 0,
				TotalCopies = book.BookEditions?.Sum(e => e.BookCopies?.Count ?? 0) ?? 0
			});
		}

		public async Task<BookWorkDto> CreateBookWorkAsync(CreateBookWorkDto dto)
		{
			var bookWork = new BookWork
			{
				Title = dto.Title,
				OriginalTitle = dto.OriginalTitle,
				Summary = dto.Summary,
				SeriesId = dto.SeriesId.GetValueOrDefault(),
				VolumeNumber = dto.VolumeNumber
			};

			await _uow.DbContext.BookWorks.AddAsync(bookWork);
			await _uow.SaveChangesAsync();

			if (dto.AuthorIds != null && dto.AuthorIds.Any())
			{
				foreach (var authorId in dto.AuthorIds)
				{
					await _uow.DbContext.WorkAuthors.AddAsync(new WorkAuthor
					{
						WorkId = bookWork.WorkId,
						AuthorId = authorId
					});
				}
			}

			if (dto.CategoryIds != null && dto.CategoryIds.Any())
			{
				foreach (var categoryId in dto.CategoryIds)
				{
					await _uow.DbContext.WorkCategories.AddAsync(new WorkCategory
					{
						WorkId = bookWork.WorkId,
						CategoryId = categoryId
					});
				}
			}

			await _uow.SaveChangesAsync();

			return new BookWorkDto
			{
				WorkId = bookWork.WorkId,
				Title = bookWork.Title,
				OriginalTitle = bookWork.OriginalTitle,
				Summary = bookWork.Summary,
				SeriesId = bookWork.SeriesId,
				VolumeNumber = bookWork.VolumeNumber,
				Authors = new List<string>(),
				Categories = new List<string>(),
				AvailableCopies = 0,
				TotalCopies = 0
			};
		}

		public async Task UpdateBookWorkAsync(UpdateBookWorkDto dto, int workId)
		{
			var bookWork = await _uow.DbContext.BookWorks
				.Include(b => b.WorkAuthors)
				.Include(b => b.WorkCategories)
				.FirstOrDefaultAsync(b => b.WorkId == workId);

			if (bookWork == null)
			{
				throw new KeyNotFoundException("Không tìm thấy tác phẩm");
			}

			if (dto.Title != null) bookWork.Title = dto.Title;
			if (dto.OriginalTitle != null) bookWork.OriginalTitle = dto.OriginalTitle;
			if (dto.Summary != null) bookWork.Summary = dto.Summary;

			// Fix lỗi CS0266: dùng .Value để chuyển int? thành int
			if (dto.SeriesId.HasValue) bookWork.SeriesId = dto.SeriesId.Value;

			if (dto.VolumeNumber.HasValue) bookWork.VolumeNumber = dto.VolumeNumber.Value;

			if (dto.AuthorIds != null)
			{
				_uow.DbContext.WorkAuthors.RemoveRange(bookWork.WorkAuthors);
				foreach (var authorId in dto.AuthorIds)
				{
					bookWork.WorkAuthors.Add(new WorkAuthor { WorkId = workId, AuthorId = authorId });
				}
			}

			if (dto.CategoryIds != null)
			{
				_uow.DbContext.WorkCategories.RemoveRange(bookWork.WorkCategories);
				foreach (var categoryId in dto.CategoryIds)
				{
					bookWork.WorkCategories.Add(new WorkCategory { WorkId = workId, CategoryId = categoryId });
				}
			}

			_uow.DbContext.BookWorks.Update(bookWork);
			await _uow.SaveChangesAsync();
		}

		public async Task DeleteBookWorkAsync(int workId)
		{
			var bookWork = await _uow.DbContext.BookWorks.FindAsync(workId);
			if (bookWork == null)
			{
				throw new KeyNotFoundException("Không tìm thấy tác phẩm");
			}

			_uow.DbContext.BookWorks.Remove(bookWork);
			await _uow.SaveChangesAsync();
		}
	}
}