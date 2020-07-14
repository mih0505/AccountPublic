using Account.DAL.Contexts;
using Account.DAL.Entities;
using Account.DAL.Interfaces;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Account.DAL.Repositories
{
    public class EFMaterialBaseRepository : EFBaseRepository<MaterialBase>, IEFMaterialBaseRepository
    {
        public EFMaterialBaseRepository(AccountContext context)
            : base(context)
        { }

        public async Task<IEnumerable<MaterialBase>> GetRPDsMaterialBasesAsync(int RPDId)
        {
            return await _context.MaterialBases.Where(MaterialBase => MaterialBase.RPDId.Equals(RPDId)).ToListAsync();
        }
    }
}
