using Account.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Account.DAL.Interfaces
{
    public interface IEFRPDItemRepository : IEFRepository<RPDItem>
    {
        Task<IEnumerable<RPDItem>> GetEducationStandartRPDItemsAsync(int educationStandartId, int documentTypeId);
    }
}