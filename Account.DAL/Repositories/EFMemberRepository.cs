using Account.DAL.Contexts;
using Account.DAL.Entities;
using Account.DAL.Interfaces;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Account.DAL.Repositories
{
    public class EFMemberRepository : EFBaseRepository<Member>, IEFMemberRepository
    {
        public EFMemberRepository(AccountContext db) : base(db)
        { }

        public async Task<IEnumerable<Member>> GetRPDsMembersAsync(int RPDId)
        {
            return await _context.Members.Where(Member => Member.RPDId.Equals(RPDId)).ToListAsync();
        }
    }
}