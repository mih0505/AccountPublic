using AccountRPD.Interfaces.Presenters;
using AccountRPD.Interfaces.Views;

namespace AccountRPD.Presenters
{
    public abstract class BasePresenter<TView> : IPresenter where TView : IView
    {
        protected TView View { get; private set; }

        protected BasePresenter(TView view)
        {
            View = view;
        }

        public virtual void Run()
        {
            View.ShowView();
        }
    }
}