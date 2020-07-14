using Account.DAL.Entities;
using AccountRPD.BL.Interfaces;
using AccountRPD.Interfaces.Presenters;
using AccountRPD.Interfaces.Views;
using AccountRPD.Presenters;
using System;

namespace AccountRPD
{
    public class RPDItemPresenter : BasePresenter<IRPDItemView>, IRPDItemPresenter
    {
        
        public RPDItem RPDItem { get; set; }
        private readonly IRPDItemService _rpdItemService;
        private readonly EducationStandard _educationStandard;

        public RPDItemPresenter(IRPDItemView view,
            IRPDItemService rpdItemServer, 
            EducationStandard standard) : base(view)
        {
            _rpdItemService = rpdItemServer;
            _educationStandard = standard;
            RPDItem = new RPDItem();
            RPDItem.EducationStandardId = standard.Id;

            View.SaveRPDItemClick += View_SaveRPDItemClick;
        }
                       
        public RPDItemPresenter(IRPDItemView view, 
            IRPDItemService rpdItemServer,
            RPDItem item,
            EducationStandard standard) : base(view)
        {
            _rpdItemService = rpdItemServer;
            _educationStandard = standard;
            RPDItem = item;

            View.Number = item.Number;
            //View.ParentId = item.ParentItemId;
            View.NameItem = item.Name;
            View.TemplateItem = item.Note;            

            View.SaveRPDItemClick += View_SaveRPDItemClick;
        }

        
        //public StandardPresenter(IStandardView view,
        //    IStandardService standardService,
        //    BindingList<EducationStandard> standards) : base(view)
        //{
        //    _standardService = standardService;
        //    EducationStandard = new EducationStandard();
        //    View.NameStandard = EducationStandard.Title;
        //    View.isHide = EducationStandard.IsHide;

        //    standards.Add(EducationStandard);

        //    View.SaveStandardClick += View_SaveStandardClick;
        //}

        private void View_SaveRPDItemClick(object sender, EventArgs e)
        {
            RPDItem.Number = View.Number;
            //RPDItem.ParentItemId = View.ParentId;
            RPDItem.Name = View.NameItem;
            RPDItem.Note = View.TemplateItem;
            RPDItem.EducationStandardId = _educationStandard.Id;
            _rpdItemService.SaveRPDItemAsync(RPDItem);
        }
    }
}