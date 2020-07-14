using AccountRPD.BL.Infrastructure;
using System.Windows.Forms;

namespace AccountRPD.Presenters
{
    public class ApplicationPresenter
    {
        public ApplicationContext Context { get; set; }

        public Session Session { get; set; }

        public ApplicationPresenter(ApplicationContext applicationContext)
        {
            Context = applicationContext;
        }
    }
}
