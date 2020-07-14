using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Accounts.Models
{
    public class Discipline
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Факультет")]
        public int? FacultyId { get; set; }
        public Faculty Faculty { get; set; }

        [Required]
        [Display(Name = "Кафедра")]        
        public int? DepartmentId { get; set; }
        public Department Department { get; set; }

        [Required]
        [Display(Name = "Название дисциплины")]
        [StringLength(50, ErrorMessage = "Максимальная длина 200 символов.")]
        public string Name { get; set; }

        [Display(Name = "Количество часов")]
        public int? Hourse { get; set; }

        [Display(Name = "Количество ЗЕТ")]
        public int? ZET { get; set; }

        [Display(Name = "Курс")]
        public int? YearOfStudy { get; set; }

        [Display(Name = "Семестр")]
        public int? Semester { get; set; }

        [Display(Name = "Учебный год")]
        [StringLength(50, ErrorMessage = "Максимальная длина 50 символов.")]
        public string AcademicYear { get; set; }
        
        [Display(Name = "Форма отчетности по дисциплине")]
        [StringLength(50, ErrorMessage = "Максимальная длина 50 символов.")]
        public string FormReporting { get; set; }

        [Display(Name = "Программа (профиль, направленность)")]
        public int? ProfileId { get; set; }
        public Profile Profile { get; set; }

        [Display(Name = "Принимаете ли Вы экзамен (зачет) по дисциплине?")]
        public bool TypeTeacherDiscipline { get; set; }//тип преподавателя: true-принимает экзамен (зачет), false-не принимает


        [Display(Name = "URL-адрес к электронному (дист.) курсу")]
        [StringLength(50, ErrorMessage = "Максимальная длина 50 символов.")]
        [Url]
        public string MoodleURL { get; set; }

        [Display(Name = "Название электронного курса в Moodle")]
        [StringLength(50, ErrorMessage = "Максимальная длина 200 символов.")]
        public string MoodleNameCourse { get; set; }

        [Display(Name = "Вы являетесь автором этого электронного курса в Moodle?")]
        public bool TypeTeacherMoodle { get; set; }//тип преподавателя: true-автор дист. курса, false-ассистент
        

        public string AuthorId { get; set; }//id автор
        [ForeignKey("AuthorId")]
        public ApplicationUser User { get; set; }

        [Display(Name = "Группы")]
        public ICollection<Group> Groups { get; set; }
        public Discipline()
        {
            Groups = new List<Group>();
        }


    }
}