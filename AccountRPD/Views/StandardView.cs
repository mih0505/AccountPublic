using AccountRPD.Interfaces.Views;
using System;
using System.Windows.Forms;

namespace AccountRPD.Views
{
    public partial class StandardView : Form, IStandardView
    {
        public string NameStandard 
        { 
            get => tbNameStandard.Text; 
            set => tbNameStandard.Text = value; 
        }

        public bool isHide
        {
            get => chbIsHide.Checked;
            set => chbIsHide.Checked = value;
        }

        public event EventHandler SaveStandardClick;



        public StandardView()
        {
            InitializeComponent();
        }              

        public void ShowView()
        {
            ShowDialog();
        }
        
        private void StandardView_Load(object sender, EventArgs e)
        {
            tbNameStandard.Focus();
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            SaveStandardClick?.Invoke(this, e);
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
