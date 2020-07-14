using Account.DAL.Entities;
using Account.DAL.Interfaces;
using System.Collections.Generic;

namespace Account.DAL.Interfaces
{
    public interface IEFThematicPlanRepository : IEFRepository<ThematicPlan>
    {
        IEnumerable<ThematicPlan> GetRPDsThematicPlans(int RPDId);
    }
}