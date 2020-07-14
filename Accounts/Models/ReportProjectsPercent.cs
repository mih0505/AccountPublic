namespace Accounts.Models
{
    public class ReportProjectsPercent
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public string Group { get; set; }
        public int Course { get; set; }
        public int Semester { get; set; }
        public string Year { get; set; }
        public int CountStudents { get; set; }
        public int CountProject { get; set; }
        public double FillPercent { get; set; }
    }
}