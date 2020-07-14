using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Account.DAL.Interfaces
{
    public interface IEFRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Получает сущность по ее идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сущности</param>
        /// <returns></returns>
        TEntity Get(int id);

        /// <summary>
        /// Получает список всех сущностей
        /// </summary>
        /// <returns></returns>
        IEnumerable<TEntity> GetAll();

        /// <summary>
        /// Получение список всех сущностей по заданному условию
        /// </summary>
        /// <param name="predicate">Предикат, определяющий условие выборки сущностей</param>
        /// <returns></returns>
        IEnumerable<TEntity> GetAll(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Получает сущность
        /// </summary>
        /// <param name="predicate">Предикат, определяющий условие выборки сущности</param>
        /// <returns></returns>
        TEntity Find(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Добавляет сущность в базу данных
        /// </summary>
        /// <param name="entity">Сущность, которую необходимо добавить</param>
        TEntity Add(TEntity entity);

        /// <summary>
        /// Добавляет сущности в базу данных
        /// </summary>
        /// <param name="entity">Сущности, которую необходимо добавить</param>
        IEnumerable<TEntity> AddRange(IEnumerable<TEntity> entities);

        /// <summary>
        /// Удаляет сущность из базы данных
        /// </summary>
        /// <param name="entity">Сущность, которую необходимо удалить</param>
        void Remove(TEntity entity);

        void RemoveRange(IEnumerable<TEntity> entities);
    }
}
