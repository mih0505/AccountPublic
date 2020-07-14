using System.ComponentModel;

namespace Account.DAL.Entities
{
    public class License
    {
        public int Id { get; set; }

        [DisplayName("Наименование программного обеспечения")]
        public string ProgramName { get; set; }

        public int RPDId { get; set; }

        public RPD RPD { get; set; }
    }
}
