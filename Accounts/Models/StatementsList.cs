using System;

namespace Accounts.Models
{
    public class StatementsList
    {
        public int Id { get; set; }
        public int? FacultyId { get; set; }
        public string TeacherDisciplineId { get; set; }
        public string TeacherDiscipline2Id { get; set; }
        public string TeacherDiscipline3Id { get; set; }
        public string TeacherDiscipline4Id { get; set; }
        public string TeacherDiscipline5Id { get; set; }
        public string TeacherDiscipline6Id { get; set; }
        public string TeacherDiscipline7Id { get; set; }
        public string AliasFaculty { get; set; }
        public string Faculty { get; set; }
        public string NameDiscipline { get; set; }
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
        public DateTime DateEnd { get; set; }
        public string CurrentYear { get; set; }
        public int? Form { get; set; }
        public int? Hours { get; set; }
        public int? ZET { get; set; }
        public int Complete { get; set; }
        public int CountStudents { get; set; }
    }
}