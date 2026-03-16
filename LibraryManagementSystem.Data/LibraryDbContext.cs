// Full code cho LibraryDbContext.cs (giữ .ToTable("BorrowTransaction") để khớp tên bảng)
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data.Entities;

namespace LibraryManagementSystem.Data
{
	public class LibraryDbContext : DbContext
	{
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
                optionsBuilder.UseSqlServer(
					"Server=DESKTOP-4MP3LIQ\\SQLEXPRESS03;Database=LibraryManagementDB;Integrated Security=True;MultipleActiveResultSets=True;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Config table names (số ít, khớp DDL của bạn)
			modelBuilder.Entity<Role>().ToTable("Role");
			modelBuilder.Entity<Employee>().ToTable("Employee");
			modelBuilder.Entity<Reader>().ToTable("Reader");
			modelBuilder.Entity<BookWork>().ToTable("BookWork");
			modelBuilder.Entity<BookEdition>().ToTable("BookEdition");
			modelBuilder.Entity<BookCopy>().ToTable("BookCopy");
			modelBuilder.Entity<Author>().ToTable("Author");
			modelBuilder.Entity<Category>().ToTable("Category");
			modelBuilder.Entity<Publisher>().ToTable("Publisher");
			modelBuilder.Entity<Series>().ToTable("Series");
			modelBuilder.Entity<WorkAuthor>().ToTable("WorkAuthor");
			modelBuilder.Entity<WorkCategory>().ToTable("WorkCategory");
			modelBuilder.Entity<BorrowRequest>().ToTable("BorrowRequest");
			modelBuilder.Entity<BorrowRequestDetail>().ToTable("BorrowRequestDetail");
			modelBuilder.Entity<BorrowTransaction>().ToTable("BorrowTransaction");  // Key fix cho tên bảng
			modelBuilder.Entity<BorrowTransactionDetail>().ToTable("BorrowTransactionDetail");

			// Junction tables config
			modelBuilder.Entity<WorkAuthor>()
				.HasKey(wa => new { wa.WorkId, wa.AuthorId });

			modelBuilder.Entity<WorkAuthor>()
				.HasOne(wa => wa.BookWork)
				.WithMany(bw => bw.WorkAuthors)
				.HasForeignKey(wa => wa.WorkId);

			modelBuilder.Entity<WorkAuthor>()
				.HasOne(wa => wa.Author)
				.WithMany(a => a.WorkAuthors)
				.HasForeignKey(wa => wa.AuthorId);

			modelBuilder.Entity<WorkCategory>()
				.HasKey(wc => new { wc.WorkId, wc.CategoryId });

			modelBuilder.Entity<WorkCategory>()
				.HasOne(wc => wc.BookWork)
				.WithMany(bw => bw.WorkCategories)
				.HasForeignKey(wc => wc.WorkId);

			modelBuilder.Entity<WorkCategory>()
				.HasOne(wc => wc.Category)
				.WithMany(c => c.WorkCategories)
				.HasForeignKey(wc => wc.CategoryId);
		}
	}
}