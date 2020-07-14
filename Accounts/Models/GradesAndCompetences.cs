using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Accounts.Models
{
    public class GradesAndCompetences
    {
        public List<StatementViewModel> Grades { get; set; }
        public List<Competence> Competences { get; set; }
        public List<CompetenceResults> Codes { get; set; }
    }
}