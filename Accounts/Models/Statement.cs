using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Accounts.Models
{
    public class Statement
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Номер ведомости")]
        [StringLength(15, ErrorMessage = "Максимальная длина 15 символов.")]
        public string Number { get; set; }

        [Required]
        [Display(Name = "Дисциплина")]
        public string NameDiscipline { get; set; }
                
        [Display(Name = "Кафедра")]
        public int? DepartmentId { get; set; }
        public Department Department { get; set; }

        [Display(Name = "Преподаватель")]
        public string TeacherDisciplineId { get; set; }
        public virtual ApplicationUser TeacherDiscipline { get; set; }

        [Display(Name = "Преподаватель")]
        public string TeacherDiscipline2Id { get; set; }
        public virtual ApplicationUser TeacherDiscipline2 { get; set; }

        [Display(Name = "Преподаватель")]
        public string TeacherDiscipline3Id { get; set; }
        public virtual ApplicationUser TeacherDiscipline3 { get; set; }

        [Display(Name = "Преподаватель")]
        public string TeacherDiscipline4Id { get; set; }
        public virtual ApplicationUser TeacherDiscipline4 { get; set; }

        [Display(Name = "Преподаватель")]
        public string TeacherDiscipline5Id { get; set; }
        public virtual ApplicationUser TeacherDiscipline5 { get; set; }

        [Display(Name = "Преподаватель")]
        public string TeacherDiscipline6Id { get; set; }
        public virtual ApplicationUser TeacherDiscipline6 { get; set; }

        [Display(Name = "Преподаватель")]
        public string TeacherDiscipline7Id { get; set; }
        public virtual ApplicationUser TeacherDiscipline7 { get; set; }

        [Required]
        [Display(Name = "Вид контроля")]
        public string TypeControl { get; set; }
        
        [Required]
        [Display(Name = "Группа")]
        public int GroupId { get; set; }
        public Group Group { get; set; }
        public string GroupName { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int GroupIdDecanate { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int? ProfileId { get; set; }
        public Profile Profile { get; set; }

        [Display(Name = "Факультет")]
        public int? FacultyId { get; set; }
        public Faculty Faculty { get; set; }
        public string FacultyName { get; set; }

        [Required]
        [Display(Name = "Курс")]
        public int Course { get; set; }

        [Required]
        [Display(Name = "Семестр")]
        public int Semester { get; set; }
                
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата экзамена/зачета")]
        public DateTime? DateBegin { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата закрытия ведомости")]
        public DateTime DateEnd { get; set; }
        
        [HiddenInput(DisplayValue = false)]
        public string CreaterStatement { get; set; }        

        [HiddenInput(DisplayValue = false)]
        [Display(Name = "Учебный год")]
        public string CurrentYear { get; set; }
                
        [Display(Name = "ЗЕТ")]
        public int? ZET { get; set; }
                
        [Display(Name = "Количество часов")]
        public int? Hours { get; set; }

        public ICollection<StatementStudent> StatementStudents { get; set; }
        public Statement()
        {
            StatementStudents = new List<StatementStudent>();
        }

        [HiddenInput(DisplayValue = false)]
        public int? ParentId { get; set; }

        public bool AllTeachers { get; set; }
    }


    


}

