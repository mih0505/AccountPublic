using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Accounts.Models
{
    public class FacultyStatisticViewModel
    {
        [Display(Name = "Факультет")]
        public int FacultyId { get; set; }

        [Display(Name = "Учебный год")]
        public string Year { get; set; }

        [Display(Name = "Форма обучения")]
        public int FormOfTrainingId { get; set; }

        [Display(Name = "Сессия")]
        public string Term { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Отчет на")]
        public DateTime Date { get; set; }
    }
}