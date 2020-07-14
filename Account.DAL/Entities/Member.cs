using Accounts.Models;
using System.ComponentModel;

namespace Account.DAL.Entities
{
    public enum MemberType : int
    {
        [Description("Составитель")]
        Compiler = 1,

        [Description("Рецензент")]
        Reviewer = 2,

        [Description("Заведующий кафедрой")]
        Manager = 3,

        [Description("Председатель НМСН")]
        Chairman = 4
    };

    public class Member
    {
        /// <summary>
        /// Идентификатор составителя или рецензента
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Фамилия составителя или рецензента РПД
        /// </summary>
        [DisplayName("Фамилия")]
        public string Lastname { get; set; }

        /// <summary>
        /// Имя составителя или рецензента РПД 
        /// </summary>
        [DisplayName("Имя")]
        public string Firstname { get; set; }

        /// <summary>
        /// Отчество составителя или рецензента РПД
        /// </summary>
        [DisplayName("Отчество")]
        public string Middlename { get; set; }

        /// <summary>
        /// Научная степень составителя или рецензента РПД
        /// </summary>
        [DisplayName("Степень")]
        public string AcademicDegree { get; set; }

        /// <summary>
        /// Должность составителя или рецензента РПД
        /// </summary>
        [DisplayName("Должность")]
        public string Position { get; set; }

        /// <summary>
        /// Идентификатор РПД
        /// </summary>
        public int RPDId { get; set; }

        /// <summary>
        /// Сущность РПД
        /// </summary>
        public RPD RPD { get; set; }

        /// <summary>
        /// Идентификатор составителя или рецензента РПД (если он есть в ЛК)
        /// </summary>
        public string TeacherId { get; set; }

        /// <summary>
        /// Сущность составителя или рецензента РПД (если он есть в ЛК)
        /// </summary>
        public ApplicationUser Teacher { get; set; }

        /// <summary>
        /// Тип участника разрабатывающего РПД
        /// </summary>
        public MemberType MemberType { get; set; }
    }
}
