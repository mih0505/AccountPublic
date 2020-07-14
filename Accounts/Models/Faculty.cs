using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Accounts.Models
{
    public class Faculty
    {
        public int Id { get; set; }
        [Display(Name = "Факультет")]
        public string Name { get; set; }
        
        [Display(Name = "Аббревиатура")]
        public string AliasFaculty { get; set; }

        [Display(Name = "Декан")]
        [StringLength(150, ErrorMessage = "Максимальная длина 150 символов.")]
        public string Boss { get; set; }

        [Display(Name = "Секретарь факультета")]
        [StringLength(150, ErrorMessage = "Максимальная длина 150 символов.")]
        public string Secretary { get; set; }

        [Display(Name = "Корпус")]
        [StringLength(200, ErrorMessage = "Максимальная длина 200 символов.")]
        public string Corps { get; set; }

        [Display(Name = "Кабинет")]
        [StringLength(10, ErrorMessage = "Максимальная длина 10 символов.")]
        public string Room { get; set; }

        public int? DecanatID { get; set; }

        [Display(Name = "Удален")]
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Используется в печатных формах ведомостей, так как название факультета в Деканате и ЛК не всегда соответствует официальному
        /// </summary>
        [Display(Name = "Псевдоним факультета")]
        public string AliasFullName { get; set; }

        [Display(Name = "Декан")]
        public string AliasBoss { get; set; }

        public ICollection<ApplicationUser> Users { get; set; }
        public ICollection<Department> Departments { get; set; }
        public ICollection<Course> Courses { get; set; }
        public ICollection<Profile> Profiles { get; set; }
        public ICollection<Group> Groups { get; set; }
        public ICollection<Plan> Plans { get; set; }
        public ICollection<Journal> Journals { get; set; }
        public ICollection<GIA> GIAs { get; set; }

        public Faculty()
        {
            Departments = new List<Department>();
            Profiles = new List<Profile>();
            Groups = new List<Group>();
            Plans = new List<Plan>();
            Courses = new List<Course>();
            Journals = new List<Journal>();
            GIAs = new List<GIA>();
        }
    }
}