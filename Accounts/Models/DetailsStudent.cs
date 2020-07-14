using System.Collections.Generic;

namespace Accounts.Models
{
    public class DetailsStudent
    {
        public ApplicationUser User { get; set; }
        public Group Group { get; set; }
        public Faculty Faculty { get; set; }
        public Profile Profile { get; set; }
        public List<StatementViewModel> Grades { get; set; }
        public List<StatementViewModel> Arrears { get; set; }
        public List<CourseWorkStudent> CoursesWork { get; set; }
        public List<ReportPortfolio> Portfolio { get; set; }
    }
}