using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace Accounts.Models
{
    public class DiplomWork
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("StudentDiplom")]
        public string StudentId { get; set; }
        public ApplicationUser StudentDiplom { get; set; }

        public string GIAId { get; set; }
        public GIA GIA { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Тема работы")]
        public string Topic { get; set; }
                
        [ForeignKey("TeacherDiplom")]
        [Display(Name = "Научный руководитель")]
        public string TeacherId { get; set; }
        public ApplicationUser TeacherDiplom { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string PathConducting { get; set; }
        [Display(Name = "Заявление на организацию и проведение защиты ВКР с применением ЭО и ДОТ")]
        public string FileNameConducting { get; set; }
        public bool ConductingIsBlocked { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string PathDiplom { get; set; }//путь к файлу с уникальным именем
        [Display(Name = "ВКР (в формате Word)")]
        public string FileNameDiplom { get; set; }//имя файла для отображения
        public bool DiplomIsBlocked { get; set; }//блокировка редактирования документа

        [HiddenInput(DisplayValue = false)]
        public string PathFeedback { get; set; }
        [Display(Name = "Отзыв руководителя")]
        public string FileNameFeedback { get; set; }
        public bool FeedbackIsBlocked { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string PathReview { get; set; }
        [Display(Name = "Рецензия на работу")]
        public string FileNameReview { get; set; }
        public bool ReviewIsBlocked { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string PathPlagiarism { get; set; }
        [Display(Name = "Отчет (справка) о проверке ВКР на объем заимствования и неправомочных заимствований")]
        public string FileNamePlagiarism { get; set; }
        public bool PlagiarismIsBlocked { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string PathApplication { get; set; }
        [Display(Name = "Приложение")]
        public string FileNameApplication { get; set; }
        public bool ApplicationIsBlocked { get; set; }
        
        [HiddenInput(DisplayValue = false)]
        public string PathConfirmation { get; set; }
        [Display(Name = "Заявление на утверждение темы и назначения руководителя ВКР")]
        public string FileNameConfirmation { get; set; }
        public bool ConfirmationIsBlocked { get; set; }
        
        [HiddenInput(DisplayValue = false)]
        public string PathDeclaration { get; set; }
        [Display(Name = "Заявление на изменение темы")]
        public string FileNameDeclaration { get; set; }
        public bool DeclarationIsBlocked { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string PathСonsent { get; set; }
        [Display(Name = "Согласие автора на передачу неисключительных прав на ВКР")]
        public string FileNameСonsent { get; set; }
        public bool СonsentIsBlocked { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string PathDiplomPDF { get; set; }
        [Display(Name = "ВКР (в формате PDF)")]
        public string FileNameDiplomPDF { get; set; }
        public bool DiplomIsBlockedPDF { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string PathOther { get; set; }
        [Display(Name = "Другие документы")]
        public string FileNameOther { get; set; }
        public bool OtherIsBlocked { get; set; }
    }
}