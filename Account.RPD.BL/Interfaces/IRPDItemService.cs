using Account.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AccountRPD.BL.Interfaces
{
    public interface IRPDItemService
    {
        IEnumerable<RPDItem> GetRPDItems(int standardId);
        Task SaveRPDItemAsync(RPDItem item);
        Task RemoveRPDItem(RPDItem item);
    }
}