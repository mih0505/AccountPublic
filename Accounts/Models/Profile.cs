using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Accounts.Models
{
    public class Profile
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Название программы")]
        [StringLength(500, ErrorMessage = "Максимальная длина 500 символов.")]
        public string Name { get; set; }

        [Display(Name = "Сокращенное название")]
        [StringLength(50, ErrorMessage = "Максимальная длина 50 символов.")]
        public string ShortName { get; set; }

        [Display(Name = "Шифр")]
        [StringLength(20, ErrorMessage = "Максимальная длина 20 символов.")]
        public string Code1 { get; set; }

        [Display(Name = "Шифр1")]
        [StringLength(20, ErrorMessage = "Максимальная длина 20 символов.")]
        public string Code2 { get; set; }

        [Display(Name = "Шифр2")]
        [StringLength(20, ErrorMessage = "Максимальная длина 20 символов.")]
        public string Code3 { get; set; }

        [Display(Name = "Период обучения")]
        public double? Period { get; set; }

        [Display(Name = "Направление обучения")]
        public int? DirectionOfTrainingId { get; set; }
        public DirectionOfTraining DirectionOfTraining { get; set; }
        
        [Display(Name = "Факультет")]
        public int? FacultyId { get; set; }
        public Faculty Faculty { get; set; }

        [Display(Name = "Учебный план")]
        public int? PlanId { get; set; }
        public Plan Plan { get; set; }

        [Display(Name = "Квалификация")]
        [StringLength(500, ErrorMessage = "Максимальная длина 500 символов.")]
        public string Qualification { get; set; }

        [Display(Name = "Кафедра")]
        public int? DepartmentId { get; set; }
        public Department Department { get; set; }

        [Display(Name = "Прием на специальность")]
        public bool Acceptance { get; set; }

        [Display(Name = "Заведующий направлением")]
        [StringLength(150, ErrorMessage = "Максимальная длина 150 символов.")]
        public string Boss { get; set; }

        [Display(Name = "Секретарь направления")]
        [StringLength(150, ErrorMessage = "Максимальная длина 150 символов.")]
        public string Secretary { get; set; }

        public int? DecanatID { get; set; }

        [Display(Name = "Удалена")]
        public bool IsDeleted { get; set; }

        public ICollection<Group> Groups { get; set; }
        public Profile()
        {
            Groups = new List<Group>();
        }

    }
}