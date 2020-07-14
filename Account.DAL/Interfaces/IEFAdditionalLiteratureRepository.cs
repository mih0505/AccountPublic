using Account.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Account.DAL.Interfaces
{
    public interface IEFAdditionalLiteratureRepository : IEFRepository<AdditionalLiterature>
    {
        Task<IEnumerable<AdditionalLiterature>> GetRPDsAdditionalLiteraturesAsync(int RPDId);
    }
}
