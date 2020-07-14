using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Accounts.Models
{
    public class StatementFull
    {
        public Statement Statement { get; set; }
        public List<StatementDistributionViewModel> StatementDistribution { get; set; }
    }
}