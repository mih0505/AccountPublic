using System.Collections.Generic;

namespace Account.DAL.Entities
{
    public class EducationStandard
    {
        /// <summary>
        /// Идентификатор образовательного стандарта
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название образовательного стандарта
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Скрыть образовательный стандарт
        /// </summary>
        public bool IsHide { get; set; }

        /// <summary>
        /// Список сущностей пунктов
        /// </summary>
        public IEnumerable<RPDItem> RPDItems { get; set; }

        public EducationStandard()
        {
            RPDItems = new List<RPDItem>();
        }
    }
}
