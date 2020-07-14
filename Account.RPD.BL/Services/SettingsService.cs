using Account.DAL.Entities;
using Account.DAL.Interfaces;
using AccountRPD.BL.Interfaces;
using System.Collections.Generic;

namespace AccountRPD.BL.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly IEFUnitOfWork efUnitOfWork;

        public SettingsService(IEFUnitOfWork efUnitOfWork)
        {
            this.efUnitOfWork = efUnitOfWork;
        }

        //public IEnumerable<EducationStandard> GetAllStandard()
        //{
        //    return efUnitOfWork.EducationStandards.GetAll();
        //}

        //public IEnumerable<RPDItem> GetRPDItems(int standardId)
        //{
        //    return efUnitOfWork.RPDItems.GetAll(a => a.EducationStandardId == standardId);
        //}
    }
}
