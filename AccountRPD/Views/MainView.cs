using AccountRPD.Infrastucture;
using AccountRPD.Interfaces.Views;
using System;
using System.Windows.Forms;

namespace AccountRPD.Views
{
    public partial class MainView : Form, IMainView
    {
        public MainView()
        {
            InitializeComponent();
        }

        public event EventHandler OpenManagerHandler;
        public event EventHandler OpenSettingsHandler;

        private void OpenManagerExecute(object sender, EventArgs e)
        {
            OpenManagerHandler?.Invoke(this, e);
        }

        public void ShowView()
        {
            Controller.Application.Context.MainForm = this;
            Show();
        }


        private void MainView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.Shift && e.KeyCode == Keys.S) 
            {
                OpenSettingsHandler?.Invoke(this, e);
                e.SuppressKeyPress = true;
            }
        }

        /* Внимание! Добавляйте новые методы перед этим комментарием */

        public void CloseView()
        {
            Close();
        }
    }
}
