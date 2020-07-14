using Account.DAL.Entities;
using AccountRPD.BL;
using AccountRPD.BL.Interfaces;
using AccountRPD.Interfaces.Presenters;
using AccountRPD.Interfaces.Views;
using AccountRPD.Presenters;
using System.ComponentModel;

namespace AccountRPD
{
    public class StandardPresenter : BasePresenter<IStandardView>, IStandardPresenter
    {
        public EducationStandard EducationStandard { get; private set; }

        private readonly IStandardService _standardService;

        //public StandardPresenter(IStandardView view, IStandardService standard) : base(view)
        //{
        //    _standardService = standard;
        //    EducationStandard = new EducationStandard();
        //    View.SaveStandardClick += View_SaveStandardClick;
        //}

        public StandardPresenter(IStandardView view, 
            IStandardService standardService, 
            EducationStandard standard) : base(view)
        {
            _standardService = standardService;

            EducationStandard = standard;

            View.NameStandard = EducationStandard.Title;
            View.isHide = EducationStandard.IsHide;

            View.SaveStandardClick += View_SaveStandardClick;
        }

        //дополнительный конструктор с передачей bindingList, т.к. иногда не происходит обновления dgv после первого добавления
        public StandardPresenter(IStandardView view, 
            IStandardService standardService, 
            BindingList<EducationStandard> standards) : base(view)
        {
            _standardService = standardService;
            EducationStandard = new EducationStandard();
            View.NameStandard = EducationStandard.Title;
            View.isHide = EducationStandard.IsHide;

            standards.Add(EducationStandard);

            View.SaveStandardClick += View_SaveStandardClick;
        }

        private void View_SaveStandardClick(object sender, System.EventArgs e)
        {
            EducationStandard.Title = View.NameStandard;
            EducationStandard.IsHide = View.isHide;
            _standardService.SaveStandardAsync(EducationStandard);
        }
    }
}