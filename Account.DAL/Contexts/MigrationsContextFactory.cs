using System.Data.Entity.Infrastructure;

namespace Account.DAL.Contexts
{
    public class MigrationsContextFactory : IDbContextFactory<AccountContext>
    {
        public AccountContext Create()
        {
            return new AccountContext("DefaultConnection");
        }
    }
}
