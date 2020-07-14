using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Accounts.Models
{
    public class FormOfTraining
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Название формы обучения")]
        [StringLength(500, ErrorMessage = "Максимальная длина 500 символов.")]               
        public string Name { get; set; }

        public ICollection<Group> Groups { get; set; }
        public FormOfTraining()
        {
            Groups = new List<Group>();
        }
    }
}