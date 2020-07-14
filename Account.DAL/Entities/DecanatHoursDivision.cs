using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Account.DAL.Entities
{
    public class DecanatHoursDivision
    {
        /// <summary>
        /// Идентификатор строки в плане
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор плана
        /// </summary>
        public int PlanId { get; set; }
 
        /// <summary>
        /// Идентификатор дисциплины
        /// </summary>
        public int DisciplineId { get; set; }

        /// <summary>
        /// Расчасовка по каждому из видов работ
        /// </summary>
        public IDictionary<string, string> Values { get; set; }

        public DecanatHoursDivision()
        {
            Values = new Dictionary<string, string>();
        }
    }
}
