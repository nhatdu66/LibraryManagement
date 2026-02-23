using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryManagementSystem.Data.Entities;
using LibraryManagementSystem.Repositories.Interfaces;
using LibraryManagementSystem.Services.DTOs;
using LibraryManagementSystem.Services.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryManagementSystem.Data.Entities;
using LibraryManagementSystem.Repositories.Interfaces;
using LibraryManagementSystem.Services.DTOs;
using LibraryManagementSystem.Services.Interfaces;

namespace LibraryManagementSystem.Services
{
	public class BookService : IBookService
	{
		private readonly IUnitOfWork _uow;

		public BookService(IUnitOfWork uow)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
		}

		public async Task<BookWorkDto> GetBookWorkByIdAsync(int workId)
		{
			var book = await _uow.BookWorkRepository.GetByIdAsync(workId);
			if (book == null) throw new KeyNotFoundException("Không tìm thấy tác phẩm");

			return new BookWorkDto
			{
				WorkId = book.WorkId,
				Title = book.Title,
				OriginalTitle = book.OriginalTitle,
				Summary = book.Summary,
				FirstPublishYear = book.FirstPublishYear ?? 0,
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
			var books = await _uow.BookWorkRepository.GetAllAsync();
			return books.Select(book => new BookWorkDto
			{
				WorkId = book.WorkId,
				Title = book.Title,
				OriginalTitle = book.OriginalTitle,
				Summary = book.Summary,
				FirstPublishYear = book.FirstPublishYear ?? 0,
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
			var book = new BookWork
			{
				Title = dto.Title,
				OriginalTitle = dto.OriginalTitle,
				Summary = dto.Summary,
				FirstPublishYear = dto.FirstPublishYear,
				SeriesId = dto.SeriesId ?? 0,
				VolumeNumber = dto.VolumeNumber
			};

			await _uow.BookWorkRepository.AddAsync(book);
			await _uow.SaveChangesAsync();

			// Thêm junction qua repository
			foreach (var authorId in dto.AuthorIds)
			{
				var workAuthor = new WorkAuthor { WorkId = book.WorkId, AuthorId = authorId };
				await _uow.BookWorkRepository.AddWorkAuthorAsync(workAuthor);
			}

			foreach (var categoryId in dto.CategoryIds)
			{
				var workCategory = new WorkCategory { WorkId = book.WorkId, CategoryId = categoryId };
				await _uow.BookWorkRepository.AddWorkCategoryAsync(workCategory);
			}

			await _uow.SaveChangesAsync();

			return await GetBookWorkByIdAsync(book.WorkId);
		}

		public async Task UpdateBookWorkAsync(UpdateBookWorkDto dto, int workId)
		{
			var book = await _uow.BookWorkRepository.GetByIdAsync(workId);
			if (book == null) throw new KeyNotFoundException("Không tìm thấy tác phẩm");

			if (dto.Title != null) book.Title = dto.Title;
			if (dto.OriginalTitle != null) book.OriginalTitle = dto.OriginalTitle;
			if (dto.Summary != null) book.Summary = dto.Summary;
			if (dto.FirstPublishYear != null) book.FirstPublishYear = dto.FirstPublishYear;
			if (dto.SeriesId != null) book.SeriesId = dto.SeriesId.Value;
			if (dto.VolumeNumber != null) book.VolumeNumber = dto.VolumeNumber.Value;

			// Nếu AuthorIds gửi, xóa cũ rồi add mới
			if (dto.AuthorIds != null)
			{
				await _uow.BookWorkRepository.RemoveWorkAuthorsAsync(workId);
				foreach (var authorId in dto.AuthorIds)
				{
					var workAuthor = new WorkAuthor { WorkId = workId, AuthorId = authorId };
					await _uow.BookWorkRepository.AddWorkAuthorAsync(workAuthor);
				}
			}

			// Nếu CategoryIds gửi, xóa cũ rồi add mới
			if (dto.CategoryIds != null)
			{
				await _uow.BookWorkRepository.RemoveWorkCategoriesAsync(workId);
				foreach (var categoryId in dto.CategoryIds)
				{
					var workCategory = new WorkCategory { WorkId = workId, CategoryId = categoryId };
					await _uow.BookWorkRepository.AddWorkCategoryAsync(workCategory);
				}
			}

			await _uow.BookWorkRepository.UpdateAsync(book);
			await _uow.SaveChangesAsync();
		}

		public async Task DeleteBookWorkAsync(int workId)
		{
			var book = await _uow.BookWorkRepository.GetByIdAsync(workId);
			if (book == null) throw new KeyNotFoundException("Không tìm thấy tác phẩm");

			await _uow.BookWorkRepository.DeleteAsync(book);
			await _uow.SaveChangesAsync();
		}

		public async Task<IEnumerable<BookWorkDto>> SearchBooksAsync(string keyword, int? authorId, int? categoryId, int? seriesId)
		{
			var books = await _uow.BookWorkRepository.GetAllAsync();

			if (!string.IsNullOrWhiteSpace(keyword))
				books = books.Where(b => b.Title.Contains(keyword)).ToList();

			if (authorId.HasValue)
				books = books.Where(b => b.WorkAuthors.Any(wa => wa.AuthorId == authorId.Value)).ToList();

			if (categoryId.HasValue)
				books = books.Where(b => b.WorkCategories.Any(wc => wc.CategoryId == categoryId.Value)).ToList();

			if (seriesId.HasValue)
				books = books.Where(b => b.SeriesId == seriesId.Value).ToList();

			return books.Select(book => new BookWorkDto
			{
				WorkId = book.WorkId,
				Title = book.Title,
				OriginalTitle = book.OriginalTitle,
				Summary = book.Summary,
				FirstPublishYear = book.FirstPublishYear ?? 0,
				SeriesId = book.SeriesId,
				VolumeNumber = book.VolumeNumber,
				Authors = book.WorkAuthors?.Select(wa => wa.Author.AuthorName).ToList() ?? new List<string>(),
				Categories = book.WorkCategories?.Select(wc => wc.Category.CategoryName).ToList() ?? new List<string>(),
				AvailableCopies = book.BookEditions?.Sum(e => e.BookCopies?.Count(c => c.CirculationStatus == "Available") ?? 0) ?? 0,
				TotalCopies = book.BookEditions?.Sum(e => e.BookCopies?.Count ?? 0) ?? 0
			});
		}
	}
}