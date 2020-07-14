using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Accounts.Models
{
    public class GIA
    {
        [Key]
        [HiddenInput]
        public string Id { get; set; }

        [Display(Name = "Факультет")]
        [Required]
        public int FacultyId { get; set; }
        public Faculty Faculty { get; set; }

        [Display(Name = "Группа")]
        [Required]
        public int GroupId { get; set; }
        public Group Group { get; set; }
                
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата сдачи")]
        public DateTime DateEnd { get; set; }

        [Display(Name = "Блокировать")]
        public bool IsBlocked { get; set; }

        [Display(Name = "Путь к видео")]
        public string PathVideo { get; set; }

        [HiddenInput]
        public string CurrentYear { get; set; }

        [Display(Name = "Ссылка на видеоконференцию")]
        [Url]
        public string Link { get; set; }

        public ICollection<DiplomWork> Diploms { get; set; }
        

        public GIA()
        {            
            Diploms = new List<DiplomWork>();
        }
    }
}