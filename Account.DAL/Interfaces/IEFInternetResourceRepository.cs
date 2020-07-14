using Account.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Account.DAL.Interfaces
{
    public interface IEFInternetResourceRepository : IEFRepository<InternetResource>
    {
        Task<IEnumerable<InternetResource>> GetRPDsInternetResourcesAsync(int RPDId);
    }
}
