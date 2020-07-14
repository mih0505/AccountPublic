namespace Account.DAL.Entities
{
    public class RPDContent
    {
        /// <summary>
        /// Идентификатор содержимого
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор пункта
        /// </summary>
        public int RPDItemId { get; set; }

        /// <summary>
        /// Сущность пункта
        /// </summary>
        public RPDItem RPDItem { get; set; }

        /// <summary>
        /// Содержимое
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Название файла
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Прикрепленный файл
        /// </summary>
        public byte[] FileContent { get; set; }

        /// <summary>
        /// Идентификатор РПД
        /// </summary>
        public int RPDId { get; set; }

        /// <summary>
        /// Сущность РПД
        /// </summary>
        public RPD RPD { get; set; }
    }
}
