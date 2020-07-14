using Account.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Account.DAL.Interfaces
{
    public interface IEFThematicContentRepository : IEFRepository<ThematicContent>
    {
        Task<IEnumerable<ThematicContent>> GetRPDsThematicContentsAsync(int RPDId);
        Task<IEnumerable<ThematicContent>> GetRPDsThematicContentsByLessonType(int RPDId, string lessonType);
    }
}
