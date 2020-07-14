using System.Collections.Generic;
using System.Web.Mvc;

namespace Accounts.Models
{
    public class StudiesViewModel
    {
        public Study Study { get; set; }
        public IEnumerable<SelectListItem> Grades1 { get; set; }
        public IEnumerable<SelectListItem> Grades2 { get; set; }
        public IEnumerable<SelectListItem> Grades3 { get; set; }
    }
}