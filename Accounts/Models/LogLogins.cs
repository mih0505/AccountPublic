using System;
using System.ComponentModel.DataAnnotations;

namespace Accounts.Models
{
    public class LogLogins
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Display(Name = "Время входа")]
        public DateTime TimesLogin { get; set; }

        [Display(Name = "IP-адрес входа")]
        public string IP { get; set; }

        [Display(Name = "Браузер")]
        public string Browser { get; set; }

        [Display(Name = "Пользовательская информация")]
        public string UserAgent { get; set; }

        [Display(Name = "Реферрер")]
        public string Referrer { get; set; }

        [Display(Name = "Сообщение")]
        public string Message { get; set; }
    }
}