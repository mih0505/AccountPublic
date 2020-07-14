using System;

namespace AccountRPD.Interfaces.Views
{
    public interface ILoginView : IView
    {
        /// <summary>
        /// Введеннный пользователем логин
        /// </summary>
        string Login { get; }

        /// <summary>
        /// Введенный пользователем пароль
        /// </summary>
        string Password { get; }

        /// <summary>
        /// Событие, реагирующее на нажатие кнопки авторизации
        /// </summary>
        event EventHandler AuthorizeHandler;
    }
}
