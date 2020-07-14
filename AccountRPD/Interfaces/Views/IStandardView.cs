using System;

namespace AccountRPD.Interfaces.Views
{
    public interface IStandardView : IView
    {
        string NameStandard { get; set; }
        bool isHide { get; set; }

        event EventHandler SaveStandardClick;
    }
}