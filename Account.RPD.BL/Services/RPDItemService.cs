using Account.DAL.Entities;
using Account.DAL.Interfaces;
using AccountRPD.BL.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AccountRPD.BL
{
    public class RPDItemService : IRPDItemService
    {
        private readonly IEFUnitOfWork efUnitOfWork;

        public RPDItemService(IEFUnitOfWork efUnitOfWork)
        {
            this.efUnitOfWork = efUnitOfWork;
        }

        public async Task SaveRPDItemAsync(RPDItem item)
        {
            if (item.Id < 1)
            {
                efUnitOfWork.RPDItems.Add(item);
            }
            else
            {
                efUnitOfWork.Update(item);
            }
            await efUnitOfWork.SaveAsync();
        }

        public IEnumerable<RPDItem> GetRPDItems(int standardId)
        {
            return efUnitOfWork.RPDItems.GetAll(a => a.EducationStandardId == standardId);
        }

        public async Task RemoveRPDItem(RPDItem item)
        {
            efUnitOfWork.RPDItems.Remove(item);
            await efUnitOfWork.SaveAsync();
        }

        public void Dispose()
        {
            efUnitOfWork.Dispose();
        }
    }
}