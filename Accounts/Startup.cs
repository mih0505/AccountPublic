using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Accounts.Startup))]
namespace Accounts
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
