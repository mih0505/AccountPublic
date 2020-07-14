using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Accounts.Models
{
    public class Section
    {
        public int Id { get; set; }

        [StringLength(200, ErrorMessage = "Максимальная длина 200 символов.")]
        [Display(Name = "Название раздела")]
        public string Name { get; set; }

        [StringLength(200, ErrorMessage = "Максимальная длина 200 символов.")]
        [Display(Name = "Псевдоним")]
        public string Alias { get; set; }

        [Display(Name = "Описание разделов")]
        public string Description { get; set; }
                
        public ICollection<Catigory> Catigories { get; set; }
        public Section()
        {
            Catigories = new List<Catigory>();
        }
    }
}
