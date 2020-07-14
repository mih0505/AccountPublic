using Account.DAL.Contexts;
using Account.DAL.Entities;
using Account.DAL.Interfaces;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Account.DAL.Repositories
{
    public class EFLibrarySystemRepository : EFBaseRepository<LibrarySystem>, IEFLibrarySystemRepository
    {
        public EFLibrarySystemRepository(AccountContext context)
            : base(context)
        { }

        public async Task<IEnumerable<LibrarySystem>> GetRPDsLibrarySystemsAsync(int RPDId)
        {
            return await _context.LibrarySystems.Where(LibrarySystem => LibrarySystem.RPDId.Equals(RPDId)).ToListAsync();
        }
    }
}
