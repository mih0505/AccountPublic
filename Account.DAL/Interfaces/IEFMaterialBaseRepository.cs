using Account.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Account.DAL.Interfaces
{
    public interface IEFMaterialBaseRepository : IEFRepository<MaterialBase>
    {
        Task<IEnumerable<MaterialBase>> GetRPDsMaterialBasesAsync(int RPDId);
    }
}
