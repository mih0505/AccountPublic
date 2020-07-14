namespace AccountRPD.Views
{
    partial class StandardView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btOK = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tbNameStandard = new System.Windows.Forms.TextBox();
            this.chbIsHide = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btOK
            // 
            this.btOK.Location = new System.Drawing.Point(147, 81);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(75, 23);
            this.btOK.TabIndex = 0;
            this.btOK.Text = "OK";
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.btOK_Click);
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(228, 81);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(75, 23);
            this.btCancel.TabIndex = 0;
            this.btCancel.Text = "Отмена";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(149, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Образовательный стандарт";
            // 
            // tbNameStandard
            // 
            this.tbNameStandard.Location = new System.Drawing.Point(15, 30);
            this.tbNameStandard.Name = "tbNameStandard";
            this.tbNameStandard.Size = new System.Drawing.Size(288, 20);
            this.tbNameStandard.TabIndex = 0;
            // 
            // chbIsHide
            // 
            this.chbIsHide.AutoSize = true;
            this.chbIsHide.Location = new System.Drawing.Point(15, 58);
            this.chbIsHide.Name = "chbIsHide";
            this.chbIsHide.Size = new System.Drawing.Size(113, 17);
            this.chbIsHide.TabIndex = 1;
            this.chbIsHide.Text = "Скрыть стандарт";
            this.chbIsHide.UseVisualStyleBackColor = true;
            // 
            // StandardView
            // 
            this.AcceptButton = this.btOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(314, 116);
            this.Controls.Add(this.chbIsHide);
            this.Controls.Add(this.tbNameStandard);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximumSize = new System.Drawing.Size(330, 155);
            this.MinimumSize = new System.Drawing.Size(330, 155);
            this.Name = "StandardView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Добавить/Редактировать образовательный стандарт";
            this.Load += new System.EventHandler(this.StandardView_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btOK;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbNameStandard;
        private System.Windows.Forms.CheckBox chbIsHide;
    }
}