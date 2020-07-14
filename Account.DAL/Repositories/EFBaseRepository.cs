using Account.DAL.Contexts;
using Account.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Linq.Expressions;

namespace Account.DAL.Repositories
{
    public delegate void Messenger(string message);

    public class EFBaseRepository<TEntity> : IEFRepository<TEntity> where TEntity : class
    {
        protected readonly AccountContext _context;
        public DbSet<TEntity> _dbSet;

        public EFBaseRepository(AccountContext context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }

        public TEntity Add(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "Входное значение параметра метода Add() равно null");
            }
            
            try
            {
                return _dbSet.Add(entity);                
            }
            catch (Exception ex)
            {
                _msgr($"Ошибка в методе Add(): {ex}");
                throw;
            }
        }

        public IEnumerable<TEntity> AddRange(IEnumerable<TEntity> entities)
        {
            if (entities is null)
            {
                throw new ArgumentNullException(nameof(entities), "Входное значение параметра метода Add() равно null");
            }

            try
            {
                return _dbSet.AddRange(entities);
            }

            catch (Exception ex)
            {
                _msgr($"Ошибка в методе Add(): {ex}");
                throw;
            }
        }

        public TEntity Find(Expression<Func<TEntity, bool>> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate), "Входное значение параметра метода Find() равно null");
            }

            try
            {
                return _dbSet.FirstOrDefault(predicate);
            }
            catch (DbEntityValidationException dbEx)
            {
                string errorMessage = string.Empty;
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        errorMessage += string.Format("Property: {0} Error: {1}",
                        validationError.PropertyName, validationError.ErrorMessage) + Environment.NewLine;
                    }
                }
                throw new Exception(errorMessage, dbEx);
            }
            catch (Exception ex)
            {
                _msgr($"Ошибка в методе Find(): {ex}");
                throw;
            }
        }

        public TEntity Get(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id), "Отсутствует или некорректный идентификатор записи");
            }

            try
            {
                return _dbSet.Find(id);
            }
            catch (Exception ex)
            {
                _msgr($"Ошибка в методе Get(): {ex}");
                throw;
            }
        }

        public IEnumerable<TEntity> GetAll()
        {
            try
            {
                return _dbSet.ToList();
            }
            catch (Exception ex)
            {
                _msgr($"Ошибка в методе GetAll(): {ex}");
                throw;
            }
        }

        public IEnumerable<TEntity> GetAll(Expression<Func<TEntity, bool>> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate), "Входное значение параметра метода GetAll() равно null");
            }

            try
            {
                return _dbSet.Where(predicate).ToList();
            }
            catch(Exception ex)
            {
                _msgr($"Ошибка в методе GetAll(): {ex}");
                throw;
            }
        }

        public void Remove(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "Входное значение параметра метода Remove() равно null");
            }

            try
            {
                _dbSet.Remove(entity);
            }
            catch (Exception ex)
            {
                _msgr($"Ошибка в методе Remove(): {ex}");
            }
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            if (entities is null)
            {
                throw new ArgumentNullException(nameof(entities), "Входное значение параметра метода RemoveRange() равно null");
            }

            try
            {
                _dbSet.RemoveRange(entities);
            }

            catch (Exception ex)
            {
                _msgr($"Ошибка в методе RemoveRange(): {ex}");
            }
        }
        
        protected Messenger _msgr;

        public void MessageHandler(Messenger msgr)
        {
            _msgr = msgr;
        }
    }
}
