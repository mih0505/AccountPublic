using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Accounts.Models
{
    public class StatementIndividual
    {
        public int Id { get; set; }        

        [Required]
        [HiddenInput(DisplayValue = false)]        
        [StringLength(15, ErrorMessage = "Максимальная длина 15 символов.")]
        public string Number { get; set; }

        [Required]
        [HiddenInput(DisplayValue = false)]        
        public string NameDiscipline { get; set; }
                        
        [HiddenInput(DisplayValue = false)]
        public int? DepartmentId { get; set; }
        public Department Department { get; set; }

        [Display(Name = "Студент")]
        public string StudentId { get; set; }
        public virtual ApplicationUser Student { get; set; }

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
        [HiddenInput(DisplayValue = false)]
        public string TypeControl { get; set; }
        
        [Required]        
        [HiddenInput(DisplayValue = false)]
        public int GroupId { get; set; }
        public Group Group { get; set; }
        public string GroupName { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int GroupIdDecanate { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int? ProfileId { get; set; }
        public Profile Profile { get; set; }
                
        [HiddenInput(DisplayValue = false)]
        public int? FacultyId { get; set; }
        public Faculty Faculty { get; set; }

        [Required]
        [HiddenInput(DisplayValue = false)]        
        public int Course { get; set; }

        [Required]
        [HiddenInput(DisplayValue = false)]        
        public int Semester { get; set; }

        [HiddenInput(DisplayValue = false)]        
        public DateTime? DateBegin { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата закрытия ведомости")]
        public DateTime? DateEnd { get; set; }
        
        [HiddenInput(DisplayValue = false)]
        public string CreaterStatement { get; set; }        

        [HiddenInput(DisplayValue = false)]        
        public string CurrentYear { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int? ZET { get; set; }

        [HiddenInput(DisplayValue = false)]        
        public int? Hours { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int? ParentId { get; set; }

        public ICollection<StatementStudent> StatementStudents { get; set; }
        public StatementIndividual()
        {
            StatementStudents = new List<StatementStudent>();
        } 
    }
}

