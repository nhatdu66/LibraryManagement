using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryManagementSystem.Data.Entities;


namespace LibraryManagementSystem.Services.Interfaces
{
    public interface IBookWorkService 
    {
        Task<IEnumerable<BookWork>> GetWorks();

        Task<IEnumerable<BookWork>> GetWorksPaging(string keywords, int page, int pageSize);

        Task<int> GetTotalPages(string keywords, int pageSize);
    }
}
