using System;
using System.ComponentModel.DataAnnotations;

namespace Accounts.Models
{
    public class MessageViewModel
    {
        public int MessageId { get; set; }

        public string RoleId { get; set; }

        [Display(Name = "Роль пользователя")]
        public string RoleName { get; set; }

        [Display(Name = "Тема сообщения")]
        public string Title { get; set; }

        [Display(Name = "Сообщение")]
        public string Content { get; set; }

        [Display(Name = "Дата")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = @"{0:dd\.MM\.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }
    }
}