using System.ComponentModel.DataAnnotations;

namespace Accounts.Models
{
    public class Load
    {
        public int TeacherId { get; set; }

        public int LoadIdDecanat { get; set; }

        public int? DisciplineId { get; set; }

        [Display(Name = "Дисциплина")]
        public string Discipline { get; set; }

        [Display(Name = "Кафедра")]
        public string Department { get; set; }

        public int GroupId { get; set; }

        [Display(Name = "Группа")]
        public string Group { get; set; }

        [Display(Name = "Курс")]
        public string Course { get; set; }

        [Display(Name = "Сем.")]
        public string Semester { get; set; }

        [Display(Name = "Студ.")]
        public string Students { get; set; }

        [Display(Name = "Вид занятия")]
        public string TypeLesson { get; set; }

        [Display(Name = "Контроль")]
        public string TypeControl { get; set; }

        [Display(Name = "Конт. раб.")]
        public string Control { get; set; }

        [Display(Name = "Ауд. нагрузка")]
        public string LoadClass { get; set; }

        [Display(Name = "Др. нагрузка")]
        public string LoadOther { get; set; }

        [Display(Name = "Описание часов")]
        public string Note { get; set; }
    }
}