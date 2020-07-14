using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.Models
{
    public class AcademicDegree
    {
        public int Id { get; set; }

        [Display(Name = "Преподаватель")]
        public string AcademicDegreeUserId { get; set; }
        [ForeignKey("AcademicDegreeUserId")]
        public ApplicationUser User { get; set; }

        [Display(Name = "Год получения")]
        [StringLength(4, ErrorMessage = "Максимальная длина 4 символа.")]
        public string Year { get; set; }

        [Display(Name = "Наименование степени/звания")]
        public string Name { get; set; }

        [Display(Name = "Реквизиты документа")]
        public string Requisites { get; set; }
                
        public string Path { get; set; }

        [Display(Name = "Файл")]
        public string FileName { get; set; }
    }
}