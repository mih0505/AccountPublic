using Account.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Account.DAL.Interfaces
{
    public interface IEFLibrarySystemRepository : IEFRepository<LibrarySystem>
    {
        Task<IEnumerable<LibrarySystem>> GetRPDsLibrarySystemsAsync(int RPDId);
    }
}
