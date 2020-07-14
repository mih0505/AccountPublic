using Accounts.Models;
using System;
using System.Data.Entity;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;


namespace Accounts
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Database.SetInitializer<ApplicationDbContext>(null);//(new ApplicationDbInitializer());

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        public override void Init()
        {
            base.Init();
            this.BeginRequest += GlobalBeginRequest;
        }

        private void GlobalBeginRequest(object sender, EventArgs e)
        {
            var runTime = (HttpRuntimeSection)WebConfigurationManager.GetSection("system.web/httpRuntime");
            var maxRequestLength = runTime.MaxRequestLength * 1024;

            if (Request.ContentLength > maxRequestLength)
            {
                // или другой свой код обработки
                Response.Redirect("~/Home/Index");
            }
        }              
    }
}
