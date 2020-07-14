using System.ComponentModel;

namespace Account.DAL.Entities
{
    public class DecanatDepartment
    {
        /// <summary>
        /// Идентификатор кафедры
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор факультета
        /// </summary>
        public int FacultyId { get; set; }

        /// <summary>
        /// Название факультета
        /// </summary>
        [DisplayName("Факультет")]
        public string FacultyTitle { get; set; }

        /// <summary>
        /// Название кафедры
        /// </summary>
        [DisplayName("Кафедра")]
        public string Title { get; set; }

        /// <summary>
        /// Заведующий кафедрой
        /// </summary>
        [DisplayName("Заведующий кафедрой")]
        public string Chief { get; set; }
    }
}
