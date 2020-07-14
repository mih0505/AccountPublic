using Account.DAL.Contexts;
using Account.DAL.Interfaces;
using Accounts.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Account.DAL.Repositories
{
    public class EFDepartmentRepository : EFBaseRepository<Department>, IEFDepartmentRepository
    {
        public EFDepartmentRepository(AccountContext db) : base(db)
        { }

        public AccountContext AccountContext
        {
            get { return _context as AccountContext; }
        }

        public IEnumerable<Department> GetAllDepartmentOfUser(IEnumerable<int> depsId)
        {
            if (depsId == null)
            {
                throw new ArgumentNullException(nameof(depsId), "Входное значение параметра метода GetAllDepartmentOfUser() равно null");
            }

            try
            {
                return _dbSet.Where(a => depsId.Contains(a.Id)).ToList();
            }
            catch (Exception ex)
            {
                _msgr($"Ошибка в методе GetAllDepartmentOfUser(): {ex}");
                throw;
            }
        }
    }
}