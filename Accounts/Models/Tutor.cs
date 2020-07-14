namespace Accounts.Models
{
    public class Tutor
    {
        public int Id { get; set; }

        public int? FacultyId { get; set; }
        public Faculty Faculty { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public int GroupId { get; set; }
        public virtual Group Group { get; set; }
    }
}