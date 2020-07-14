using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace Accounts.Models
{
    public class StatementStudent
    {
        public int Id { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int GroupIdSite { get; set; }
        [HiddenInput(DisplayValue = false)]
        public int GroupIdDecanate { get; set; }
                
        [Display(Name = "Дата")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = @"{0:dd\.MM\.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? Date { get; set; }//выставления оценки

        [Required]
        [Display(Name = "Студент")]
        [ForeignKey("StudentStatement")]
        public string StudentStatementId { get; set; }
        public ApplicationUser StudentStatement { get; set; }

        [Display(Name = "Преподаватель")]
        [ForeignKey("TeacherStatement")]
        public string TeacherStatementId { get; set; }
        public ApplicationUser TeacherStatement { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int IdStudentDecanate { get; set; }
                
        [Display(Name = "Баллы за семестр")]
        public int PointSemester { get; set; }

        [Display(Name = "Доп. баллы")]
        public int PointAdvanced { get; set; }

        [Display(Name = "Баллы за экзамен")]
        public int PointControl { get; set; }

        [Display(Name = "Итого баллов")]
        public int TotalPoint { get; set; }

        [Display(Name = "Оценка")]
        public string Grade { get; set; }
        public int GradeByNumber { get; set; }

        public int? StatementId { get; set; }
        public Statement Statement { get; set; }

        [Display(Name = "Блокировать студента")]
        public bool IsBlocked { get; set; }

        public string CurrentNameGroup { get; set; }

        [Display(Name = "Дата выдачи")]
        public DateTime? DateBegin { get; set; }

        [Display(Name = "Действительна до")]
        public DateTime? DateEnd { get; set; }
    }
}