using Account.DAL.Contexts;
using Account.DAL.Entities;
using Account.DAL.Interfaces;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Account.DAL.Repositories
{
    public class EFLicenceRepository : EFBaseRepository<License>, IEFLicenceRepository
    {
        public EFLicenceRepository(AccountContext context)
            : base(context)
        { }

        public async Task<IEnumerable<License>> GetRPDsLicensesAsync(int RPDId)
        {
            return await _context.Licenses.Where(Licence => Licence.RPDId.Equals(RPDId)).ToListAsync();
        }
    }
}
