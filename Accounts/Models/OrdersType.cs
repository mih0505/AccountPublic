using System.Collections.Generic;

namespace Accounts.Models
{
    public class OrdersType
    {
        public int Id { get; set; }
        public int DecanateId { get; set; }
        public string Name { get; set; }
        public bool? Out { get; set; }
        public string Note { get; set; }

        public ICollection<History> Histories { get; set; }
        public OrdersType()
        {
            Histories = new List<History>();
        }
    }
}