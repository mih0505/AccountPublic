using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Accounts.Models
{
    public class IndividualStatement
    {
        public Statement Statement { get; set; }
        public StatementStudent StatementStudent { get; set; }
    }
}