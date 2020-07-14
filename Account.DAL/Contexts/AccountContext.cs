using Account.DAL.Entities;
using Account.DAL.Infrastructure;
using Accounts.Models;
using System.Data.Entity;
using Competence = Account.DAL.Entities.Competence;

namespace Account.DAL.Contexts
{
    public class AccountContext : ApplicationDbContext
    {
        /// <summary>
        /// РПД
        /// </summary>
        public DbSet<RPD> RPDs { get; set; }

        /// <summary>
        /// Участники
        /// </summary>
        public DbSet<Member> Members { get; set; }

        /// <summary>
        /// Образовательные стандарты
        /// </summary>
        public DbSet<EducationStandard> EducationStandarts { get; set; }

        /// <summary>
        /// Пункты
        /// </summary>
        public DbSet<RPDItem> RPDItems { get; set; }

        public DbSet<DocumentType> DocumentTypes { get; set; }

        /// <summary>
        /// Содержимое пунктов
        /// </summary>
        public DbSet<RPDContent> RPDContents { get; set; }

        /// <summary>
        /// Компетенции
        /// </summary>
        public DbSet<Competence> Competences { get; set; }

        /// <summary>
        /// Тематический план дисциплины
        /// </summary>
        public DbSet<ThematicPlan> ThematicPlans { get; set; }

        public DbSet<ThematicContent> ThematicContents { get; set; }

        public DbSet<CompetenceGrade> CompetenceGrades { get; set; }

        public DbSet<BasicLiterature> BasicLiteratures { get; set; }

        public DbSet<AdditionalLiterature> AdditionalLiteratures { get; set; }

        public DbSet<LibrarySystem> LibrarySystems { get; set; }

        public DbSet<InternetResource> InternetResources { get; set; }

        public DbSet<License> Licenses { get; set; }

        public DbSet<MaterialBase> MaterialBases { get; set; }
        
        public AccountContext()
            : base(SecureConnectionString.GetSecureConnectionString().Default)
        { }        
    }
}
