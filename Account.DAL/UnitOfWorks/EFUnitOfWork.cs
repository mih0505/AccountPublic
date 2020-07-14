using Account.DAL.Contexts;
using Account.DAL.Interfaces;
using Account.DAL.References;
using Account.DAL.Repositories;
using Accounts.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Account.DAL.UnitOfWorks
{
    public class EFUnitOfWork : IEFUnitOfWork
    {
        private AccountContext _db;
        public ApplicationUserManager UserManager { get; set; }
        public ApplicationRoleManager RoleManager { get; set; }

        public IEFRPDRepository RPDs { get; private set; }
        public IEFMemberRepository Members { get; private set; }
        public IEFTeacherDepartmentRepository TeacherDepartments { get; private set; }
        public IEFDepartmentRepository Departments { get; private set; }
        public IEFStandardRepository EducationStandards { get; set; }
        public IEFCompetenceRepository Competences { get; set; }
        public IEFRPDContentRepository RPDContents { get; set; }
        public IEFRPDItemRepository RPDItems { get; set; }
        public IEFThematicPlanRepository ThematicPlans { get; set; }
        public IEFThematicContentRepository ThematicContents { get; set; }
        public IEFCompetenceGradeRepository CompetenceGrades { get; set; }
        public IEFBasicLiteratureRepository BasicLiteratures { get; set; }
        public IEFAdditionalLiteratureRepository AdditionalLiteratures { get; set; }
        public IEFLibrarySystemRepository LibrarySystems { get; set; }
        public IEFInternetResourceRepository InternetResources { get; set; }
        public IEFLicenceRepository Licences { get; set; }
        public IEFMaterialBaseRepository MaterialBases { get; set; }
        public IEFDocumentTypeRepository DocumentTypeRepository { get; set; }

        public EFUnitOfWork()
        {
            _db = new AccountContext();
            RPDs = new EFRPDRepository(_db);
            Members = new EFMemberRepository(_db);
            UserManager = new ApplicationUserManager(new UserStore<ApplicationUser>(_db));
            RoleManager = new ApplicationRoleManager(new RoleStore<ApplicationRole>(_db));
            TeacherDepartments = new EFTeacherDepartmentRepository(_db);
            Departments = new EFDepartmentRepository(_db);
            EducationStandards = new EFStandardRepository(_db);
            Competences = new EFCompetenceRepository(_db);
            RPDContents = new EFRPDContentRepository(_db);
            RPDItems = new EFRPDItemRepository(_db);
            ThematicPlans = new EFThematicPlanRepository(_db);
            ThematicContents = new EFThematicContentRepository(_db);
            CompetenceGrades = new EFCompetenceGradeRepository(_db);
            BasicLiteratures = new EFBasicLiteratureRepository(_db);
            AdditionalLiteratures = new EFAdditionalLiteratureRepository(_db);
            LibrarySystems = new EFLibrarySystemRepository(_db);
            InternetResources = new EFInternetResourceRepository(_db);
            Licences = new EFLicenceRepository(_db);
            MaterialBases = new EFMaterialBaseRepository(_db);
            DocumentTypeRepository = new EFDocumentTypeRepository(_db);
        }

        public void Save()
        {
            try
            {
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                // ToDo: написать нормальный вывод ошибки
                Debug.WriteLine($"Ошибка сохранения данных: {ex}");
                throw;
            }
        }

        public async Task SaveAsync()
        {
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // ToDo: написать нормальный вывод ошибки
                Debug.WriteLine($"Ошибка сохранения данных: {ex}");
                throw;
            }
        }

        public void Update<TEntity>(TEntity entity) where TEntity : class
        {
            _db.Entry(entity).State = EntityState.Modified;
        }

        public void Update<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        {
            foreach (var entity in entities)
            {
                _db.Entry(entity).State = EntityState.Modified;
            }
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
