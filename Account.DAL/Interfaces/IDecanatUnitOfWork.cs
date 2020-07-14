using System;
using System.Collections.Generic;

namespace Account.DAL.Interfaces
{
    public interface IDecanatUnitOfWork : IDisposable
    {
        /// <summary>
        /// Репозиторий кафедр
        /// </summary>
        IDecanatDepartmentRepository Departments { get; set; }

        /// <summary>
        /// Репозиторий дисциплин
        /// </summary>
        IDecanatDisciplineRepository Disciplines { get; set; }

        /// <summary>
        /// Репозиторий планов
        /// </summary>
        IDecanatPlanRepository Plans { get; set; }

        /// <summary>
        /// Репозиторий компетенций
        /// </summary>
        IDecanatCompetenceRepository Competences { get; set; }

        /// <summary>
        /// Получение всех актуальных учебных годов
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetAllStudyYears();

        /// <summary>
        /// Получение текущего учебного 
        /// </summary>
        /// <param name="currentYear"></param>
        /// <returns></returns>
        string GetStudyYear(DateTime currentDate);
    }
}
