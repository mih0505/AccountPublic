using Account.DAL.Entities;
using Accounts.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Account.DAL.Interfaces
{
    public interface IEFRPDRepository : IEFRepository<RPD>
    {
        ApplicationUser GetRPDAuthor(int rpdId);

        IEnumerable<RPD> GetRPDsByPlan(int decanatDisciplineId, int decanatPlanId);
    }
}