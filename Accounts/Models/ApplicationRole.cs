using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Accounts.Models
{
    public class ApplicationRole : IdentityRole
    {
        public string Description { get; set; }

        public List<Message> Messages { get; set; }

        public ApplicationRole()
        {
            Messages = new List<Message>();
        }
    }
}