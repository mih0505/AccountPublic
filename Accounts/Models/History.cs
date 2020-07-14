using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Accounts.Models
{
    public class History
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Студент")]
        [ForeignKey("StudentHystory")]
        public string StudentId { get; set; }
        public ApplicationUser StudentHystory { get; set; }
        public int? ProfileDecanateId { get; set; }
        public int? ProfileId { get; set; }
        public Profile Profile { get; set; }
        public string BaseLearning { get; set; }
        public string GroupFrom { get; set; }
        public string GroupIn { get; set; }
        public int? YearStartLearning { get; set; }
        public string Name{ get; set; }
        public DateTime DateMove { get; set; }
        public string NumberDocument { get; set; }
        public bool Debt { get; set; }//долг

        [Required]
        [Display(Name = "Сотрудник")]
        [ForeignKey("Worker")]
        public string WorkerId { get; set; }
        public ApplicationUser Worker { get; set; }
        public string PC { get; set; }
        public DateTime DateAddRecord { get; set; }
        public string Year { get; set; }
        public int? TypeOrderDecanateId { get; set; }//столбец кодТипа
        public int? TypeOrderId { get; set; }
        public TypeOrder TypeOrder { get; set; }
        public int GroupInDecanateId { get; set; }
        public int GroupFromDecanateId { get; set; }
        public int PastStatus { get; set; }
        public int OrderDecanateId { get; set; }//столбец КодПриказа
        public int FacultyFromId { get; set; }
        public Faculty FacultyFrom { get; set; }
        public int FacultyFromDecanateId { get; set; }
        public int FacultyInId { get; set; }
        public Faculty FacultyIn { get; set; }
        public int FacultyInDecanateId { get; set; }
        public bool Hidden { get; set; }
        public string Note { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public int? TypeMoveDecanateId { get; set; }
        public int? TypeMoveId { get; set; }        
        public TypeMove TypeMove { get; set; }
        public int CourseBefore { get; set; }
        public int CourseAfter { get; set; } 
        

    }
}