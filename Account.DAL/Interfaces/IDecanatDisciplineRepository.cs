using Account.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Account.DAL.Interfaces
{
    public interface IDecanatDisciplineRepository : IDecanatRepository<DecanatDiscipline>
    {
        /// <summary>
        /// Получает список дисциплин по идентификатору кафедры
        /// </summary>
        /// <param name="departmentId">Идентификатор кафедры</param>
        /// <param name="year">Выбранный учебный год</param>
        /// <returns></returns>
        Task<IEnumerable<DecanatDiscipline>> GetAllByDepartmentAsync(int departmentId, string year, bool hide);

        Task<IEnumerable<DecanatDiscipline>> GetAllByFilterAsync(int departmentId, string year, bool hide, string filterText);

        /// <summary>
        /// Получает список курсов, на которых изучается дисциплина
        /// </summary>
        /// <param name="planId">Идентификатор плана</param>
        /// <param name="disciplineId">Идентификатор дисциплины</param>
        /// <returns></returns>
        IEnumerable<int> GetDisciplineCoursesByPlan(int planId, int disciplineId);

        /// <summary>
        /// Получает список семестров, в которых изучается дисциплина
        /// </summary>
        /// <param name="planId">Идентификатор плана</param>
        /// <param name="disciplineId">Идентификатор дисциплины</param>
        /// <returns></returns>
        IEnumerable<int> GetDisciplineSemestersByPlan(int planId, int disciplineId);

        #region Временное исключение запросов
        /// <summary>
        /// Получает трудоёмкость дисциплины, выраженную в ЗЕТ
        /// </summary>
        /// <param name="planId">Идентификатор плана</param>
        /// <param name="disciplineId">Идентификатор дисциплины</param>
        /// <returns></returns>
        //int GetDisciplineZET(int planId, int disciplineId);
        
        /// <summary>
        /// Получает трудоёмкость дисциплины, выраженную в часах
        /// </summary>
        /// <param name="planId">Идентификатор плана</param>
        /// <param name="disciplineId">Идентификатор дисциплины</param>
        /// <returns></returns>
        //int GetDisciplineHours(int planId, int disciplineId);

        /// <summary>
        /// Получает часы для КСР
        /// </summary>
        /// <param name="planId">Идентификатор плана</param>
        /// <param name="disciplineId">Идентификатор дисциплины</param>
        /// <returns></returns>
        //int GetDisciplineKSR(int planId, int disciplineId);
        #endregion

        /// <summary>
        /// Получает расчасовку по видам работ
        /// </summary>
        /// <param name="planId">Идентификатор плана</param>
        /// <param name="disciplineId">Идентификатор дисциплины</param>
        /// <returns></returns>
        DecanatHoursDivision GetDisciplineHoursDivision(int planId, int disciplineId);

        /// <summary>
        /// Получает виды контроля и семестры, в которых они проводятся
        /// </summary>
        /// <param name="planId">Идентификатор плана</param>
        /// <param name="disciplineId">Идентификатор дисциплины</param>
        /// <returns></returns>
        IDictionary<string, string> GetDisciplineControlSemesters(int planId, int disciplineId);

        string GetWorkHourDivision(int planId, int disciplineId, string workTypeName);
    }
}
