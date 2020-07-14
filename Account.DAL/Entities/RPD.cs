using Accounts.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Account.DAL.Entities
{
    public enum RpdType : int
    {
        [Description("Черновик")]
        Draft = 0,

        [Description("Завершен")]
        Complete = 1,
    };

    public class RPD
    {
        /// <summary>
        /// Идентификатор автора РПД
        /// </summary>
        [Browsable(false)]
        public string AuthorId { get; set; }

        /// <summary>
        /// Сущность автора РПД
        /// </summary>
        [Browsable(false)]
        public ApplicationUser Author { get; set; }

        /// <summary>
        /// Идентификатор РПД
        /// </summary>
        [Browsable(false)]
        public int Id { get; set; }

        [Browsable(false)]
        public int EducationStandardId { get; set; }

        [Browsable(false)]
        public EducationStandard EducationStandard { get; set; }

        /// <summary>
        /// Идентификатор дисциплины в базе деканата
        /// </summary>
        [Browsable(false)]
        public int DecanatDisciplineId { get; set; }

        /// <summary>
        /// Идентификатор плана в базе деканата
        /// </summary>
        [Browsable(false)]
        public int DecanatPlanId { get; set; }

        /// <summary>
        /// Название дисциплины
        /// </summary>
        [Browsable(false)]
        public string DisciplineName { get; set; }

        /// <summary>
        /// Идентификатор кафедры из базы деканата
        /// </summary>
        [Browsable(false)]
        public int DecanatDepartmentId { get; set; }

        /// <summary>
        /// Имя файла плана
        /// </summary>
        [DisplayName("Имя файла плана")]
        public string PlanFileName { get; set; }

        /// <summary>
        /// Название направления обучения
        /// </summary>
        [Browsable(false)]
        public string DirectionName { get; set; }

        /// <summary>
        /// Код программы обучения
        /// </summary>
        [Browsable(false)]
        public string ProfileCode { get; set; }

        /// <summary>
        /// Название программы обучения
        /// </summary>
        [Browsable(false)]
        public string ProfileName { get; set; }

        /// <summary>
        /// Заведующий кафедрой
        /// </summary>
        [Browsable(false)]
        public string DepartmentManager { get; set; }

        /// <summary>
        /// Дата утверждения РПД
        /// </summary>
        [Browsable(false)]
        public DateTime? ApprovalDateRPD { get; set; }

        /// <summary>
        /// Номер протокола утверждения РПД
        /// </summary>
        [Browsable(false)]
        public string ProtocolNumberRPD { get; set; }

        ///// <summary>
        ///// Срок действия РПД (год-год)
        ///// </summary>
        //[DisplayName("Срок действия РПД")]
        //[Browsable(false)]
        //public string ValidityPeriod { get; set; }

        /// <summary>
        /// Блок
        /// </summary>
        [Browsable(false)]
        public string Block { get; set; }

        /// <summary>
        /// Дата изменения
        /// </summary>
        [DisplayName("Дата изменения")]
        public DateTime? EditDate { get; set; }

        /// <summary>
        /// Статус РПД
        /// </summary>
        [DisplayName("Статус")]
        public RpdType RdpType { get; set; }

        /// <summary>
        /// Процент заполненности РПД
        /// </summary>
        [DisplayName("% заполненности")]
        public double FullnessPercent { get; set; }

        /// <summary>
        /// Участники составления РПД
        /// </summary>
        [Browsable(false)]
        public IEnumerable<Member> Members { get; set; }

        /// <summary>
        /// Компетенции и их содерживание
        /// </summary>
        [Browsable(false)]
        public IEnumerable<Competence> Competences { get; set; }

        /// <summary>
        /// Содержание РПД
        /// </summary>
        [Browsable(false)]
        public IEnumerable<RPDContent> RPDContents { get; set; }

        [Browsable(false)]
        public IEnumerable<ThematicContent> ThematicContents { get; set; }

        [Browsable(false)]
        public IEnumerable<BasicLiterature> BasicLiteratures { get; set; }

        [Browsable(false)]
        public IEnumerable<AdditionalLiterature> AdditionalLiteratures { get; set; }

        [Browsable(false)]
        public IEnumerable<LibrarySystem> LibrarySystems { get; set; }

        [Browsable(false)]
        public IEnumerable<InternetResource> InternetResources { get; set; }

        [Browsable(false)]
        public IEnumerable<License> Licenses { get; set; }

        [Browsable(false)]
        public IEnumerable<MaterialBase> MaterialBases { get; set; }

        public RPD()
        {
            Members = new List<Member>();
            Competences = new List<Competence>();
            RPDContents = new List<RPDContent>();
            ThematicContents = new List<ThematicContent>();
            BasicLiteratures = new List<BasicLiterature>();
            AdditionalLiteratures = new List<AdditionalLiterature>();
            LibrarySystems = new List<LibrarySystem>();
            InternetResources = new List<InternetResource>();
            Licenses = new List<License>();
            MaterialBases = new List<MaterialBase>();
        }
    }
}
