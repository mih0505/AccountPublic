using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Accounts.Models
{
    public class TeacherIdDecanat
    {
        public int Id { get; set; }

        public string SiteId { get; set; }
        [ForeignKey("SiteId")]
        public ApplicationUser User { get; set; }

        public int DecanatId { get; set; }
    }
}