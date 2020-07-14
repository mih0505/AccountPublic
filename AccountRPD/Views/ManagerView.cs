using Account.DAL.Entities;
using AccountRPD.Enums;
using AccountRPD.EventArguments;
using AccountRPD.Extensions;
using AccountRPD.Infrastucture;
using AccountRPD.Interfaces.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AccountRPD.Views
{
    public partial class ManagerView : Form, IManagerView
    {
        public IEnumerable<string> StudyYears
        {
            get => studyYearsComboBox.DataSource as IEnumerable<string>;
            set => studyYearsComboBox.DataSource = value;
        }

        public string SelectedStudyYear
        {
            get => studyYearsComboBox.SelectedItem as string;
            set => studyYearsComboBox.SelectedItem = value;
        }

        public int SelectedStudyYearIndex
        {
            get => studyYearsComboBox.SelectedIndex;
            set => studyYearsComboBox.SelectedIndex = value;
        }

        public IEnumerable<DecanatDepartment> Departments
        {
            get => departmentsComboBox.DataSource as IEnumerable<DecanatDepartment>;
            set => departmentsComboBox.DataSource = value;
        }

        public DecanatDepartment SelectedDepartment => departmentsComboBox.SelectedItem as DecanatDepartment;

        public int SelectedDepartmentIndex
        {
            get => departmentsComboBox.SelectedIndex;
            set => departmentsComboBox.SelectedIndex = value;
        }

        public IEnumerable<DecanatDiscipline> Disciplines
        {
            get => disciplinesDataGrid.DataSource as IEnumerable<DecanatDiscipline>;
            set => disciplinesDataGrid.DataSource = value;
        }

        public DecanatDiscipline SelectedDiscipline => disciplinesDataGrid.GetSelectedItem() as DecanatDiscipline;

        public int SelectedDisciplineIndex
        {
            get => disciplinesDataGrid.GetSelectedItemIndex();
            set => disciplinesDataGrid.Rows[value].Selected = true;
        }

        public string FilterText
        {
            get => disciplineFilterTextBox.Text;

            set
            {
                disciplineFilterTextBox.Text = value;
                ExecuteSearch();
            }
        }

        public IEnumerable<DecanatPlan> Plans
        {
            get => plansDataGrid.DataSource as IEnumerable<DecanatPlan>;
            set => plansDataGrid.DataSource = value;
        }

        public DecanatPlan SelectedPlan => plansDataGrid.GetSelectedItem() as DecanatPlan;

        public int SelectedPlanIndex
        {
            get => plansDataGrid.GetSelectedItemIndex();
            set => plansDataGrid.Rows[value].Selected = true;
        }

        public bool HideNotActualPlans
        {
            get => hideNotActualPlansCheckBox.Checked;
            set => hideNotActualPlansCheckBox.Checked = value;
        }

        public IEnumerable<RPD> RPDs
        {
            get => rpdsDataGrid.DataSource as IEnumerable<RPD>;
            set => rpdsDataGrid.DataSource = value;
        }

        public RPD SelectedRPD => rpdsDataGrid.GetSelectedItem() as RPD;

        public int SelectedRPDIndex
        {
            get => rpdsDataGrid.GetSelectedItemIndex();
            set => rpdsDataGrid.Rows[value].Selected = true;
        }

        public string AssessmentFilePath => assessmentSaveFileDialog.FileName;
        public string RPDFilePath => rpdSaveFileDialog.FileName;

        public event EventHandler InitializedHandler;
        public event EventHandler<SelectionChangedEventArgs> SelectionChangedHandler;
        public event EventHandler<PermissionCheckEventArgs> PermissionCheckHandler;
        public event EventHandler DisciplineFilterClearedHandler;
        public event EventHandler RPDCreateHandler;
        public event EventHandler RPDEditHandler;
        public event CancelEventHandler RPDRemoveHandler;
        public event CancelEventHandler RPDExportHandler;
        public event CancelEventHandler AssessmentExportHandler;
        public event EventHandler ClosingHandler;

        public ManagerView()
        {
            InitializeComponent();
        }

        public void ShowView()
        {
            departmentsComboBox.ValueMember = "Id";
            departmentsComboBox.DisplayMember = "Title";

            InitializedHandler?.Invoke(this, EventArgs.Empty);

            ShowDialog();
        }

        private SelectionChangedEventArgs GetSelectionChangedEventArgs(SelectionTypes selectionType)
        {
            ValidateChildren();

            return new SelectionChangedEventArgs(selectionType);
        }

        private SelectionChangedEventArgs GetSelectionChangedEventArgs(object selectedValue)
        {
            var selectionType = selectedValue.GetSelectionType();

            return GetSelectionChangedEventArgs(selectionType);
        }

        private void ComboBoxSelectionChanged(object sender, EventArgs eventArgs)
        {
            if (sender is ComboBox comboBox)
            {
                ClearDisciplineFilter();

                var selectionChangedEventArgs = GetSelectionChangedEventArgs(comboBox.SelectedItem);
                SelectionChangedHandler?.Invoke(sender, selectionChangedEventArgs);
            }
        }

        private void DataGridSelectionChanged(object sender, EventArgs eventArgs)
        {
            if (sender is DataGridView dataGridView)
            {
                var selectedItem = dataGridView.GetSelectedItem();
                var selectionChangedEventArgs = GetSelectionChangedEventArgs(selectedItem);
                SelectionChangedHandler?.Invoke(sender, selectionChangedEventArgs);
            }
        }

        private void HideNotActualPlansCheckBoxCheckedChanged(object sender, EventArgs eventArgs)
        {
            var selectionChangedEventArgs = GetSelectionChangedEventArgs(SelectionTypes.Discipline);
            SelectionChangedHandler?.Invoke(sender, selectionChangedEventArgs);
        }

        private void ClearDisciplineFilter()
        {
            disciplineFilterTextBox.Text = string.Empty;
        }

        private void ExecuteSearch()
        {
            var selectionChangedEventArgs = GetSelectionChangedEventArgs(SelectionTypes.Filter);
            SelectionChangedHandler?.Invoke(this, selectionChangedEventArgs);
        }

        private void DisciplineFilterSearchButtonClick(object sender, EventArgs eventArgs)
        {
            ExecuteSearch();
        }

        private void DisciplineFilterClearButtonClick(object sender, EventArgs eventArgs)
        {
            ClearDisciplineFilter();

            DisciplineFilterClearedHandler?.Invoke(this, EventArgs.Empty);

            var selectionChangedEventArgs = GetSelectionChangedEventArgs(SelectionTypes.Department);
            SelectionChangedHandler?.Invoke(sender, selectionChangedEventArgs);
        }

        private void RPDExportButtonValidating(object sender, CancelEventArgs eventArgs)
        {
            exportButton.Enabled = SelectedRPD != null;
        }

        private void RPDCreateButtonClick(object sender, EventArgs eventArgs)
        {
            RPDCreateHandler?.Invoke(sender, eventArgs);
        }

        private void RPDEditButtonClick(object sender, EventArgs eventArgs)
        {
            RPDEditHandler?.Invoke(sender, eventArgs);
        }

        private void RPDRemoveButtonClick(object sender, EventArgs eventArgs)
        {
            var rpdRemoveCancelEventArgs = new CancelEventArgs();
            RPDRemoveHandler?.Invoke(sender, rpdRemoveCancelEventArgs);

            if (!rpdRemoveCancelEventArgs.Cancel)
            {
                var selectionChangedEventArgs = GetSelectionChangedEventArgs(SelectionTypes.Plan);
                SelectionChangedHandler?.Invoke(sender, selectionChangedEventArgs);
            }
        }

        private void DataGridBindingComplete(DataGridView dataGridView)
        {
            dataGridView.HighlightHeaderTextInBold();
        }

        private void DataGridBindingComplete(object sender, DataGridViewBindingCompleteEventArgs eventArgs)
        {
            DataGridBindingComplete(sender as DataGridView);
        }

        private void RPDsDataGridBindingComplete(object sender, DataGridViewBindingCompleteEventArgs eventArgs)
        {
            DataGridBindingComplete(rpdsDataGrid);

            rpdsDataGrid.Columns["FullnessPercent"].DefaultCellStyle.Format = "P1";
            rpdsDataGrid.Columns["EditDate"].DefaultCellStyle.NullValue = "Не редактировалось";
            rpdsDataGrid.Columns["EditDate"].DefaultCellStyle.Format = "dd MMMM yyyy г. в HH:mm (ddd)";
        }

        private void RPDsDataGridCellFormatting(object sender, DataGridViewCellFormattingEventArgs eventArgs)
        {
            if (rpdsDataGrid.Columns[eventArgs.ColumnIndex].Name.Equals("RdpType"))
            {
                var name = eventArgs.Value.ToString();
                var field = eventArgs.Value.GetType().GetField(name);
                var attributes = field.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

                if (!attributes.Length.Equals(0))
                {
                    eventArgs.Value = attributes.First().Description;
                }
            }
        }

        private void DisciplineFilterClearButtonValidating(object sender, CancelEventArgs eventArgs)
        {
            disciplineFilterClearButton.Enabled = !string.IsNullOrEmpty(FilterText);
        }

        private void RPDCreateButtonValidating(object sender, CancelEventArgs eventArgs)
        {
            rpdCreateButton.Enabled = SelectedPlan != null;
        }

        private void RPDSpecialButtonsValidating(object sender, CancelEventArgs eventArgs)
        {
            var permissionCheckEventArgs = new PermissionCheckEventArgs();
            PermissionCheckHandler?.Invoke(sender, permissionCheckEventArgs);

            rpdEditButton.Enabled = permissionCheckEventArgs.CanEdit;
            rpdRemoveButton.Enabled = permissionCheckEventArgs.CanRemove;
        }

        private void ExportButtonClick(object sender, EventArgs eventArgs)
        {
            rpdSaveFileDialog.FileName = $"Annot_{SelectedRPD.Block}_{SelectedRPD.DisciplineName}_{SelectedPlan.StudyYear}";
            assessmentSaveFileDialog.FileName = $"ОМ_{SelectedRPD.Block}_{SelectedRPD.DisciplineName}_{SelectedPlan.StudyYear}";

            var rpdExportDialogResult = rpdSaveFileDialog.ShowDialog();

            if (rpdExportDialogResult.Equals(DialogResult.OK))
            {
                var rpdCancelEventArgs = new CancelEventArgs();
                RPDExportHandler?.Invoke(this, rpdCancelEventArgs);

                if (!rpdCancelEventArgs.Cancel)
                {
                    var assessmentExportDialogResult = assessmentSaveFileDialog.ShowDialog();

                    var rpdName = Path.GetFileName(RPDFilePath);

                    if (assessmentExportDialogResult.Equals(DialogResult.OK))
                    {
                        var assessmentCancelEventArgs = new CancelEventArgs();
                        AssessmentExportHandler?.Invoke(this, assessmentCancelEventArgs);

                        if (!assessmentCancelEventArgs.Cancel)
                        {
                            var assessmentName = Path.GetFileName(AssessmentFilePath);
                            Controller.MessageService.ShowInformation($"Файлы {rpdName} и {assessmentName} успешно экспортированы");
                        }
                    }
                    else
                    {
                        Controller.MessageService.ShowInformation($"Файл {rpdName} успешно экспортирован");
                    }
                }
            }
        }

        private void ClosingExecute(object sender, FormClosingEventArgs eventArgs)
        {
            ClosingHandler?.Invoke(sender, eventArgs);
        }

        private void ExitButtonClick(object sender, EventArgs eventArgs)
        {
            CloseView();
        }

        public void CloseView()
        {
            Close();
        }
    }
}