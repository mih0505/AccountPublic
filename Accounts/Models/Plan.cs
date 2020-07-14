using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Accounts.Models
{
    public class Plan
    {
        public int Id { get; set; }
        [Display(Name = "Блок")]
        public string Block { get; set; }
        [Display(Name = "Дисциплина")]
        public string Name { get; set; }
        [Display(Name = "Всего часов")]
        public int AllHours { get; set; }//всего часов
        [Display(Name = "Сам. раб.")]
        public int IndependentWork { get; set; }//самостоятельная работа
        [Display(Name = "Курс")]
        public int Course { get; set; }
        [Display(Name = "Семестр")]
        public int Session { get; set; }
        [Display(Name = "Контроль")]
        public string Controls { get; set; }
        [Display(Name = "Лекций")]
        public int Lecture { get; set; }
        [Display(Name = "Практик")]
        public int PracticalWork { get; set; }
        [Display(Name = "Лаб. раб.")]
        public int LaboratoryWork { get; set; }
        [Display(Name = "Блок")]
        public int CIW { get; set; }
        [Display(Name = "Инд. раб.")]
        public int IndividualLessons { get; set; }
        [Display(Name = "Компетенции")]
        public string Competences { get; set; }

    }
}