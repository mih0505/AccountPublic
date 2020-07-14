using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace Accounts.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Тип работы")]
        public string Type { get; set; }
                
        [Display(Name = "Кафедра")]
        public int? DepartmentId { get; set; }
        public Department Department { get; set; }

        [Required]
        //[Display(Name = "Группа")]
        public int GroupId { get; set; }
        public Group Group { get; set; }

        [Display(Name = "Группа")]
        public string GroupName { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int GroupIdDecanate { get; set; }

        [Display(Name = "Факультет")]
        public int FacultyId { get; set; }
        public Faculty Faculty { get; set; }
        
        [Required]
        [Display(Name = "Курс")]
        public int Cours { get; set; }

        [Required]
        [Display(Name = "Семестр")]
        public int Semester { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата начала работы")]
        public DateTime DateBegin { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата сдачи")]
        public DateTime DateEnd { get; set; }

        [Display(Name = "Создатель")]
        public string CourseWorkCreater { get; set; }        
        [ForeignKey("CourseWorkCreater")]
        public ApplicationUser User { get; set; }

        [Display(Name = "Учебный год")]
        public string CurrentYear { get; set; }

        [Display(Name = "Количество руководителей")]
        [Range(1, 5)]        
        public int CountTeachers { get; set; }

        public ICollection<CourseWorkStudent> CourseWorkStudents { get; set; }
        public Course()
        {
            CourseWorkStudents = new List<CourseWorkStudent>();
        }
    }    
}