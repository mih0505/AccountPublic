using Account.DAL.Contexts;
using Account.DAL.Entities;
using Account.DAL.Interfaces;

namespace Account.DAL.Repositories
{
    public class EFDocumentTypeRepository : EFBaseRepository<DocumentType>, IEFDocumentTypeRepository
    {
        public EFDocumentTypeRepository(AccountContext context)
            : base(context)
        { }
    }
}
