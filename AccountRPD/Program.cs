using AccountRPD.Infrastucture;
using AccountRPD.Interfaces.Presenters;
using System;
using System.Windows.Forms;

namespace AccountRPD
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            /* Загружаем настройки IoC-контейнера */
            Container.Setup();

            /* Загружаем окно авторизации */
            Controller.Run<ILoginPresenter>();
        }
    }
}
