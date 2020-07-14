using Account.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Account.DAL.Interfaces
{
    public interface IEFLicenceRepository : IEFRepository<License>
    {
        Task<IEnumerable<License>> GetRPDsLicensesAsync(int RPDId);
    }
}
