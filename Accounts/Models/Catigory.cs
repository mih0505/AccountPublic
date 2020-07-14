using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Accounts.Models
{
    public class Catigory
    {
        public int Id { get; set; }

        [StringLength(200, ErrorMessage = "Максимальная длина 200 символов.")]
        [Display(Name = "Название категории")]
        public string Name { get; set; }
        public int IndexSort { get; set; }

        [Display(Name = "Раздел")]
        public int SectionId { get; set; }
        public Section Section { get; set; }

        public ICollection<Artifact> Artifacts { get; set; }
        public Catigory()
        {
            Artifacts = new List<Artifact>();
        }
    }
}
