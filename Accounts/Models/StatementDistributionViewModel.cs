using System.Collections.Generic;
using System.Web.Mvc;

namespace Accounts.Models
{
    public class StatementDistributionViewModel
    {        
        public StatementStudent StatementStudent { get; set; }
        public IEnumerable<SelectListItem> Teachers { get; set; }
        public IEnumerable<SelectListItem> Grades { get; set; }
    }
}