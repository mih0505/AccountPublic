using System.Collections.Generic;
using System.ComponentModel;

namespace Account.DAL.Entities
{
    public class DecanatDiscipline
    {
        [Browsable(false)]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор кафедры
        /// </summary>
        [Browsable(false)]
        public int DepartmentId { get; set; }

        /// <summary>
        /// Название дисциплины
        /// </summary>
        [DisplayName("Название дисциплины")]
        public string Title { get; set; }

        /// <summary>
        /// Тип дисциплины, необходим для фильтрации в планах
        /// </summary>
        [Browsable(false)]
        public int TypeObject { get; set; }
    }
}
