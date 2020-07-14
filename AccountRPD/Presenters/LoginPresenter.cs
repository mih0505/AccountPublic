using AccountRPD.BL.Interfaces;
using AccountRPD.Infrastucture;
using AccountRPD.Interfaces.Presenters;
using AccountRPD.Interfaces.Views;
using System;

namespace AccountRPD.Presenters
{
    public class LoginPresenter : BasePresenter<ILoginView>, ILoginPresenter
    {
        private readonly ILoginService loginService;

        public LoginPresenter(ILoginView view, ILoginService loginService)
            : base(view)
        {
            this.loginService = loginService;

            View.AuthorizeHandler += new EventHandler(View_Auth);
        }

        private void View_Auth(object sender, EventArgs e)
        {
            try
            {
                string login = View.Login;
                string password = View.Password;

                var session = loginService.Login(login, password);

                if (!session.Details.Succedeed)
                {
                    Controller.MessageService.ShowError(session.Details.Message);
                    return;
                }

                Controller.Application.Session = session;

                Controller.Run<IMainPresenter>();
                View.CloseView();
            }

            catch (Exception ex)
            {
                Controller.MessageService.ShowError(ex.Message);
            }
        }
    }
}
