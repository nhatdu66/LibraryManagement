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
		public LibraryDbContext()
		{
		}

		// Constructor chính - nhận DbContextOptions (sẽ được inject từ DI sau này)
		public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
			: base(options)
		{
		}

		// DbSet cho tất cả các entity (giữ nguyên plural như cũ)
		public DbSet<Role> Roles { get; set; }
		public DbSet<Employee> Employees { get; set; }
		public DbSet<Reader> Readers { get; set; }
		public DbSet<Series> Series { get; set; }
		public DbSet<BookWork> BookWorks { get; set; }
		public DbSet<Author> Authors { get; set; }
		public DbSet<WorkAuthor> WorkAuthors { get; set; }
		public DbSet<Category> Categories { get; set; }
		public DbSet<WorkCategory> WorkCategories { get; set; }
		public DbSet<Publisher> Publishers { get; set; }
		public DbSet<BookEdition> BookEditions { get; set; }
		public DbSet<BookCopy> BookCopies { get; set; }
		public DbSet<BorrowRequest> BorrowRequests { get; set; }
		public DbSet<BorrowRequestDetail> BorrowRequestDetails { get; set; }
		public DbSet<BorrowTransaction> BorrowTransactions { get; set; }
		public DbSet<BorrowTransactionDetail> BorrowTransactionDetails { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			// Chỉ dùng khi chạy migration thủ công hoặc design-time
			// Trong runtime thật sẽ dùng options từ DI → nên comment dòng dưới nếu không cần
			// optionsBuilder.UseSqlServer("Server=DESKTOP-4MP3LIQ\\SQLEXPRESS03;Database=LibraryManagementDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;");
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// 1. Config composite keys cho bảng junction (many-to-many)
			modelBuilder.Entity<WorkAuthor>()
				.HasKey(wa => new { wa.WorkId, wa.AuthorId });

			modelBuilder.Entity<WorkCategory>()
				.HasKey(wc => new { wc.WorkId, wc.CategoryId });

			// 2. Config unique indexes (tương ứng UNIQUE trong DDL)
			modelBuilder.Entity<Role>()
				.HasIndex(r => r.RoleName)
				.IsUnique();

			modelBuilder.Entity<Employee>()
				.HasIndex(e => e.Email)
				.IsUnique();

			modelBuilder.Entity<Reader>()
				.HasIndex(r => r.Email)
				.IsUnique();

			modelBuilder.Entity<Reader>()
				.HasIndex(r => r.CardNumber)
				.IsUnique();

			modelBuilder.Entity<BookEdition>()
				.HasIndex(be => be.ISBN)
				.IsUnique();

			modelBuilder.Entity<BookCopy>()
				.HasIndex(bc => bc.Barcode)
				.IsUnique();

			// 3. Config delete behavior (tránh cascade không mong muốn)
			modelBuilder.Entity<BookEdition>()
				.HasOne(be => be.BookWork)
				.WithMany(bw => bw.BookEditions)
				.HasForeignKey(be => be.WorkId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<BookCopy>()
				.HasOne(bc => bc.BookEdition)
				.WithMany(be => be.BookCopies)
				.HasForeignKey(bc => bc.EditionId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<BorrowTransactionDetail>()
				.HasOne(btd => btd.BookCopy)
				.WithMany(bc => bc.BorrowTransactionDetails)
				.HasForeignKey(btd => btd.CopyId)
				.OnDelete(DeleteBehavior.Restrict);

			// 4. Config decimal precision (nếu có cột tiền phạt)
			modelBuilder.Entity<BorrowTransactionDetail>()
				.Property(btd => btd.FineAmount)
				.HasPrecision(10, 2);

			// 5. Mapping tên bảng singular (đây là phần fix lỗi Invalid object name)
			modelBuilder.Entity<Role>().ToTable("Role");
			modelBuilder.Entity<Employee>().ToTable("Employee");
			modelBuilder.Entity<Reader>().ToTable("Reader");
			modelBuilder.Entity<BookWork>().ToTable("BookWork");
			modelBuilder.Entity<Author>().ToTable("Author");
			modelBuilder.Entity<Publisher>().ToTable("Publisher");
			modelBuilder.Entity<BookEdition>().ToTable("BookEdition");
			modelBuilder.Entity<BookCopy>().ToTable("BookCopy");
			modelBuilder.Entity<BorrowRequest>().ToTable("BorrowRequest");
			modelBuilder.Entity<BorrowRequestDetail>().ToTable("BorrowRequestDetail");
			modelBuilder.Entity<BorrowTransaction>().ToTable("BorrowTransaction");
			modelBuilder.Entity<BorrowTransactionDetail>().ToTable("BorrowTransactionDetail");

			// Series đã plural nên không cần map
			// WorkAuthor/WorkCategory là junction, không cần map tên bảng riêng
		}
	}
}
