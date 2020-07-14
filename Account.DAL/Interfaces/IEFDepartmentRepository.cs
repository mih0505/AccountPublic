using Accounts.Models;
using System.Collections.Generic;

namespace Account.DAL.Interfaces
{
    public interface IEFDepartmentRepository : IEFRepository<Department>
    {
        /// <summary>
        /// Получение всех идентификаторов кафедр, за которыми закреплен пользователь
        /// </summary>
        /// <param name="predicate">Предикат определяющий условие для выборки сущностей</param>
        /// <returns></returns>
        IEnumerable<Department> GetAllDepartmentOfUser(IEnumerable<int> depsId);
    }
}