using System.Windows.Forms;

namespace AccountRPD.Interfaces
{
    public interface IMessageService
    {
        /// <summary>
        /// Отображает сообщение
        /// </summary>
        /// <param name="message">Содержание сообщения</param>
        /// <param name="caption">Заголовок сообщения</param>
        /// <param name="icon">Отображаемая иконка</param>
        void ShowMessage(string message, string caption, MessageBoxIcon icon);

        /// <summary>
        /// Отображает информационное сообщение
        /// </summary>
        /// <param name="message">Содержание сообщения</param>
        void ShowInformation(string message);

        /// <summary>
        /// Отображает сообщение с предупреждением
        /// </summary>
        /// <param name="exclamation">Содержание сообщения</param>
        void ShowExclamation(string exclamation);

        /// <summary>
        /// Отображает сообщение с ошибкой
        /// </summary>
        /// <param name="error">Содержание сообщения</param>
        void ShowError(string error);


        /// <summary>
        /// Отображает подтверждение
        /// </summary>
        /// <param name="message">Содержание сообщения</param>
        DialogResult ShowQuestion(string message, string title);
    }
}
