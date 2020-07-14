using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Accounts.Models
{
    public class Study
    {
        public int Id { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int LessonId { get; set; }
        public Lesson Lesson { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int? JournalId { get; set; }
        public Journal Journal { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string StudentId { get; set; }
        public ApplicationUser Student { get; set; }
                
        [Display(Name = "Оценка")]
        [StringLength(15, ErrorMessage = "Максимальная длина 15 символа.")]
        public string Grade1 { get; set; }
                
        [Display(Name = "Доп. оценка")]
        [StringLength(15, ErrorMessage = "Максимальная длина 15 символа.")]
        public string Grade2 { get; set; }
                
        [Display(Name = "Доп. оценка2")]
        [StringLength(15, ErrorMessage = "Максимальная длина 15 символа.")]
        public string Grade3 { get; set; }
    }
}