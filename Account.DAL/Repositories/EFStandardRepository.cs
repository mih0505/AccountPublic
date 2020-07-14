using Account.DAL.Contexts;
using Account.DAL.Entities;
using Account.DAL.Interfaces;

namespace Account.DAL.Repositories
{
    public class EFStandardRepository : EFBaseRepository<EducationStandard>, IEFStandardRepository
    {
        public EFStandardRepository(AccountContext db) : base(db) { }
    }
}
