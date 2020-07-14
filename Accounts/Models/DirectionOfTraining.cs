using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Accounts.Models
{
    public class DirectionOfTraining
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Название направления")]
        [StringLength(200, ErrorMessage = "Максимальная длина 200 символов.")]
        public string Name { get; set; }

        public ICollection<Profile> Profiles { get; set; }

        public int? DecanatID { get; set; }

        public DirectionOfTraining()
        {
            Profiles = new List<Profile>();
        }
    }
}