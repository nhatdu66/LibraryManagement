using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data.Entities;

using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data.Entities;

namespace LibraryManagementSystem.Data
{
	public class LibraryDbContext : DbContext
	{
		// Constructor mặc định (dùng cho migration và design-time)
		public LibraryDbContext() : base()
		{
		}

		public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
		{
		}

		// DbSets
		public DbSet<Role> Roles { get; set; }
		public DbSet<Employee> Employees { get; set; }
		public DbSet<Reader> Readers { get; set; }
		public DbSet<BookWork> BookWorks { get; set; }
		public DbSet<BookEdition> BookEditions { get; set; }
		public DbSet<BookCopy> BookCopies { get; set; }
		public DbSet<Author> Authors { get; set; }
		public DbSet<Category> Categories { get; set; }
		public DbSet<Publisher> Publishers { get; set; }
		public DbSet<Series> Series { get; set; }
		public DbSet<WorkAuthor> WorkAuthors { get; set; }
		public DbSet<WorkCategory> WorkCategories { get; set; }
		public DbSet<BorrowRequest> BorrowRequests { get; set; }
		public DbSet<BorrowRequestDetail> BorrowRequestDetails { get; set; }
		public DbSet<BorrowTransaction> BorrowTransactions { get; set; }
		public DbSet<BorrowTransactionDetail> BorrowTransactionDetails { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{
				// Connection string fallback for design-time (e.g., migration)
				optionsBuilder.UseSqlServer("DESKTOP-4MP3LIQ\\SQLEXPRESS03;Database=LibraryManagementDB;Trusted_Connection=True;MultipleActiveResultSets=true");
			}
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			// Config tên bảng singular cho các entity chính (để khớp DDL của bạn)
			modelBuilder.Entity<Author>().ToTable("Author");
			modelBuilder.Entity<Category>().ToTable("Category");
			modelBuilder.Entity<BookWork>().ToTable("BookWork");
			modelBuilder.Entity<BookEdition>().ToTable("BookEdition");
			modelBuilder.Entity<BookCopy>().ToTable("BookCopy");
			modelBuilder.Entity<Role>().ToTable("Role");
			modelBuilder.Entity<Employee>().ToTable("Employee");
			modelBuilder.Entity<Reader>().ToTable("Reader");
			modelBuilder.Entity<Series>().ToTable("Series");
			modelBuilder.Entity<Publisher>().ToTable("Publisher");

			// Junction tables (đã có từ trước, giữ nguyên)
			modelBuilder.Entity<WorkAuthor>()
				.ToTable("WorkAuthor")
				.HasKey(wa => new { wa.WorkId, wa.AuthorId });

			modelBuilder.Entity<WorkAuthor>()
				.HasOne(wa => wa.BookWork)
				.WithMany(bw => bw.WorkAuthors)
				.HasForeignKey(wa => wa.WorkId);

			modelBuilder.Entity<WorkAuthor>()
				.HasOne(wa => wa.Author)
				.WithMany(a => a.WorkAuthors)  // Nếu Author có ICollection<WorkAuthor> WorkAuthors
				.HasForeignKey(wa => wa.AuthorId);

			modelBuilder.Entity<WorkCategory>()
				.ToTable("WorkCategory")
				.HasKey(wc => new { wc.WorkId, wc.CategoryId });

			modelBuilder.Entity<WorkCategory>()
				.HasOne(wc => wc.BookWork)
				.WithMany(bw => bw.WorkCategories)
				.HasForeignKey(wc => wc.WorkId);

			modelBuilder.Entity<WorkCategory>()
				.HasOne(wc => wc.Category)
				.WithMany(c => c.WorkCategories)
				.HasForeignKey(wc => wc.CategoryId);

			// Các config khác nếu có, giữ nguyên
		}
	}
}