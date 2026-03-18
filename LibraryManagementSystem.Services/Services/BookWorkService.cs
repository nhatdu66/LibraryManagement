using LibraryManagementSystem.Data.Entities;
using LibraryManagementSystem.Repositories.Interfaces;
using LibraryManagementSystem.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace LibraryManagementSystem.Services.Services
{
    public class BookWorkService : IBookWorkService
    {
        private readonly IBookWorkRepository bookWorkRepository;

        public BookWorkService(IBookWorkRepository bookWorkRepository)
        {
            this.bookWorkRepository = bookWorkRepository;
        }

        public async Task<int> GetTotalPages(string keywords, int pageSize)
        {
            int totalRecords = await bookWorkRepository.CountTotalPage(keywords);
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            return totalPages;
        }
        public async Task<IEnumerable<BookWork>> GetWorks()
        {
            return await bookWorkRepository.GetWorks();
        }

        public async Task<IEnumerable<BookWork>> GetWorksPaging(string keywords, int page, int pageSize)
        {
            return await bookWorkRepository.GetWorksPaging(keywords, page, pageSize);
        }
    }
}
