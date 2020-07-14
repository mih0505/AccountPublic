using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Accounts.Models
{
    public class Declaration
    {
        public int Id { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        
        public string Path { get; set; }//путь к файлу с уникальным именем
        [Display(Name = "Файл")]
        public string NameFile { get; set; }

        [HiddenInput(DisplayValue = false)]
        public DateTime DateAdd { get; set; }
    }
}