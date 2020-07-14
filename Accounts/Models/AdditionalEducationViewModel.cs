using System.ComponentModel.DataAnnotations;

namespace Accounts.Models
{
    public class AdditionalEducationViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Кафедра")]
        public string Department { get; set; }

        [Display(Name = "Преподаватель")]
        public string Teacher { get; set; }
        public string Type { get; set; }

        [Display(Name = "Год получения")]
        public string Year { get; set; }

        [Display(Name = "Наименование")]
        public string Name { get; set; }

        [Display(Name = "Часы")]
        public string Hours { get; set; }

        [Display(Name = "Место")]
        public string Location { get; set; }
        public string Path { get; set; }

        [Display(Name = "Файл")]
        public string FileName { get; set; }
        public string UserId { get; internal set; }
    }
}