using Account.DAL.Entities;
using System.Collections.Generic;

namespace Account.DAL.Interfaces
{
    public interface IEFCompetenceRepository : IEFRepository<Competence>
    {
        Competence GetCompetenceDescription(int decanatCompetenceId);
        Competence GetCompetenceDescriptionByRPD(int decanatCompetenceId, int rpdId);
        IEnumerable<Competence> GetCompetencesDescription(int RPDId);
    }
}