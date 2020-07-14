using Account.DAL.Entities;
using System.Collections.Generic;

namespace Account.DAL.Interfaces
{
    public interface IDecanatDepartmentRepository : IDecanatRepository<DecanatDepartment>
    {
        /// <summary>
        /// Получает список кафедр, привязанных к пользователю
        /// </summary>
        /// <param name="departmentsId">Список идентификаторов кафедр, относящихся к пользователю</param>
        /// <returns></returns>
        IEnumerable<DecanatDepartment> GetUserDepartments(List<int> departmentsId);
    }
}
