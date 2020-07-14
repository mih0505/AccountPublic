using Account.DAL.Contexts;
using Account.DAL.Entities;
using Account.DAL.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Account.DAL.Repositories
{
    public class EFCompetenceGradeRepository : EFBaseRepository<CompetenceGrade>, IEFCompetenceGradeRepository
    {
        public EFCompetenceGradeRepository(AccountContext db) : base(db) { }

        public IEnumerable<CompetenceGrade> GetCompetenceGrades(int decanatId)
        {
            var competenceGrades = _context.CompetenceGrades
                                   .Where(CompetenceGrade => CompetenceGrade.DecanatId.Equals(decanatId))
                                   .OrderBy(CompetenceGrade => CompetenceGrade.Stage)
                                   .ToList();

            return new BindingList<CompetenceGrade>(competenceGrades); 
        }
    }
}
