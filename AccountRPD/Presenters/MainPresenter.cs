using AccountRPD.BL.Infrastructure;
using AccountRPD.Infrastucture;
using AccountRPD.Interfaces.Presenters;
using AccountRPD.Interfaces.Views;
using System;
using System.Linq;

namespace AccountRPD.Presenters
{
    public class MainPresenter : BasePresenter<IMainView>, IMainPresenter
    {
        public MainPresenter(IMainView view)
            : base(view)
        {
            View.OpenManagerHandler += View_OpenManager;
            View.OpenSettingsHandler += View_OpenSettings;
        }

        private void View_OpenSettings(object sender, EventArgs e)
        {
            var session = Session.GetSession();

            if (session.Roles.ToList().Contains("Administrators"))
            {
                Controller.Run<ISettingsPresenter>();
            }
        }

        private void View_OpenManager(object sender, EventArgs e)
        {
            Controller.Run<IManagerPresenter>();
        }
    }
}
