using System.Collections.Generic;

namespace Accounts.Models
{
    public class TypeOrder
    {
        public int Id { get; set; }
        public int DecanateId { get; set; }
        public string Name { get; set; }
        public int? ActionId { get; set; }
        public int? ActionDecanateId { get; set; }
        public int? OrdersTypeDecanateId { get; set; }
        public int? OrdersTypeId { get; set; }
        public OrdersType OrdersType { get; set; }
        public bool? HideOrder { get; set; }

        public ICollection<History> Histories { get; set; }
        public TypeOrder()
        {
            Histories = new List<History>();
        }
    }
}