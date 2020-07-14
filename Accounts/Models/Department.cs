using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Accounts.Models
{
    public class Department
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Название кафедры")]
        [StringLength(500, ErrorMessage = "Максимальная длина 500 символов.")]
        public string Name { get; set; }

        [Display(Name = "Название кафедры")]
        [StringLength(10, ErrorMessage = "Максимальная длина 10 символов.")]
        public string ShortName { get; set; }

        [Display(Name = "Заведующий кафедрой")]
        [StringLength(150, ErrorMessage = "Максимальная длина 150 символов.")]
        public string Boss { get; set; }

        [Display(Name = "Секретарь кафедры")]
        [StringLength(150, ErrorMessage = "Максимальная длина 150 символов.")]
        public string Secretary { get; set; }

        [Display(Name = "Номер")]                
        public int? Number { get; set; }

        [Display(Name = "Факультет")]
        public int? FacultyId { get; set; }
        public Faculty Faculty { get; set; }

        [Display(Name = "Корпус")]
        [StringLength(200, ErrorMessage = "Максимальная длина 200 символов.")]
        public string Corps { get; set; }

        [Display(Name = "Кабинет")]
        [StringLength(10, ErrorMessage = "Максимальная длина 10 символов.")]
        public string Room { get; set; }
        
        public int? DecanatID { get; set; }

        [Display(Name = "Удалена")]        
        public bool IsDeleted { get; set; }
                
        public ICollection<Profile> Profiles { get; set; }
        public ICollection<Course> Courses { get; set; }
        public ICollection<Statement> Statements { get; set; }
        public ICollection<TeacherDepartment> TeacherDepartments { get; set; }
        public Department()
        {            
            Profiles = new List<Profile>();
            Courses = new List<Course>();
            TeacherDepartments = new List<TeacherDepartment>();
            Statements = new List<Statement>();
        }
    }
}