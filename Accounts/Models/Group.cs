using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Accounts.Models
{
    public class Group
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Название")]
        [StringLength(50, ErrorMessage = "Максимальная длина 50 символов.")]
        public string Name { get; set; }

        public int? DecanatID { get; set; }

        public int? MoodleID { get; set; }

        //[Display(Name = "Учебный план")]
        //public int? PlanId { get; set; }
        //public Plan Plan { get; set; }
        public int? idPlanDecanat { get; set; }
        public string PlanNameDecanat { get; set; }

        [Display(Name = "Программа")]
        public int? ProfileId { get; set; }
        public Profile Profile { get; set; }

        [Display(Name = "Курс")]
        public int? Course { get; set; }

        [Display(Name = "Факультет")]
        public int? FacultyId { get; set; }
        public Faculty Faculty { get; set; }

        [Display(Name = "Год поступления")]
        public string YearOfReceipt { get; set; }

        [Display(Name = "Учебный год")]
        public string AcademicYear { get; set; }

        [Display(Name = "Срок обучения")]
        public double? Period { get; set; }

        [Display(Name = "Форма обучения")]
        public int? FormOfTrainingId { get; set; }
        public FormOfTraining FormOfTraining { get; set; }

        [Display(Name = "Удалена")]
        public bool IsDeleted { get; set; }

        public ICollection<ApplicationUser> Users { get; set; }

        public ICollection<Course> Courses { get; set; }

        public ICollection<Discipline> Disciplines { get; set; }

        public ICollection<Journal> Journals { get; set; }

        public ICollection<GIA> GIAs { get; set; }
        public Group()
        {
            Courses = new List<Course>();
            Disciplines = new List<Discipline>();
            Journals = new List<Journal>();
            GIAs = new List<GIA>();
        }
    }
}