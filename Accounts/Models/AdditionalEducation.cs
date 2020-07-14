using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.Models
{
    public class AdditionalEducation
    {
        public int Id { get; set; }

        [Display(Name = "Преподаватель")]
        public string AdditionalEducationUserId { get; set; }
        [ForeignKey("AdditionalEducationUserId")]
        public ApplicationUser User { get; set; }

        [Display(Name = "Год получения")]
        [StringLength(4, ErrorMessage = "Максимальная длина 4 символа.")]
        public string Year { get; set; }

        [Display(Name = "Вид образования")]
        public string Type { get; set; }

        [Display(Name = "Наименование")]
        public string Name { get; set; }

        [Display(Name = "Количество часов")]
        [StringLength(4, ErrorMessage = "Максимальная длина 4 символа.")]
        public string Hours { get; set; }

        [Display(Name = "Место прохождения")]
        public string Location { get; set; }

        public string Path { get; set; }

        [Display(Name = "Файл")]
        public string FileName { get; set; }
    }
}