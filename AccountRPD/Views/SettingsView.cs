using Account.DAL.Entities;
using AccountRPD.Infrastucture;
using AccountRPD.Interfaces.Views;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace AccountRPD.Views
{
    public partial class SettingsView : Form, ISettingsView
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        #region Свойства

        public IEnumerable<EducationStandard> StandardsList
        {
            get => dgvStandards.DataSource as IEnumerable<EducationStandard>;
            set => dgvStandards.DataSource = value;
        }

        public IEnumerable<EducationStandard> StandardSelectList
        {
            get => cbStandards.DataSource as IEnumerable<EducationStandard>;
            set => cbStandards.DataSource = value;
        }

        public IEnumerable<RPDItem> ItemsList
        {
            get => dgvItemsRPD.DataSource as IEnumerable<RPDItem>;
            set => dgvItemsRPD.DataSource = value;
        }

        public EducationStandard SelectedStandard => dgvStandards.CurrentRow?.DataBoundItem as EducationStandard;
        public EducationStandard SelectedStandardInFilter => cbStandards.SelectedItem as EducationStandard;
        public RPDItem SelectedItem => dgvItemsRPD.CurrentRow?.DataBoundItem as RPDItem;

        public string EducationStandartName { get; set; }
        public bool IsEducationStandartHide { get; set; }

        #endregion


        #region Проброс событий
        public event EventHandler StandardValuesChangedHandler;

        public event EventHandler AddStandardHandler;
        public event EventHandler EditStandardHandler;
        public event EventHandler DeleteStandardHandler;

        public event EventHandler AddItemHandler;
        public event EventHandler EditItemHandler;
        public event EventHandler DeleteItemHandler;

        //кнопки работы со стандартами
        private void btAddStandard_Click(object sender, EventArgs e)
        {
            AddStandardHandler?.Invoke(this, e);
            dgvStandards.Refresh();
        }

        private void btEditStandard_Click(object sender, EventArgs e)
        {
            EditStandardHandler?.Invoke(this, e);
            dgvStandards.Refresh();
        }

        private void btDeleteStandard_Click(object sender, EventArgs e)
        {
            if (dgvStandards.CurrentCell != null)
            {
                int rowIndex = dgvStandards.CurrentCell.RowIndex;
                var dr = Controller.MessageService.ShowQuestion($"Вы действительно хотите удалить образовательный стандарт: {dgvStandards.Rows[rowIndex].Cells["Title"].Value}", "Удаление");
                if (dr == DialogResult.Yes)
                {
                    DeleteStandardHandler?.Invoke(this, e);
                }
            }
        }

        private void cbStandards_SelectionChangeCommitted(object sender, EventArgs e)
        {
            StandardValuesChangedHandler?.Invoke(this, e);
        }

        //кнопки работы с содержанием РПД
        private void btAddItem_Click(object sender, EventArgs e)
        {
            AddItemHandler?.Invoke(this, e);
        }

        private void btEditItem_Click(object sender, EventArgs e)
        {
            EditItemHandler?.Invoke(this, e);
        }

        private void btDeleteItem_Click(object sender, EventArgs e)
        {
            int rowIndex = dgvItemsRPD.CurrentCell.RowIndex;
            var dr = Controller.MessageService.ShowQuestion($"Вы действительно хотите удалить пункт: {dgvItemsRPD.Rows[rowIndex].Cells["Name"].Value}", "Удаление");
            if (dr == DialogResult.Yes)
            {
                DeleteItemHandler?.Invoke(this, e);
            }
        }

        #endregion

        public void ShowView()
        {
            cbStandards.ValueMember = "Id";
            cbStandards.DisplayMember = "Title";
            //cbStandards.SelectedIndex = 0;

            ShowDialog();
        }

        #region Оформление таблиц
        private void StandardsListDataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvStandards.Columns["Id"].Visible = dgvStandards.Columns["RPDItems"].Visible = false;

            dgvStandards.Columns["Title"].HeaderText = "Название стандарта";
            dgvStandards.Columns["IsHide"].HeaderText = "Скрыть стандарт";
        }

        private void ItemsListDataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvItemsRPD.Columns["Id"].Visible = false;
            dgvItemsRPD.Columns["EducationStandardId"].Visible = false;

            dgvItemsRPD.Columns["Number"].HeaderText = "Номер пункта";
            dgvItemsRPD.Columns["Name"].HeaderText = "Наименование";
            dgvItemsRPD.Columns["ParentItemId"].HeaderText = "Номер родителя";
            dgvItemsRPD.Columns["IsHasContent"].HeaderText = "Заполнение";
            dgvItemsRPD.Columns["Note"].HeaderText = "Шаблон";
        }
        #endregion

        /* Внимание! Добавляйте новые методы перед этим комментарием */

        public void CloseView()
        {
            Close();
        }

    }
}
