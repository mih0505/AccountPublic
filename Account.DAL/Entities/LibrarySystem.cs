using System.ComponentModel;

namespace Account.DAL.Entities
{
    public class LibrarySystem
    {
        public int Id { get; set; }

        [DisplayName("Наименование документа с указанием реквизитов")]
        public string NameWithRequisites { get; set; }

        [DisplayName("Срок действия документа")]
        public string Validity { get; set; }

        public int RPDId { get; set; }

        public RPD RPD { get; set; }
    }
}
