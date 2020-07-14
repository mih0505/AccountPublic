using System.Collections.Generic;

namespace Account.DAL.Entities
{
    public class RPDItem
    {
        /// <summary>
        /// Идентификатор пункта
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор образовательного стандарта
        /// </summary>
        public int EducationStandardId { get; set; }

        /// <summary>
        /// Сущность образовательного стандарта
        /// </summary>
        public EducationStandard EducationStandard { get; set; }

        /// <summary>
        /// Номер пункта
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Наименование пункта
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Идентификатор родительского пункта в содержании РПД
        /// </summary>
        public int? ParentItemId { get; set; }

        /// <summary>
        /// Сущность родительского пункта в содержании РПД
        /// </summary>
        public RPDItem ParentItem { get; set; }

        /// <summary>
        /// Имя элемента управления, который хранит в себе содержимое РПД
        /// </summary>
        public string ControlName { get; set; }

        /// <summary>
        /// Шаблонный текст пункта, для вставки в программу
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Список сущностей содержимого
        /// </summary>
        public IEnumerable<RPDContent> RPDContents { get; set; }

        public int DocumentTypeId { get; set; }

        public DocumentType DocumentType { get; set; }

        public RPDItem()
        {
            RPDContents = new List<RPDContent>();
        }
    }
}
