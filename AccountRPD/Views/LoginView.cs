using AccountRPD.Infrastucture;
using AccountRPD.Interfaces.Views;
using System;
using System.Windows.Forms;

namespace AccountRPD.Views
{
    public partial class LoginView : Form, ILoginView
    {
        public string Login => tbLogin.Text;
        public string Password => tbPassword.Text; 

        public event EventHandler AuthorizeHandler;

        public LoginView()
        {
            InitializeComponent();
        }

        public void ShowView()
        {
            Controller.Application.Context.MainForm = this;
            Application.Run(Controller.Application.Context);
        }

        private void LoginExecute(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbLogin.Text) && string.IsNullOrWhiteSpace(tbPassword.Text))
            {
                Controller.MessageService.ShowExclamation("Поля «Логин или e-mail» и «Пароль» не могут быть пустыми");

                return;
            }

            if (string.IsNullOrWhiteSpace(tbLogin.Text))
            {
                Controller.MessageService.ShowExclamation("Поле «Логин или e-mail» не может быть пустым");

                return;
            }
            
            if (string.IsNullOrWhiteSpace(tbPassword.Text))
            {
                Controller.MessageService.ShowExclamation("Поле «Пароль» не может быть пустыми");

                return;
            }

            AuthorizeHandler?.Invoke(this, e);
        }

        private void CancelExecute(object sender, EventArgs e)
        {
            CloseView();
        }

        /* Внимание! Добавляйте новые методы перед этим комментарием */

        public void CloseView()
        {
            Close();
        }
    }
}
