using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Accounts.Models
{
    public class Journal
    {
        [Key]
        public int Id { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int GroupIdDecanat { get; set; }

        
        [Display(Name = "Группа")]
        public string GroupName { get; set; }

        [Required]
        public int GroupId { get; set; }
        public Group Group { get; set; }

        [Required]
        [Display(Name = "Факультет")]
        public int FacultyId { get; set; }
        public Faculty Faculty { get; set; }

        [Required]
        [Display(Name = "Дисциплина")]
        public string Discipline { get; set; }

        [Display(Name = "Учебный год")]
        public string Year { get; set; }

        [Display(Name = "Семестр")]
        public int? Semester { get; set; }

        [Display(Name = "Вид контроля")]
        public string TypeControl { get; set; }

        [Display(Name = "Преподаватель")]
        public string TeacherNameId { get; set; }
        public virtual ApplicationUser TeacherName { get; set; }

        public bool IsDeleted { get; set; }

    }
}