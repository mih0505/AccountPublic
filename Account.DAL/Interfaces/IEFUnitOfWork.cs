using Account.DAL.References;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Account.DAL.Interfaces
{
    public interface IEFUnitOfWork : IDisposable
    {
        /// <summary>
        /// Менеджер пользователей
        /// </summary>
        ApplicationUserManager UserManager { get; set; }

        /// <summary>
        /// Менеджер ролей
        /// </summary>
        ApplicationRoleManager RoleManager { get; set; }

        /// <summary>
        /// Репозиторий РПД
        /// </summary>
        IEFRPDRepository RPDs { get; }

        /// <summary>
        /// Репозиторий участников
        /// </summary>
        IEFMemberRepository Members { get; }

        /// <summary>
        /// Репозиторий преподавательских кафедр
        /// </summary>
        IEFTeacherDepartmentRepository TeacherDepartments { get; }

        /// <summary>
        /// Репозиторий кафедр
        /// </summary>
        IEFDepartmentRepository Departments { get; }

        /// <summary>
        /// Репозиторий образовательных стандартов
        /// </summary>
        IEFStandardRepository EducationStandards { get; }

        /// <summary>
        /// Репозиторий компетенций
        /// </summary>
        IEFCompetenceRepository Competences { get; }

        /// <summary>
        /// Репозиторий для содержимого рабочих программ
        /// </summary>
        IEFRPDContentRepository RPDContents { get; }

        /// <summary>
        /// Репозиторий оглавления РПД
        /// </summary>
        IEFRPDItemRepository RPDItems { get; }

        /// <summary>
        /// Репозиторий для доступа к тематическому плану дисциплины
        /// </summary>
        IEFThematicPlanRepository ThematicPlans { get; }

        IEFThematicContentRepository ThematicContents { get; set; }

        IEFCompetenceGradeRepository CompetenceGrades { get; set; }

        IEFBasicLiteratureRepository BasicLiteratures { get; set; }
        IEFAdditionalLiteratureRepository AdditionalLiteratures { get; set; }
        IEFLibrarySystemRepository LibrarySystems { get; set; }
        IEFInternetResourceRepository InternetResources { get; set; }
        IEFLicenceRepository Licences { get; set; }
        IEFMaterialBaseRepository MaterialBases { get; set; }

        void Save();

        /// <summary>
        /// Сохраняет все изменения в базе данных
        /// </summary>
        /// <returns></returns>
        Task SaveAsync();

        void Update<TEntity>(TEntity entity) where TEntity : class;

        void Update<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
    }
}
