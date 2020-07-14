using System.Collections.Generic;

namespace Account.DAL.Interfaces
{
    public interface IDecanatRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Получения сущность по ее идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns></returns>
        TEntity Get(int id);

        /// <summary>
        /// Получает все сущности
        /// </summary>
        /// <returns></returns>
        IEnumerable<TEntity> GetAll();
    }
}
