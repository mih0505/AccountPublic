using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Account.DAL.Entities
{
    public class DocumentType
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public IEnumerable<RPDItem> RPDItems { get; set; }

        public DocumentType()
        {
            RPDItems = new List<RPDItem>();
        }
    }
}
