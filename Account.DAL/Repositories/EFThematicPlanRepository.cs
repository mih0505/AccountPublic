using Account.DAL.Contexts;
using Account.DAL.Entities;
using Account.DAL.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Account.DAL.Repositories
{
    public class EFThematicPlanRepository : EFBaseRepository<ThematicPlan>, IEFThematicPlanRepository
    {
        public EFThematicPlanRepository(AccountContext db) : base(db) { }

        public IEnumerable<ThematicPlan> GetRPDsThematicPlans(int RPDId)
        {
            return _dbSet.Where(ThematicPlan => ThematicPlan.RpdId.Equals(RPDId)).OrderBy(ThematicPlan => ThematicPlan.Number).ToList();
        }
    }
}