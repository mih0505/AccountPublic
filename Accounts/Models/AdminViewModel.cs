using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Accounts.Models
{
    public class RoleViewModel
    {
        public string Id { get; set; }
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Наименование роли")]
        public string Name { get; set; }
    }

    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required]        
        [Display(Name = "Логин")]
        public string UserName { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [StringLength(100, ErrorMessage = "Максимальная длина 100 символов.")]
        [Display(Name = "Фамилия")]
        [Required]
        public string Lastname { get; set; }

        [StringLength(100, ErrorMessage = "Максимальная длина 100 символов.")]
        [Display(Name = "Имя")]
        [Required]
        public string Firstname { get; set; }

        [StringLength(100, ErrorMessage = "Максимальная длина 100 символов.")]
        [Display(Name = "Отчество")]
        public string Middlename { get; set; }

        [Display(Name = "Возраст")]
        public int Age { get; set; }

        [Display(Name = "Дата рождения")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Факультет")]
        public int? FacultyId { get; set; }
        public Faculty Faculty { get; set; }

        [Display(Name = "Кафедра")]
        public int? DepartmentId { get; set; }
        public Department Department { get; set; }

        public int? idGroupDecanat { get; set; }

        public int? idProfileDecanat { get; set; }

        public int? MoodleId { get; set; }//id студента в moodle

        public int? DecanatId { get; set; }//id студента в деканате

        [Display(Name = "Учебный план")]
        public int? PlanId { get; set; }//id 
        public Plan Plan { get; set; }

        [Display(Name = "Группа")]
        public int? GroupId { get; set; }//id группы студента
        public Group Group { get; set; }

        [Display(Name = "Изображение пользователя")]
        public byte[] Image { get; set; }//аватарка

        [Display(Name = "Работодатель")]
        public bool Employer { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата блокировки")]
        public DateTime? DateBlocked { get; set; }//дата блокировки
        
        [Display(Name = "Причина блокировки")]
        public string BlockingReason { get; set; }//причина блокировки


        public IEnumerable<SelectListItem> RolesList { get; set; }
    }
}