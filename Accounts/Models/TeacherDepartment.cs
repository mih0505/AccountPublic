using System.ComponentModel.DataAnnotations;

namespace Accounts.Models
{
    public class TeacherDepartment
    {
        public int Id { get; set; }
        [Display(Name = "Преподаватель")]
        [Required]

        public string TeacherId { get; set; }
        public ApplicationUser Teacher { get; set; }
        [Display(Name = "Кафедра")]
        [Required]
        public int DepartmentId { get; set; }
        public Department Department { get; set; }
        [Display(Name = "Заведующий")]
        public bool IsManager { get; set; }
    }
}