using Account.DAL.Contexts;
using Account.DAL.Entities;
using Account.DAL.Interfaces;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Account.DAL.Repositories
{
    public class EFBasicLiteratureRepository : EFBaseRepository<BasicLiterature>, IEFBasicLiteratureRepository
    {
        public EFBasicLiteratureRepository(AccountContext context)
            : base(context)
        { }

        public async Task<IEnumerable<BasicLiterature>> GetRPDsBasicLiteraturesAsync(int RPDId)
        {
            return await _context.BasicLiteratures.Where(BasicLiterature => BasicLiterature.RPDId.Equals(RPDId)).ToListAsync();
        }
    }
}
