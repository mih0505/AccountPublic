using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Account.DAL.Entities
{
    public class ThematicPlan
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Идентификатор рабочей программы.
        /// </summary>
        public int RpdId { get; set; }

        /// <summary>
        /// Сущность рабочей программы
        /// </summary>
        public RPD RPD { get; set; }

        /// <summary>
        /// Порядковый номер строки в таблице.
        /// </summary>
        [DisplayName("№ п/п")]
        public string Number { get; set; }

        /// <summary>
        /// Тема/раздел.
        /// </summary>
        [DisplayName("Наименование раздела/темы дисциплины")]
        public string Topic { get; set; }

        /// <summary>
        /// Количество часов выделяемое на лекции.
        /// </summary>
        [DisplayName("Лекции")]
        public double Lecture { get; set; }

        /// <summary>
        /// Количество часов выделяемое на практическую работу.
        /// </summary>
        [DisplayName("Семинары/практики")]
        public double Practice { get; set; }

        /// <summary>
        /// Количество часов выделяемое на лабораторные работы.
        /// </summary>
        [DisplayName("Лабораторные")]
        public double Lab { get; set; }

        /// <summary>
        /// Количество часов выделяемое на самостоятельную работу.
        /// </summary>
        [DisplayName("СРС")]
        public double IndividualWork { get; set; }

        /// <summary>
        /// Раздел или тема?
        /// </summary>
        public bool IsSection { get; set; }

        public IEnumerable<ThematicContent> ThematicContents { get; set; }

        public ThematicPlan()
        {
            ThematicContents = new List<ThematicContent>();
        }
    }
}
