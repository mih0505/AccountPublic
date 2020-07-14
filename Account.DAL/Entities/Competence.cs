namespace Account.DAL.Entities
{
    /// <summary>
    /// Описание компетенций в РПД
    /// </summary>
    public class Competence
    {
        /// <summary>
        /// Идентификатор компетенции
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Индентификатор компетенции в базе данных Деканата
        /// </summary>
        public int DecanatId { get; set; }

        /// <summary>
        /// Знание
        /// </summary>
        public string Knowledge { get; set; }

        public string KnowledgeName { get; set; }

        /// <summary>
        /// Умение
        /// </summary>
        public string Skill { get; set; }

        public string SkillName { get; set; }

        /// <summary>
        /// Владение
        /// </summary>
        public string Possession { get; set; }

        public string PossessionName { get; set; }

        /// <summary>
        /// Идентификатор рабочей программы.
        /// </summary>
        public int RPDId { get; set; }

        /// <summary>
        /// Сущность РПД
        /// </summary>
        public RPD RPD { get; set; }
    }
}
