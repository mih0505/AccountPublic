using Account.DAL.Contexts;
using Account.DAL.Entities;
using Account.DAL.Interfaces;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Account.DAL.Repositories
{
    public class EFThematicContentRepository : EFBaseRepository<ThematicContent>, IEFThematicContentRepository
    {
        public EFThematicContentRepository(AccountContext db) 
            : base(db) 
        { }

        public async Task<IEnumerable<ThematicContent>> GetRPDsThematicContentsAsync(int RPDId)
        {
            return await _context.ThematicContents.Include(ThematicContent => ThematicContent.ThematicPlan).Where(ThematicContent => ThematicContent.RPDId.Equals(RPDId)).ToListAsync();
        }

        public async Task<IEnumerable<ThematicContent>> GetRPDsThematicContentsByLessonType(int RPDId, string lessonType)
        {
            return await _context.ThematicContents
                   .Include(ThematicContent => ThematicContent.ThematicPlan)
                   .Where(ThematicContent => ThematicContent.RPDId.Equals(RPDId) && ThematicContent.LessonType.Equals(lessonType))
                   .OrderBy(ThematicContent => ThematicContent.ThematicPlan.Number)
                   .ToListAsync();
        }
    }
}