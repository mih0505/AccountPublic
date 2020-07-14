using Account.DAL.Contexts;
using Account.DAL.Interfaces;
using Accounts.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Account.DAL.Repositories
{
    public class EFTeacherDepartmentRepository : EFBaseRepository<TeacherDepartment>, IEFTeacherDepartmentRepository
    {
        public EFTeacherDepartmentRepository(AccountContext context) : base(context)
        { }

        public IEnumerable<ApplicationUser> GetDepartmentTeachers(int decanatDepartmentId)
        {
            var departmentTeachers = _context.TeacherDepartments
                                             .Include(TeacherDepartment => TeacherDepartment.Teacher)
                                             .Include(TeacherDepartment => TeacherDepartment.Department.DecanatID)
                                             .Where(TeacherDepartment => TeacherDepartment.Department.DecanatID.HasValue && TeacherDepartment.Department.DecanatID.Value.Equals(decanatDepartmentId))
                                             .Select(TeacherDepartment => TeacherDepartment.Teacher)
                                             .ToList();

            return departmentTeachers;
        }

        public IEnumerable<int> GetAllIdDepartmentsOfUser(string userId)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId), "Входное значение параметра метода GetAllIdDepartmentsOfUser() равно null");
            }

            try
            {
                return _dbSet.Where(a => a.TeacherId == userId).Select(a => a.DepartmentId).ToList();
            }
            catch (Exception ex)
            {
                _msgr($"Ошибка в методе GetAllIdDepartmentsOfUser(): {ex}");
                throw;
            }
        }

        public IEnumerable<int> GetAllUserDecanatDepartmentsIds(string userId)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId), "Входное значение параметра метода GetAllIdDepartmentsOfUser() равно null");
            }

            try
            {
                return _dbSet.Include(a => a.Department).Where(a => a.TeacherId == userId && a.Department.DecanatID.HasValue).Select(a => a.Department.DecanatID.Value).ToList();
            }
            catch (Exception ex)
            {
                _msgr($"Ошибка в методе GetAllIdDepartmentsOfUser(): {ex}");
                throw;
            }
        }
    }
}
