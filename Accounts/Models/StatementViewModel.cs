using System;

namespace Accounts.Models
{
    public class StatementViewModel
    {
        public int StatementId { get; set; }        
        public string NameDiscipline { get; set; }
        public string TypeControl { get; set; }
        public string Hours{ get; set; }
        public string Teacher { get; set; }
        public string Teacher2 { get; set; }
        public string Teacher3 { get; set; }
        public string Teacher4 { get; set; }
        public string Teacher5 { get; set; }
        public string Teacher6 { get; set; }
        public string Teacher7 { get; set; }
        public string TeacherStatement { get; set; }
        public int Course { get; set; }
        public int Semester { get; set; }
        public DateTime? Date { get; set; }
        public int TotalPoint { get; set; }
        public string Grade { get; set; }
        public int GradeByNumber { get; set; }
        public string Competences { get; set; }
        public int? ParentId { get; set; }
    }
}