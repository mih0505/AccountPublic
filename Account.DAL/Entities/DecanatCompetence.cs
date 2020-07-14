using System.ComponentModel;

namespace Account.DAL.Entities
{
    public class DecanatCompetence
    {
        /// <summary>
        /// Идентификатор компетенции
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор плана
        /// </summary>
        public int PlanId { get; set; }

        /// <summary>
        /// Идентификатор дисциплины
        /// </summary>
        public int DisciplineId { get; set; }

        /// <summary>
        /// Номер компетенции
        /// </summary>
        [DisplayName("Номер")]
        public int Number { get; set; }

        /// <summary>
        /// Шифр компетенции
        /// </summary>
        [DisplayName("Шифр")]
        public string Code { get; set; }

        /// <summary>
        /// Содержимое компетенции
        /// </summary>
        [DisplayName("Содержимое")]
        public string Content { get; set; }
    }
}
