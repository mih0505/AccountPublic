using Account.DAL.Entities;
using Account.DAL.Interfaces;
using AccountRPD.BL.Infrastructure;
using AccountRPD.BL.Interfaces;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountRPD.BL.Services
{
    public class ManagerService : IManagerService
    {
        private readonly IDecanatUnitOfWork decanatUnitOfWork;
        private readonly IEFUnitOfWork efUnitOfWork;

        public ManagerService(IDecanatUnitOfWork decanatUnitOfWork, IEFUnitOfWork efUnitOfWork)
        {
            this.decanatUnitOfWork = decanatUnitOfWork;
            this.efUnitOfWork = efUnitOfWork;
        }

        public IEnumerable<DecanatDepartment> GetAllDepartments()
        {
            return decanatUnitOfWork.Departments.GetAll();
        }

        public IEnumerable<DecanatDepartment> GetDepartmentsUser()
        {
            IEnumerable<DecanatDepartment> lstDepartments = new List<DecanatDepartment>();
            var session = Session.GetSession();
            if (session.User != null)
            {
                if (!efUnitOfWork.UserManager.IsInRole(session.User.Id, "Administrators"))
                {
                    var userDepartmentsIds = efUnitOfWork.TeacherDepartments.GetAllUserDecanatDepartmentsIds(session.User.Id).ToList();
                    lstDepartments = decanatUnitOfWork.Departments.GetUserDepartments(userDepartmentsIds);
                }
                else
                {
                    lstDepartments = decanatUnitOfWork.Departments.GetAll();
                }
            }

            return lstDepartments;
        }

        public IEnumerable<string> GetStudyYears()
        {
            return decanatUnitOfWork.GetAllStudyYears();
        }

        public async Task<IEnumerable<DecanatDiscipline>> GetDisciplinesByDepartmentAsync(int departmentId, string year, bool hideNotActualPlans)
        {
            return await decanatUnitOfWork.Disciplines.GetAllByDepartmentAsync(departmentId, year, hideNotActualPlans);
        }

        public async Task<IEnumerable<DecanatDiscipline>> GetDisciplinesByFilterAsync(int departmentId, string year, bool hideNotActualPlans, string filterText)
        {
            return await decanatUnitOfWork.Disciplines.GetAllByFilterAsync(departmentId, year, hideNotActualPlans, filterText);
        }

        public IEnumerable<DecanatPlan> GetPlansByDiscipline(string discipline, int departmentId, int type, string year, bool hideNotActualPlans)
        {
            return decanatUnitOfWork.Plans.GetAllByDiscipline(discipline, departmentId, type, year, hideNotActualPlans);
        }

        public IEnumerable<RPD> GetRPDsByPlan(int decanatDisciplineId, int decanatPlanId)
        {
            return efUnitOfWork.RPDs.GetRPDsByPlan(decanatDisciplineId, decanatPlanId);
        }

        public void RemoveRPD(int RPDId)
        {
            var rpd = efUnitOfWork.RPDs.Get(RPDId);
            efUnitOfWork.RPDs.Remove(rpd);
            efUnitOfWork.Save();
        }

        public async Task RemoveRPDAsync(int RPDId)
        {
            var rpd = efUnitOfWork.RPDs.Get(RPDId);
            efUnitOfWork.RPDs.Remove(rpd);
            await efUnitOfWork.SaveAsync();
        }
    }
}