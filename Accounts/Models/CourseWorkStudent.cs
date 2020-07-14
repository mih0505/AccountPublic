using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace Accounts.Models
{
    public class CourseWorkStudent
    {
        public int Id { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int GroupIdSite { get; set; }
        [HiddenInput(DisplayValue = false)]
        public int GroupIdDecanate { get; set; }
        [HiddenInput(DisplayValue = false)]
                
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = @"{0:dd\.MM\.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DateUpload { get; set; }//дата загрузки работы студентом

        [Required]
        [Display(Name = "Студент")]
        [ForeignKey("Student")]
        public string StudentId { get; set; }
        public ApplicationUser Student { get; set; }
                
        [Display(Name = "Преподаватель")]
        [ForeignKey("Teacher")]
        public string TeacherId { get; set; }
        public ApplicationUser Teacher { get; set; }

        [ForeignKey("Teacher2")]
        public string Teacher2Id { get; set; }
        public ApplicationUser Teacher2 { get; set; }

        [ForeignKey("Teacher3")]
        public string Teacher3Id { get; set; }
        public ApplicationUser Teacher3 { get; set; }

        [ForeignKey("Teacher4")]
        public string Teacher4Id { get; set; }
        public ApplicationUser Teacher4 { get; set; }

        [ForeignKey("Teacher5")]
        public string Teacher5Id { get; set; }
        public ApplicationUser Teacher5 { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int IdSudentDecanate { get; set; }

        [DataType(DataType.MultilineText)]
        [StringLength(1000, ErrorMessage = "Максимальная длина 1000 символов.")]
        [Display(Name = "Название работы")]
        public string Name { get; set; }

        [Display(Name = "Оценка")]
        public string Grade { get; set; }

        public int? GradeByNumber { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string Path { get; set; }//путь к файлу с уникальным именем
        [Display(Name = "Файл")]
        public string NameFile { get; set; }//имя файла для отображения

        public int? CourseId { get; set; }
        public Course Course { get; set; }

        [Display(Name = "Блокировать для студента")]
        public bool IsBlocked { get; set; }

        public ICollection<Review> Reviews { get; set; }
        public CourseWorkStudent()
        {
            Reviews = new List<Review>();
        }

    }    
}