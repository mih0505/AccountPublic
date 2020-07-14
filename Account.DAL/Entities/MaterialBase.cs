using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Account.DAL.Entities
{
    public class MaterialBase
    {
        public int Id { get; set; }

        [DisplayName("Тип учебной аудитории")]
        public string Type { get; set; }

        [DisplayName("Оснащенность учебной аудитории")]
        public string Equipment { get; set; }

        public int RPDId { get; set; }

        public RPD RPD { get; set; }
    }
}
