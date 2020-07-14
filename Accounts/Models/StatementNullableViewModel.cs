using System;

namespace Accounts.Models
{
    public class StatementNullableViewModel
    {
        public int Id { get; set; }
        public int FacultyId { get; set; }
        public string FacultyName { get; set; }
        public string Discipline { get; set; }
        public string GroupName { get; set; }
        public string Teacher { get; set; }
        public string Teacher2 { get; set; }
        public string Teacher3 { get; set; }
        public string Teacher4 { get; set; }
        public string Teacher5 { get; set; }
        public string Teacher6 { get; set; }
        public string Teacher7 { get; set; }
        public string TypeControl { get; set; }
        public int Course { get; set; }
        public int Semester { get; set; }
        public DateTime Date { get; set; }
        public string Years { get; set; }
        public int? ZET { get; set; }
        public int? Hours { get; set; }
        public int CountStudents { get; set; }
        public int CountGrades { get; set; }
    }
}