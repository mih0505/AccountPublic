using Account.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Account.DAL.Interfaces
{
    public interface IEFRPDContentRepository : IEFRepository<RPDContent>
    {
        Task<IEnumerable<RPDContent>> GetRPDContentsAsync(int RPDId);
        Task<IEnumerable<RPDContent>> GetRPDContentsForRemoveAsync(int RPDId);
        Task<RPDContent> GetRPDContentAsync(int RPDId, int rpdItemId);
    }
}