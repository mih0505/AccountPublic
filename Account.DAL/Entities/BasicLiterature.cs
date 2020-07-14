using System.ComponentModel;

namespace Account.DAL.Entities
{
    public class BasicLiterature
    {
        public int Id { get; set; }

        [DisplayName("Наименование основной учебной литературы")]
        public string Title { get; set; }

        public int RPDId { get; set; }

        public RPD RPD { get; set; }
    }
}
