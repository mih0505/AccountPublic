using Accounts.Models;
using System.Collections.Generic;

namespace Account.DAL.Interfaces
{
    public interface IEFTeacherDepartmentRepository : IEFRepository<TeacherDepartment>
    {
        IEnumerable<ApplicationUser> GetDepartmentTeachers(int decanatDepartmentId);

        /// <summary>
        /// Получает список всех идентификаторов кафедр, привязанных к пользователю
        /// </summary>
        /// <param name="predicate">Предикат, определяющий условие выборки сущности</param>
        /// <returns></returns>
        IEnumerable<int> GetAllIdDepartmentsOfUser(string userId);

        /// <summary>
        /// Получает список всех идентификаторов кафедр из Деканата, привязанных к пользователю
        /// </summary>
        /// <param name="predicate">Предикат, определяющий условие выборки сущности</param>
        /// <returns></returns>
        IEnumerable<int> GetAllUserDecanatDepartmentsIds(string userId);
    }
}
