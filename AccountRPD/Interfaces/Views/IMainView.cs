using System;

namespace AccountRPD.Interfaces.Views
{
    public interface IMainView : IView
    {
        /// <summary>
        /// Событие, реагирующее на нажатие кнопки открытия менеджера РПД
        /// </summary>
        event EventHandler OpenManagerHandler;

        event EventHandler OpenSettingsHandler;
    }
}
