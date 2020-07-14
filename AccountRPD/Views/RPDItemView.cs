using Account.DAL.Entities;
using AccountRPD.Interfaces.Views;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace AccountRPD.Views
{
    public partial class RPDItemView : Form, IRPDItemView
    {
        public string Number
        {
            get => tbNumber.Text;
            set => tbNumber.Text = value;
        }
        
        public Dictionary<int, string> Parents
        {
            get => cbParentNumber.DataSource as Dictionary<int, string>;
            set => cbParentNumber.DataSource = value;
        }

        public string NameItem
        {
            get => tbNameItem.Text;
            set => tbNameItem.Text = value;
        }

        public string TemplateItem
        {
            get => rtbTemplateItem.Text;
            set => rtbTemplateItem.Text = value;
        }

        public RPDItem SelectedParent => cbParentNumber.SelectedItem as RPDItem;

        public event EventHandler SaveRPDItemClick;
        public event EventHandler StandardValuesChangedHandler;


        public RPDItemView()
        {
            InitializeComponent();
        }        

        public void ShowView()
        {
            ShowDialog();
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            SaveRPDItemClick?.Invoke(this, e);
            Close();
        }        

        private void btCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        public void CloseView()
        {
            Close();
        }
    }
}
