using System.ComponentModel;

namespace Account.DAL.Entities
{
    public class CompetenceGrade
    {
        public int Id { get; set; }

        public int DecanatId { get; set; }

        [DisplayName("Этап")]
        public string Stage { get; set; }

        [DisplayName("Неудовлетворительно")]
        public string BadGrade { get; set; }

        [DisplayName("Удовлетворительно")]
        public string TernGrade { get; set; }

        [DisplayName("Хорошо")]
        public string WellGrade { get; set; }

        [DisplayName("Отлично")]
        public string PerfectGrade { get; set; }

        [DisplayName("Вид оценочного средства")]
        public string ValuationType { get; set; }

        public RPD RPD { get; set; }

        public int RPDId { get; set; }
    }
}
