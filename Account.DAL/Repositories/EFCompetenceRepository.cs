using Account.DAL.Contexts;
using Account.DAL.Entities;
using Account.DAL.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Account.DAL.Repositories
{
    public class EFCompetenceRepository : EFBaseRepository<Competence>, IEFCompetenceRepository
    {
        public EFCompetenceRepository(AccountContext db) 
            : base(db) 
        { }

        public Competence GetCompetenceDescription(int decanatCompetenceId)
        {
            return _context.Competences.FirstOrDefault(Competence => Competence.DecanatId.Equals(decanatCompetenceId));
        }

        public Competence GetCompetenceDescriptionByRPD(int decanatCompetenceId, int rpdId)
        {
            return _context.Competences.FirstOrDefault(Competence => Competence.RPDId.Equals(rpdId) && Competence.DecanatId.Equals(decanatCompetenceId));
        }

        public IEnumerable<Competence> GetCompetencesDescription(int RPDId)
        {
            return _dbSet.Where(Competence => Competence.RPDId.Equals(RPDId)).ToList();
        }
    }
}