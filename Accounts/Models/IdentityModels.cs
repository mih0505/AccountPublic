using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Accounts.Models
{
    // В профиль пользователя можно добавить дополнительные данные, если указать больше свойств для класса ApplicationUser. Подробности см. на странице https://go.microsoft.com/fwlink/?LinkID=317594.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Обратите внимание, что authenticationType должен совпадать с типом, определенным в CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Здесь добавьте утверждения пользователя
            return userIdentity;
        }

        [StringLength(100, ErrorMessage = "Максимальная длина 100 символов.")]
        [Display(Name = "Фамилия")]
        [Required]
        public string Lastname { get; set; }

        [StringLength(100, ErrorMessage = "Максимальная длина 100 символов.")]
        [Display(Name = "Имя")]
        [Required]
        public string Firstname { get; set; }

        [StringLength(100, ErrorMessage = "Максимальная длина 100 символов.")]
        [Display(Name = "Отчество")]
        public string Middlename { get; set; }

        [Display(Name = "Дата рождения")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? BirthDay { get; set; }

        [Display(Name = "Факультет")]
        public int? FacultyId { get; set; }
        public Faculty Faculty { get; set; }
        public int? idFacultyDecanat { get; set; }

        [Display(Name = "Кафедра")]
        public int? DepartmentId { get; set; }
        public Department Department { get; set; }

        public int? idGroupDecanat { get; set; }

        public int? idProfileDecanat { get; set; }

        public int? MoodleId { get; set; }//id студента в moodle

        public int? DecanatId { get; set; }//id студента в деканате

        public int? idPlanDecanat { get; set; }

        [Display(Name = "Группа")]
        public int? GroupId { get; set; }//id группы студента
        public Group Group { get; set; }

        [Display(Name = "Изображение пользователя")]
        public byte[] Image { get; set; }//аватарка

        //поля данных из электронного деканата
        public string Power { get; set; }//степень преподавателя
        public string Rank { get; set; }//звание

        [Display(Name = "Статус учащегося")]
        public int? StatusId { get; set; }
        public Status Status { get; set; }

        public string NumberOfRecordBook { get; set; }//номер зачетки
        public int? YearOfReceipt { get; set; }//год поступления
        public bool Sex { get; set; }//true - m, false - w
        public string Language { get; set; }
        public string Bases { get; set; }//основание
        public string Nationality { get; set; }
        public string School { get; set; }
        public string SchoolLocation { get; set; }
        public int? YearOfEndingSchool { get; set; }
        public bool HighAchiever { get; set; }//отличник
        public string Country { get; set; }
        public string Region { get; set; }
        [Display(Name = "Город")]
        public string City { get; set; }
        public string Postcode { get; set; }
        public string Street { get; set; }
        public string HouseApartment { get; set; }
        public DateTime? DateEnrollment { get; set; }
        public string OrderNumber { get; set; }//номер приказа
        public DateTime? ExpirationDate { get; set; }//дата окончания обучения
        public bool Village { get; set; }//село, деревня
        public string ContractNumber { get; set; }
        public int ConditionsOfEducationId { get; set; }//условия обучения

        public DateTime? DateBlocked { get; set; }//дата блокировки
        public string BlockingReason { get; set; }//причина блокировки

        [Display(Name = "Первый вход")]
        public DateTime? FirstLogin { get; set; }
        [Display(Name = "Последний вход")]
        public DateTime? LastLogin { get; set; }

        public string PassMoodle { get; set; }

        public bool ImageBlocked { get; set; }
        public string PathPlan { get; set; }
        public string NamePlan { get; set; }

        public int? DeclarationId { get; set; }
        public Declaration Declaration { get; set; }

        public virtual ICollection<Artifact> Artifacts { get; set; }
        public virtual ICollection<Discipline> Disciplines { get; set; }
        public virtual ICollection<Course> CourseWorks { get; set; }


        public virtual ICollection<Statement> Statements { get; set; }

        [InverseProperty("StudentStatement")]
        public virtual ICollection<StatementStudent> StudentStatement { get; set; }

        [InverseProperty("TeacherStatement")]
        public virtual ICollection<StatementStudent> TeacherStatement { get; set; }

        [InverseProperty("Student")]
        public virtual ICollection<CourseWorkStudent> Student { get; set; }

        [InverseProperty("Teacher")]
        public virtual ICollection<CourseWorkStudent> Teacher { get; set; }

        [InverseProperty("StudentDiplom")]
        public virtual ICollection<DiplomWork> StudentDiplom { get; set; }

        [InverseProperty("TeacherDiplom")]
        public virtual ICollection<DiplomWork> TeacherDiplom { get; set; }

        public virtual ICollection<Review> Reviews { get; set; }

        public virtual ICollection<BasicEducation> BasicEducations { get; set; }
        public virtual ICollection<AcademicDegree> AcademicDegrees { get; set; }
        public virtual ICollection<AdditionalEducation> AdditionalEducations { get; set; }
        public virtual ICollection<TeacherDepartment> TeacherDepartments { get; set; }
        public virtual ICollection<LogLogins> LogLogins { get; set; }
        public virtual ICollection<Journal> Journals { get; set; }
        public virtual ICollection<Declaration> Declarations { get; set; }
        public virtual ICollection<ReadMessage> ReadMessages { get; set; }

        public virtual ICollection<DiplomWork> DiplomWorks { get; set; }

        //[InverseProperty("Worker")]
        //public virtual ICollection<History> Worker { get; set; }

        public bool Consent { get; set; }
        public string Answer { get; set; }
        public string Question { get; set; }
        public bool Employer { get; set; }

        public ApplicationUser() : base()
        {
            BasicEducations = new List<BasicEducation>();
            AcademicDegrees = new List<AcademicDegree>();
            AdditionalEducations = new List<AdditionalEducation>();
            Artifacts = new List<Artifact>();
            Disciplines = new List<Discipline>();
            CourseWorks = new List<Course>();
            Student = new List<CourseWorkStudent>();
            Teacher = new List<CourseWorkStudent>();
            Reviews = new List<Review>();
            Statements = new List<Statement>();
            StudentStatement = new List<StatementStudent>();
            TeacherStatement = new List<StatementStudent>();
            TeacherDepartments = new List<TeacherDepartment>();
            LogLogins = new List<LogLogins>();
            DiplomWorks = new List<DiplomWork>();
            //Worker = new List<History>();
            Journals = new List<Journal>();
            ReadMessages = new List<ReadMessage>();
        }

        public string LastnameFM
        {
            get
            {
                string lfm = Lastname + " " + Firstname.Substring(0, 1) + ".";
                lfm += (!String.IsNullOrEmpty(Middlename)) ? Middlename.Substring(0, 1) + "." : "";
                return lfm;
            }
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Section> Sections { get; set; }
        public DbSet<Artifact> Artifacts { get; set; }
        public DbSet<Catigory> Catigories { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<Faculty> Faculties { get; set; }
        public DbSet<Settings> Settings { get; set; }
        public DbSet<FormOfTraining> FormOfTrainings { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<DirectionOfTraining> DirectionOfTrainings { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<ConditionsOfEducation> ConditionsOfEducations { get; set; }
        public DbSet<Discipline> Disciplines { get; set; }
        public DbSet<Tutor> Tutors { get; set; }
        public DbSet<TeacherIdDecanat> TeachersIdDecanat { get; set; }
        public DbSet<ChangeEmail> ChangeEmails { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseWorkStudent> CourseWorkStudents { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<BasicEducation> BasicEducations { get; set; }
        public DbSet<AcademicDegree> AcademicDegrees { get; set; }
        public DbSet<AdditionalEducation> AdditionalEducations { get; set; }
        public DbSet<Statement> Statements { get; set; }
        public DbSet<StatementStudent> StatementStudents { get; set; }
        public DbSet<TeacherDepartment> TeacherDepartments { get; set; }
        public DbSet<LogLogins> LogLogins { get; set; }
        public DbSet<Journal> Journals { get; set; }
        public DbSet<Study> Studies { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ReadMessage> ReadMessages { get; set; }
        public DbSet<Declaration> Declarations { get; set; }
        public DbSet<GIA> GIAs { get; set; }
        public DbSet<DiplomWork> DiplomWorks { get; set; }
        //public DbSet<TypeOrder> TypeMoves { get; set; }

        public ApplicationDbContext(string connectionString = "DefaultConnection")
            : base(connectionString, throwIfV1Schema: false)
        { }

        //public ApplicationDbContext(string conectionString = "DefaultConnection")
        //    : base("DefaultConnection", throwIfV1Schema: false)
        //{
        //}

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

    }
}