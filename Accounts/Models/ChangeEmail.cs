using System;

namespace Accounts.Models
{
    public class ChangeEmail
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string CurrentEmail { get; set; }
        public string NewEmail { get; set; }
        public string ConfirmationToken { get; set; }
        public DateTime DateChange { get; set; }
        public DateTime DateEndChange { get; set; }
        public bool IsConfirmed { get; set; }
    }
}