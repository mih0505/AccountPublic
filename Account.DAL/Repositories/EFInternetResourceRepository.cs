using Account.DAL.Contexts;
using Account.DAL.Entities;
using Account.DAL.Interfaces;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Account.DAL.Repositories
{
    public class EFInternetResourceRepository : EFBaseRepository<InternetResource>, IEFInternetResourceRepository
    {
        public EFInternetResourceRepository(AccountContext context)
            : base(context)
        { }

        public async Task<IEnumerable<InternetResource>> GetRPDsInternetResourcesAsync(int RPDId)
        {
            return await _context.InternetResources.Where(InternetResource => InternetResource.RPDId.Equals(RPDId)).ToListAsync();
        }
    }
}
