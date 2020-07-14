using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Accounts.Models
{
    public class Message
    {
        public int Id { get; set; }

        public string RoleId { get; set; }

        [Display(Name = "Роль пользователя")]
        public ApplicationRole Role { get; set; }

        public List<ReadMessage> ReadMessages { get; set; }

        [Display(Name = "Тема сообщения")]
        public string Title { get; set; }

        [Display(Name = "Сообщение")]
        [DataType(DataType.MultilineText)]
        [AllowHtml]
        public string Content { get; set; }

        [Display(Name = "Дата")]
        [DataType(DataType.DateTime)]        
        [DisplayFormat(DataFormatString = @"{0:dd\.MM\.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        public Message()
        {
            ReadMessages = new List<ReadMessage>();
        }
    }
}