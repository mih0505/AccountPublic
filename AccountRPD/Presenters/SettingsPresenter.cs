using Account.DAL.Entities;
using AccountRPD.BL.Interfaces;
using AccountRPD.Infrastucture;
using AccountRPD.Interfaces.Presenters;
using AccountRPD.Interfaces.Views;
using Ninject.Parameters;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace AccountRPD.Presenters
{
    public class SettingsPresenter : BasePresenter<ISettingsView>, ISettingsPresenter
    {
        private readonly ISettingsService _settingsService;
        private readonly IStandardService _standardService;
        private readonly IRPDItemService _rpdItemService;

        private BindingList<EducationStandard> standardBindingList;

        public SettingsPresenter(ISettingsView view,
            ISettingsService settingsService,
            IStandardService standardService,
            IRPDItemService rpdItemService) : base(view)
        {
            _settingsService = settingsService;
            _standardService = standardService;
            _rpdItemService = rpdItemService;


            standardBindingList = new BindingList<EducationStandard>(_standardService.GetAllStandard() as IList<EducationStandard>);
            View.StandardsList = standardBindingList;
            View.StandardSelectList = _standardService.GetAllStandard();

            View.AddItemHandler += View_AddItem;
            View.AddStandardHandler += View_AddStandard;

            View.EditItemHandler += View_EditItem;
            View.EditStandardHandler += View_EditStandard;

            View.DeleteItemHandler += View_DeleteItem;
            View.DeleteStandardHandler += View_DeleteStandard;
        }

        private void View_DeleteStandard(object sender, EventArgs e)
        {
            _standardService.RemoveStandardAsync(View.SelectedStandard);
            standardBindingList.Remove(View.SelectedStandard);
        }

        private void View_DeleteItem(object sender, EventArgs e)
        {
            _rpdItemService.RemoveRPDItem(View.SelectedItem);
        }

        private void View_EditStandard(object sender, EventArgs e)
        {
            var standard = new ConstructorArgument("standard", View.SelectedStandard);
            Controller.Run<IStandardPresenter>(standard);
        }

        private void View_EditItem(object sender, EventArgs e)
        {
            var rpdItem = new ConstructorArgument("rpdItem", View.SelectedItem);
            Controller.Run<IRPDItemPresenter>(rpdItem);
        }

        private void View_AddStandard(object sender, EventArgs e)
        {
            //передаем bindingList, т.к. не происходит отображение первого добавленного элемента в dgv
            var standards = new ConstructorArgument("standards", standardBindingList);
            Controller.Run<IStandardPresenter>(standards);
        }

        private void View_AddItem(object sender, EventArgs e)
        {
            var standard = new ConstructorArgument("standard", View.SelectedStandardInFilter);
            Controller.Run<IRPDItemPresenter>(standard);
        }
    }
}
