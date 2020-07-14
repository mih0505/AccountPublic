using System.ComponentModel;

namespace Account.DAL.Entities
{
    public class InternetResource
    {
        public int Id { get; set; }

        [DisplayName("Адрес (URL)")]
        public string URL { get; set; }

        [DisplayName("Описание страницы")]
        public string Description { get; set; }

        public int RPDId { get; set; }

        public RPD RPD { get; set; }
    }
}
