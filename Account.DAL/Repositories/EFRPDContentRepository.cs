using Account.DAL.Contexts;
using Account.DAL.Entities;
using Account.DAL.Interfaces;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Account.DAL.Repositories
{
    public class EFRPDContentRepository : EFBaseRepository<RPDContent>, IEFRPDContentRepository
    {
        public EFRPDContentRepository(AccountContext db) 
            : base(db) 
        { }

        public async Task<IEnumerable<RPDContent>> GetRPDContentsAsync(int RPDId)
        {
            return await _context.RPDContents.Where(RPDContent => RPDContent.RPDId.Equals(RPDId)).ToListAsync();
        }

        public async Task<IEnumerable<RPDContent>> GetRPDContentsForRemoveAsync(int RPDId)
        {
            var rpd = await _context.RPDs.FirstOrDefaultAsync(RPD => RPD.Id.Equals(RPDId));
            var rpdItem = await _context.RPDItems.FirstOrDefaultAsync(RPDItem => RPDItem.EducationStandardId.Equals(rpd.EducationStandardId) && RPDItem.ControlName.Equals("tpIndependentWork"));
            return await _context.RPDContents.Where(RPDContent => RPDContent.RPDId.Equals(RPDId) && !RPDContent.RPDItemId.Equals(rpdItem.Id)).ToListAsync();
        }

        public async Task<RPDContent> GetRPDContentAsync(int RPDId, int rpdItemId)
        {
            return await _context.RPDContents.FirstOrDefaultAsync(RPDContent => RPDContent.RPDId.Equals(RPDId) && RPDContent.RPDItemId.Equals(rpdItemId));
        }
    }
}