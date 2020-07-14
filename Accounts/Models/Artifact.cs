using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace Accounts.Models
{
    public class Artifact
    {
        public int Id { get; set; }

        [HiddenInput(DisplayValue = false)]        
        public int CatigoryId { get; set; }
        public Catigory Catigory { get; set; }

        public string UserId { get; set; }//id автора достижения
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        public DateTime DateAdd { get; set; }//добавление артефакта в портфолио

        [Required]
        [StringLength(500, ErrorMessage = "Максимальная длина 500 символов.")]
        [Display(Name = "Название")]        
        public string Name { get; set; }//название артефакта

        [StringLength(500, ErrorMessage = "Максимальная длина 500 символов.")]
        [Display(Name = "Авторы")]
        public string Authors { get; set; }//авторы

        [StringLength(500, ErrorMessage = "Максимальная длина 500 символов.")]
        [Display(Name = "Название сборника, книги и т.п.")]
        public string BookTitle { get; set; }//Название сборника, книги и т.п.

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата")]        
        public DateTime DateBegin { get; set; }//дата начала

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата окончания")]
        public DateTime? DateEnd { get; set; }//дата окончания

        [StringLength(500, ErrorMessage = "Максимальная длина 500 символов.")]
        [Display(Name = "Организация проводившая мероприятие")]
        public string Organization { get; set; }//название организации проводившей мероприятие

        [StringLength(500, ErrorMessage = "Максимальная длина 500 символов.")]
        [Display(Name = "Место проведения")]
        [Required]
        public string Location { get; set; }//место проведения мероприятия

        [DataType(DataType.MultilineText)]
        [StringLength(1000, ErrorMessage = "Максимальная длина 1000 символов.")]
        [Display(Name = "Дополнительная информация")]
        public string AdditionalInformation { get; set; }//доп. информация

        [StringLength(50, ErrorMessage = "Максимальная длина 50 символов.")]
        [Display(Name = "Баллы")]
        public string Points { get; set; }//баллы набранные на мероприятии

        [StringLength(500, ErrorMessage = "Максимальная длина 500 символов.")]
        [Display(Name = "Ссылка")]
        [Url]
        public string Link { get; set; }//ссылка на внешний ресурс с подтверждением 
                
        public string Path { get; set; }//путь к файлу с уникальным именем
        [Display(Name = "Файл")]
        public string NameFile { get; set; }//имя файла для отображения

        [Display(Name = "Проверено")]
        public bool Verified { get; set; }//проверено
        public int? Grade { get; set; }//поле на перспективу для оценивания деятельности студента

        
        public string Property1 { get; set; }//доп. свойства
        public string Property2 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
        public string Property5 { get; set; }

    }
}
