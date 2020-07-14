using System.Collections.Generic;
using System.Web.Mvc;

namespace Accounts.Models
{
    public class DistributionViewModel
    {        
        public CourseWorkStudent CourseWorkStudent { get; set; }
        public IEnumerable<SelectListItem> Teachers { get; set; }
        public IEnumerable<SelectListItem> Teachers2 { get; set; }
        public IEnumerable<SelectListItem> Teachers3 { get; set; }
        public IEnumerable<SelectListItem> Teachers4 { get; set; }
        public IEnumerable<SelectListItem> Teachers5 { get; set; }        
    }
}