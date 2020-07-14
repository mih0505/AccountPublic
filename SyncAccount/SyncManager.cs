using Accounts.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace SyncAccount
{
    class SyncManager
    {
        public  bool VerifyUserNamePassword(string userName, string password)
        {
            var usermanager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            var user = usermanager.FindByEmail(userName);
            return user != null;
        }
    }
}
