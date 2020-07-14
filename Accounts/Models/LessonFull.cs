using System.Collections.Generic;
using System.Web.Mvc;

namespace Accounts.Models
{
    public class LessonFull
    {
        public Lesson Lesson { get; set; }
        public IEnumerable<SelectListItem> TypeLesson { get; set; }
        public IList<StudiesViewModel> Study { get; set; }
    }
}