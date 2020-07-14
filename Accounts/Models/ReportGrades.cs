using System.ComponentModel.DataAnnotations;

namespace Accounts.Models
{
    public class ReportGrades
    {
        [Required]
        [Display(Name = "Факультет")]
        public int FacultyId { get; set; }
        public Faculty Faculty { get; set; }

        [Display(Name = "Группа")]
        public int? GroupId { get; set; }
        public Group Group { get; set; }

        [Display(Name = "Учебный год")]
        public string Years { get; set; }

        [Required]
        [Display(Name = "Сессия")]
        public string Semester { get; set; }
    }
}