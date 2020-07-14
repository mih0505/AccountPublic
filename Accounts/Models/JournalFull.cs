using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Accounts.Models
{
    public class JournalFull
    {
        public Journal Journal { get; set; }
        public string[,] Grades { get; set; }
    }
}