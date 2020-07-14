using Account.DAL.Entities;
using Account.DAL.Interfaces;
using AccountRPD.BL.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AccountRPD.BL.Services
{
    public class StandardService : IStandardService
    {
        private readonly IEFUnitOfWork efUnitOfWork;

        public StandardService(IEFUnitOfWork efUnitOfWork)
        {
            this.efUnitOfWork = efUnitOfWork;
        }

        public IEnumerable<EducationStandard> GetAllStandard()
        {
            return efUnitOfWork.EducationStandards.GetAll();
        }

        public async Task SaveStandardAsync(EducationStandard standard)
        {
            if (standard.Id < 1)
            {
                efUnitOfWork.EducationStandards.Add(standard);
            }
            else
            {
                efUnitOfWork.Update(standard);
            }
            await efUnitOfWork.SaveAsync();
        }

        public async Task RemoveStandardAsync(EducationStandard standard)
        {
            efUnitOfWork.EducationStandards.Remove(standard);
            await efUnitOfWork.SaveAsync();
        }

        public void Dispose()
        {
            efUnitOfWork.Dispose();
        }
    }
}