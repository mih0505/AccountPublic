namespace AccountRPD.Interfaces.Presenters
{
    public interface IPresenter
    {
        /// <summary>
        /// Запускает презентер
        /// </summary>
        /// <param name="parameters">Необязательные параметры</param>
        void Run();
    }
}
