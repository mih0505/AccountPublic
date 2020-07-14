using AccountRPD.Interfaces;
using AccountRPD.Interfaces.Presenters;
using AccountRPD.Presenters;
using Ninject.Parameters;

namespace AccountRPD.Infrastucture
{
    public static class Controller
    {
        /// <summary>
        /// Ярлык глобальной точки доступа
        /// </summary>
        public static ApplicationPresenter Application 
        { 
            get
            {
                return Container.Get<ApplicationPresenter>();
            }
        }


        /// <summary>
        /// Ярлык сервиса сообщений
        /// </summary>
        public static IMessageService MessageService
        {
            get
            {
                return Container.Get<IMessageService>();
            }
        }

        /// <summary>
        /// Получение и запуск презентера
        /// </summary>
        /// <typeparam name="TPresenter">Интерфейс или реализация презентера</typeparam>
        /// <param name="parameters">Необязательные параметры для конструктора</param>
        public static void Run<TPresenter>(params IParameter[] parameters) where TPresenter : IPresenter
        {
            Container.Get<TPresenter>(parameters).Run();
        }
    }
}
