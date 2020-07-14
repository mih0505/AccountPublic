using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Accounts.Models
{
    public class ReadMessage
    {
        public int Id { get; set; }

        public int MessageId { get; set; }

        public Message Message { get; set; }

        public string UserId { get; set; }

        public ApplicationUser User { get; set; }
    }
}