using Accounts.Models;
using Microsoft.AspNet.Identity;

namespace Account.DAL.References
{
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
                : base(store)
        { }
    }
}
