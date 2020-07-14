using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace Accounts.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Тип")]
        public string Type { get; set; }

        [Required]
        [Display(Name = "Автор")]
        public string Author { get; set; }

        [HiddenInput(DisplayValue = false)]
        public DateTime DateUpload { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string Path { get; set; }//путь к файлу с уникальным именем
        [Display(Name = "Файл")]
        public string NameFile { get; set; }//имя файла для отображения

        public CourseWorkStudent CourseWorkStudent { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }
}