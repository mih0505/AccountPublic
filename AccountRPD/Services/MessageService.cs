using AccountRPD.Interfaces;
using System.Windows.Forms;

namespace AccountRPD.Services
{
    class MessageService : IMessageService
    {
        public void ShowMessage(string message, string caption, MessageBoxIcon icon)
        {
            MessageBox.Show(message, caption, MessageBoxButtons.OK, icon);
        }

        public void ShowInformation(string message)
        {
            ShowMessage(message, "Сообщение", MessageBoxIcon.Information);
        }

        public void ShowExclamation(string exclamation)
        {
            ShowMessage(exclamation, "Предупреждение", MessageBoxIcon.Exclamation);
        }

        public void ShowError(string error)
        {
            ShowMessage(error, "Ошибка", MessageBoxIcon.Error);
        }

        public DialogResult ShowQuestion(string message, string title)
        {
            var dr = MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            return dr;
        }
    }
}
