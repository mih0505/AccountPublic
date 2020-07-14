using System.ComponentModel.DataAnnotations;

namespace Accounts.Models
{
    public class ListTeachersDepartments
    {
        public int Id { get; set; }
        public string TeacherId { get; set; }
        [Display(Name = "Преподаватель")]
        public string Lastname { get; set; }
        public string Firstname { get; set; }
        public string Middlename { get; set; }
        [Display(Name = "Кафедра")]
        public string Department { get; set; }
        [Display(Name = "Заведующий")]
        public bool IsManager { get; set; }
    }
}