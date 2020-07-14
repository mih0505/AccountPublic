using Account.DAL.Entities;
using System.Collections.Generic;

namespace Account.DAL.Interfaces
{
    public interface IDecanatPlanRepository : IDecanatRepository<DecanatPlan>
    {
        /// <summary>
        /// Получает список планов
        /// </summary>
        /// <param name="disciplineId">Идентификатор дисциплины</param>
        /// <param name="departmentId">Идентификатор кафедры</param>
        /// <param name="year">Учебный год</param>
        /// <param name="typeObj">Тип дисциплины</param>
        /// <returns></returns>
        IEnumerable<DecanatPlan> GetAllByDiscipline(string discipline, int departmentId, int typeObj, string year, bool hide);

        /// <summary>
        /// Получает актуальный список планов
        /// </summary>
        /// <param name="disciplineId">Идентификатор дисциплины</param>
        /// <param name="departmentId">Идентификатор кафедры</param>
        /// <param name="year">Учебный год</param>
        /// <returns></returns>
        //IEnumerable<DecanatPlan> GetAllActualByDiscipline(int disciplineId, int departmentId, string year);
    }
}
