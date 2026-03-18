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
	public class BookWorkRepository : GenericRepository<BookWork>, IBookWorkRepository
	{
		public BookWorkRepository(LibraryDbContext context) : base(context)
		{
		}

		public async Task<IEnumerable<BookWork>> FindByTitleAsync(string titleKeyword)
		{
			return await _dbSet
				.Where(b => b.Title.ToLower().Contains(titleKeyword.ToLower()))
				.Include(b => b.BookEditions)
				.Include(b => b.WorkAuthors).ThenInclude(wa => wa.Author)
				.Include(b => b.WorkCategories).ThenInclude(wc => wc.Category)
				.ToListAsync();
		}

		public async Task<IEnumerable<BookWork>> FindByAuthorIdAsync(int authorId)
		{
			return await _dbSet
				.Where(b => b.WorkAuthors.Any(wa => wa.AuthorId == authorId))
				.Include(b => b.WorkAuthors).ThenInclude(wa => wa.Author)
				.ToListAsync();
		}

		public async Task<IEnumerable<BookWork>> FindByCategoryIdAsync(int categoryId)
		{
			return await _dbSet
				.Where(b => b.WorkCategories.Any(wc => wc.CategoryId == categoryId))
				.Include(b => b.WorkCategories).ThenInclude(wc => wc.Category)
				.ToListAsync();
		}

		public async Task<IEnumerable<BookWork>> FindBySeriesIdAsync(int seriesId)
		{
			return await _dbSet
				.Where(b => b.SeriesId == seriesId)
				.OrderBy(b => b.VolumeNumber)
				.ToListAsync();
		}

		public async Task<IEnumerable<BookWork>> FindAvailableForBorrowAsync(int minAvailableCopies)
		{
			return await _dbSet
				.Where(b => b.BookEditions.Any(e => e.BookCopies.Count(c => c.CirculationStatus == "Available") >= minAvailableCopies))
				.Include(b => b.BookEditions).ThenInclude(e => e.BookCopies)
				.ToListAsync();
		}

		public async Task AddWorkAuthorAsync(WorkAuthor workAuthor)
		{
			await _context.WorkAuthors.AddAsync(workAuthor);
		}

		public async Task AddWorkCategoryAsync(WorkCategory workCategory)
		{
			await _context.WorkCategories.AddAsync(workCategory);
		}

		public async Task RemoveWorkAuthorsAsync(int workId)
		{
			var workAuthors = await _context.WorkAuthors.Where(wa => wa.WorkId == workId).ToListAsync();
			_context.WorkAuthors.RemoveRange(workAuthors);
		}

		public async Task RemoveWorkCategoriesAsync(int workId)
		{
			var workCategories = await _context.WorkCategories.Where(wc => wc.WorkId == workId).ToListAsync();
			_context.WorkCategories.RemoveRange(workCategories);
		}

        public async Task<IEnumerable<BookWork>> GetWorks()
        {
			return await _context.BookWorks
				.Include(b => b.Series)
				.ToListAsync();
        }

        public async Task<IEnumerable<BookWork>> GetWorksPaging(string keywords, int page, int pageSize)
        {
            IQueryable<BookWork> query = _context.BookWorks
                .Include(b => b.Series);

            if (!string.IsNullOrEmpty(keywords))
            {
                query = query.Where(b => b.Title.Contains(keywords));
            }
            IEnumerable<BookWork> result = await query
                .OrderBy(b => b.WorkId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return result;
        }
        public async Task<int> CountTotalPage(string keywords)
        {
            IQueryable<BookWork> query = _context.BookWorks;

            if (!string.IsNullOrEmpty(keywords))
            {
                query = query.Where(b => b.Title.Contains(keywords));
            }

            int total = await query.CountAsync();

            return total;
        }
    }
}