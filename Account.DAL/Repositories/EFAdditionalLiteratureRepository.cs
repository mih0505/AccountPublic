using Account.DAL.Contexts;
using Account.DAL.Entities;
using Account.DAL.Interfaces;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Account.DAL.Repositories
{
    public class EFAdditionalLiteratureRepository : EFBaseRepository<AdditionalLiterature>, IEFAdditionalLiteratureRepository
    {
        public EFAdditionalLiteratureRepository(AccountContext db) 
            : base(db) 
        { }

        public async Task<IEnumerable<AdditionalLiterature>> GetRPDsAdditionalLiteraturesAsync(int RPDId)
        {
            return await _context.AdditionalLiteratures.Where(AdditionalLiterature => AdditionalLiterature.RPDId.Equals(RPDId)).ToListAsync();
        }
    }
}
