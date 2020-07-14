using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.Models
{
    public class BasicEducation
    {
        public int Id { get; set; }

        [Display(Name = "Преподаватель")]
        public string BasicEducationUserId { get; set; }
        [ForeignKey("BasicEducationUserId")]
        public ApplicationUser User { get; set; }

        [Display(Name = "Образовательное учреждение")]
        public string EducationalInstitution { get; set; }

        [Display(Name = "Год получения")]
        [StringLength(4, ErrorMessage = "Максимальная длина 4 символа.")]
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