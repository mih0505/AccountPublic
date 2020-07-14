using System.ComponentModel;

namespace Account.DAL.Entities
{
    public class DecanatPlan
    {
        /// <summary>
        /// Идентификатор плана
        /// </summary>
        [Browsable(false)]
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор кафедры
        /// </summary>
        [Browsable(false)]
        public int DepartmentId { get; set; }

        /// <summary>
        /// Идентификатор дисциплины
        /// </summary>
        [Browsable(false)]
        public int DisciplineId { get; set; }

        /// <summary>
        /// Имя файла
        /// </summary>
        [DisplayName("Имя файла")]
        public string Filename { get; set; }

        /// <summary>
        /// Идентификатор ФГОС'a ("3,5" - ФГОС 3+, "3,75" - ФГОС 3++)
        /// </summary>
        [Browsable(false)]
        public string FGOSId { get; set; }

        /// <summary>
        /// Тип ФГОС'a (например: ФГОС 3+, ФГОС 3++)
        /// </summary>
        [DisplayName("Тип ФГОС'a")]
        public string FGOS { get; set; }

        /// <summary>
        /// Идентификатор формы обучения
        /// </summary>
        [Browsable(false)]
        public int EducationFormId { get; set; }

        /// <summary>
        /// Форма обучения
        /// </summary>
        [DisplayName("Форма обучения")]
        public string EducationForm { get; set; }

        /// <summary>
        /// Квалификация
        /// </summary>
        [DisplayName("Квалификация")]
        public string Qualification { get; set; }

        /// <summary>
        /// Блок
        /// </summary>
        [DisplayName("Блок")]
        public string Block { get; set; }

        /// <summary>
        /// Идентификатор направления
        /// </summary>
        [Browsable(false)]
        public int ProfileId { get; set; }

        /// <summary>
        /// Шифр направления
        /// </summary>
        [DisplayName("Шифр направления")]
        public string ProfileCode { get; set; }

        /// <summary>
        /// Направление обучения
        /// </summary>
        [DisplayName("Направление обучения")]
        public string Profile { get; set; }

        /// <summary>
        /// Реквизиты ОП
        /// </summary>
        [DisplayName("Реквизиты ОП")]
        [Browsable(false)]
        public string ProgramRequisites { get; set; }

        /// <summary>
        /// Учебный год
        /// </summary>
        [DisplayName("Учебный год")]
        [Browsable(false)]
        public string StudyYear { get; set; }
    }
}
