using Account.DAL.Entities;

namespace AccountRPD.Interfaces.Presenters
{
    public interface IStandardPresenter : IPresenter
    {
        EducationStandard EducationStandard { get; }
    }
}