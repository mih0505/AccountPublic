using Account.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AccountRPD.BL.Interfaces
{
    public interface IManagerService
    {
        IEnumerable<DecanatDepartment> GetAllDepartments();
        IEnumerable<DecanatDepartment> GetDepartmentsUser();
        Task<IEnumerable<DecanatDiscipline>> GetDisciplinesByDepartmentAsync(int departmentId, string year, bool hideNotActualPlans);
        Task<IEnumerable<DecanatDiscipline>> GetDisciplinesByFilterAsync(int departmentId, string year, bool hideNotActualPlans, string filterText);
        IEnumerable<DecanatPlan> GetPlansByDiscipline(string discipline, int departmentId, int type, string year, bool hideNotActualPlans);
        IEnumerable<RPD> GetRPDsByPlan(int decanatDisciplineId, int decanatPlanId);
        IEnumerable<string> GetStudyYears();
        void RemoveRPD(int RPDId);
        Task RemoveRPDAsync(int RPDId);
    }
}