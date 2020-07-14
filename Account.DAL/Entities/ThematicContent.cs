using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Account.DAL.Entities
{
    public class ThematicContent
    {
        public int Id { get; set; }

        public int RPDId { get; set; }

        public RPD RPD { get; set; }

        public int? ThematicPlanId { get; set; }

        public ThematicPlan ThematicPlan { get; set; } 

        public string Content { get; set; }

        public string LessonType { get; set; }
    }
}
