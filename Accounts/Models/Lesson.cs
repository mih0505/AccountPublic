using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Accounts.Models
{
    public class Lesson
    {
        public int Id { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int JournalId { get; set; }
        public Journal Journal { get; set; }
                
        [Display(Name = "Номер")]
        public int? Number { get; set; }

        [Required]
        [Display(Name = "Дата")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        [Required]
        [Display(Name = "Вид занятия")]
        public string TypeLesson { get; set; }

        [DataType(DataType.MultilineText)]
        [StringLength(2000, ErrorMessage = "Максимальная длина 2000 символов.")]
        [Display(Name = "Тема")]
        public string Topic { get; set; }

        [DataType(DataType.MultilineText)]
        [StringLength(2000, ErrorMessage = "Максимальная длина 2000 символов.")]
        [Display(Name = "Домашняя работа")]
        public string HomeWork { get; set; }

        [DataType(DataType.MultilineText)]
        [StringLength(2000, ErrorMessage = "Максимальная длина 2000 символов.")]
        [Display(Name = "Примечание")]
        public string Note { get; set; }
    }
}