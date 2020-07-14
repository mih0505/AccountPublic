using Account.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Account.DAL.Interfaces
{
    public interface IEFMemberRepository : IEFRepository<Member>
    {
        Task<IEnumerable<Member>> GetRPDsMembersAsync(int RPDId);
    }
}