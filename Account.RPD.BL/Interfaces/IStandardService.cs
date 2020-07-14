using Account.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AccountRPD.BL.Interfaces
{
    public interface IStandardService
    {
        IEnumerable<EducationStandard> GetAllStandard();

        Task SaveStandardAsync(EducationStandard standard);

        Task RemoveStandardAsync(EducationStandard standard);
    }
}