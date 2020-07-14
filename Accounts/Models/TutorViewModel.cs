using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Accounts.Models
{
    public class TutorViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Поле {0} должно быть заполнено")]
        public int? FacultyId { get; set; }
        public List<Faculty> Faculties { get; set; }

        [Required(ErrorMessage = "Поле {0} должно быть заполнено")]
        public string UserId { get; set; }
        public List<TutorsList> TutorsList { get; set; }

        [Required(ErrorMessage = "Поле {0} должно быть заполнено")]
        public int GroupId { get; set; }
        public List<GroupsList> Groups { get; set; }
    }


    public class TutorsList
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class GroupsList
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class DepartmentsList
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}