namespace Accounts.Models
{
    public class ReportPortfolio
    {
        public string Id { get; set; }
        public string Lastname { get; set; }
        public string Firstname { get; set; }
        public string Middlename { get; set; }
        public bool loggedIn { get; set; }
        public string Faculty { get; set; }
        public string Department { get; set; }
        public int Science { get; set; }
        public int Social { get; set; }
        public int Sports { get; set; }
        public int Cultural { get; set; }
    }
}