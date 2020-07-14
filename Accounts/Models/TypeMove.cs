using System.Collections.Generic;

namespace Accounts.Models
{
    public class TypeMove
    {
        public int Id { get; set; }
        public int DecanateId { get; set; }
        public string Name { get; set; }
        public int? Status1 { get; set; }
        public int? Status2 { get; set; }
        public string Comment { get; set; }
        public string Action { get; set; }

        public ICollection<History> Histories { get; set; }
        public TypeMove()
        {
            Histories = new List<History>();
        }
    }
}