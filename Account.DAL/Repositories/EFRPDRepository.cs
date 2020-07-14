using Account.DAL.Contexts;
using Account.DAL.Entities;
using Account.DAL.Interfaces;
using Accounts.Models;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;

namespace Account.DAL.Repositories
{
    public class EFRPDRepository : EFBaseRepository<RPD>, IEFRPDRepository
    {
        public EFRPDRepository(AccountContext db) 
            : base (db)
        { }

        public ApplicationUser GetRPDAuthor(int rpdId)
        {
            return _context.RPDs.Include(RPD => RPD.Author).FirstOrDefault(RPD => RPD.Id.Equals(rpdId)).Author;
        }

        public IEnumerable<RPD> GetRPDsByPlan(int decanatDisciplineId, int decanatPlanId)
        {
            return _context.RPDs.Where(RPD => RPD.DecanatDisciplineId.Equals(decanatDisciplineId) && RPD.DecanatPlanId.Equals(decanatPlanId)).ToList();
        }
    }
}