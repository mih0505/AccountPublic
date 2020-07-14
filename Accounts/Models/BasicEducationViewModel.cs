using System.ComponentModel.DataAnnotations;

namespace Accounts.Models
{
    public class BasicEducationViewModel
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        [Display(Name = "Кафедра")]
        public string Department { get; set; }

        [Display(Name = "Преподаватель")]
        public string Teacher { get; set; }

        [Display(Name = "Образовательное учреждение")]
        public string EducationalInstitution { get; set; }

        [Display(Name = "Год получения")]
        public string Year { get; set; }

        [Display(Name = "Специальность")]
        public string Specialty { get; set; }

        [Display(Name = "Квалификация")]
        public string Qualification { get; set; }

        public string Path { get; set; }

        [Display(Name = "Файл")]
        public string FileName { get; set; }
    }
}