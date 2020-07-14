using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Accounts.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Адрес электронной почты")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Код")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Запомнить браузер?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Адрес электронной почты")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Логин/Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Display(Name = "Запомнить меня")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [StringLength(100, ErrorMessage = "Максимальная длина 100 символов.")]
        [Display(Name = "Фамилия")]
        [Required]
        public string Lastname { get; set; }

        [StringLength(100, ErrorMessage = "Максимальная длина 100 символов.")]
        [Display(Name = "Имя")]
        [Required]
        public string Firstname { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Максимальная длина 100 символов.")]
        [Display(Name = "Отчество")]
        public string Middlename { get; set; }

        [Display(Name = "Факультет")]
        public int? FacultyId { get; set; }
        public Faculty Faculty { get; set; }

        [Display(Name = "Кафедра")]
        public int? DepartmentId { get; set; }
        public Department Department { get; set; }

        public int? DecanatId { get; set; }//id студента в деканате

        [Required]
        [EmailAddress]
        [Display(Name = "Адрес электронной почты")]
        public string Email { get; set; }

        [Required]        
        [Display(Name = "Логин")]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Длина {0} должна быть не менее {2} символов.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        [Compare("Password", ErrorMessage = "Пароль и его подтверждение не совпадают.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Работодатель")]
        public bool Employer { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Адрес электронной почты")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Длина {0} должна быть не менее {2} символов.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        [Compare("Password", ErrorMessage = "Пароль и его подтверждение не совпадают.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        public string Id { get; set; }

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
        [Required]
        public string Middlename { get; set; }

        [StringLength(15, ErrorMessage = "Максимальная длина 15 символов.")]
        [Display(Name = "Номер студенческого / зачет. кн.")]        
        public string NumberOfRecordBook { get; set; }

        [Display(Name = "Секретный вопрос")]
        public string Question { get; set; }

        [StringLength(100, ErrorMessage = "Максимальная длина 50 символов.")]
        [Display(Name = "Ответ на секретный вопрос")]
        public string Answer { get; set; }
                                
        [Display(Name = "Группа")]
        public int? GroupId { get; set; }
        public Group Group { get; set; }

        [EmailAddress]
        [Display(Name = "Почта")]
        public string Email { get; set; }
    }

    public class SecurityQuestionViewModel
    {        
        [Required]
        [DataType(DataType.MultilineText)]
        [StringLength(200, ErrorMessage = "Максимальная длина 200 символов.")]
        [Display(Name = "Секретный вопрос")]
        public string Question { get; set; }

                
        [StringLength(100, ErrorMessage = "Максимальная длина 100 символов.")]
        [Display(Name = "Ответ")]
        public string Answer { get; set; }
    }
}
