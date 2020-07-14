using Account.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Account.DAL.Interfaces
{
    public interface IEFBasicLiteratureRepository : IEFRepository<BasicLiterature>
    {
        Task<IEnumerable<BasicLiterature>> GetRPDsBasicLiteraturesAsync(int RPDId);
    }
}
