using Account.DAL.Contexts;
using Account.DAL.Interfaces;
using Account.DAL.UnitOfWorks;
using Account.DAL.Repositories;
using AccountRPD.BL.Interfaces;
using AccountRPD.BL.Services;
using AccountRPD.Interfaces;
using AccountRPD.Interfaces.Presenters;
using AccountRPD.Interfaces.Views;
using AccountRPD.Presenters;
using AccountRPD.Services;
using AccountRPD.Views;
using Accounts.Models;
using Ninject;
using Ninject.Parameters;
using System.Windows.Forms;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using Account.DAL.References;

namespace AccountRPD.Infrastucture
{
    public class Container
    {
        /// <summary>
        /// Экземпляр IoC-контейнера
        /// </summary>
        public static IKernel Kernel { get; private set; }

        static Container()
        {
            Kernel = new StandardKernel();
        }

        /// <summary>
        /// Регистрация зависимостей
        /// </summary>
        public static void Setup()
        {
            /* Регистрация UnitOfWork'ов */
            Kernel.Bind<IEFUnitOfWork>().To<EFUnitOfWork>();
            Kernel.Bind<IDecanatUnitOfWork>().To<DecanatUnitOfWork>();

            /* Регистрация глобальных точек доступа */
            var applicationContext = new ApplicationContext();
            var applicationPresenter = new ApplicationPresenter(applicationContext);
            Kernel.Bind<ApplicationPresenter>().ToConstant(applicationPresenter);

            var messageService = new MessageService();
            Kernel.Bind<IMessageService>().ToConstant(messageService);

            /* Регистрация сервисов */
            Kernel.Bind<ILoginService>().To<LoginService>();
            Kernel.Bind<IMainService>().To<MainService>();
            Kernel.Bind<IManagerService>().To<ManagerService>();
            //Kernel.Bind<ISettingsService>().To<SettingsService>();
            Kernel.Bind<IRPDService>().To<RPDService>();
            Kernel.Bind<IRPDExportService>().To<RPDExportService>();
            Kernel.Bind<IAssessmentExportService>().To<AssessmentExportService>();

            Kernel.Bind<IManagerState>().To<ManagerState>().InSingletonScope();

            /* Регистрация представлений */
            Kernel.Bind<ILoginView>().To<LoginView>();
            Kernel.Bind<IMainView>().To<MainView>();
            Kernel.Bind<IManagerView>().To<ManagerView>();
            Kernel.Bind<IRPDView>().To<RPDView>();
            Kernel.Bind<ISettingsView>().To<SettingsView>();

            /* Регистрация презентеров */
            Kernel.Bind<ILoginPresenter>().To<LoginPresenter>();
            Kernel.Bind<IMainPresenter>().To<MainPresenter>();
            Kernel.Bind<IManagerPresenter>().To<ManagerPresenter>();
            Kernel.Bind<IRPDPresenter>().To<RPDPresenter>();
            Kernel.Bind<ISettingsPresenter>().To<SettingsPresenter>();
        }

        /// <summary>
        /// Получение реализации интерфейса или сервиса и передача в него параметров при необходимости
        /// </summary>
        /// <typeparam name="TImplementation">Интерфейс или реализация</typeparam>
        /// <typeparam name="parameters">Передаваемые параметры</typeparam>
        /// <returns></returns>
        public static TImplementation Get<TImplementation>(params IParameter[] parameters)
        {
            return Kernel.Get<TImplementation>(parameters);
        }
    }
}
