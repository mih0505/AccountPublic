using Account.DAL.Entities;
using System.Collections.Generic;

namespace Account.DAL.Interfaces
{
    public interface IDecanatCompetenceRepository : IDecanatRepository<DecanatCompetence>
    {
        /// <summary>
        /// Получает список компетенций для дисциплины
        /// </summary>
        /// <param name="disciplineId">Идентификатор дисциплины</param>
        /// <param name="planId">Идентификатор плана</param>
        /// <returns></returns>
        IEnumerable<DecanatCompetence> GetAllByDiscipline(int disciplineId, int planId);
    }
}
