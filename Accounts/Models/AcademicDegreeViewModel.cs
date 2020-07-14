using System.ComponentModel.DataAnnotations;

namespace Accounts.Models
{
    public class AcademicDegreeViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Кафедра")]
        public string Department { get; set; }

        [Display(Name = "Преподаватель")]
        public string Teacher { get; set; }

        [Display(Name = "Наименование")]
        public string Name { get; set; }

        [Display(Name = "Год получения")]
        public string Year { get; set; }

        [Display(Name = "Реквизиты")]
        public object Requisites { get; set; }
        public string Path { get; set; }

        [Display(Name = "Файл")]
        public string FileName { get; set; }
        public string UserId { get; set; }
    }
}