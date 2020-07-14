using Account.DAL.Entities;
using System.Collections.Generic;

namespace Account.DAL.Interfaces
{
    public interface IEFCompetenceGradeRepository : IEFRepository<CompetenceGrade>
    {
        IEnumerable<CompetenceGrade> GetCompetenceGrades(int decanatId);
    }
}
