namespace AccountRPD.Views
{
    partial class RPDItemView
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tbNumber = new System.Windows.Forms.TextBox();
            this.cbParentNumber = new System.Windows.Forms.ComboBox();
            this.tbNameItem = new System.Windows.Forms.TextBox();
            this.rtbTemplateItem = new System.Windows.Forms.RichTextBox();
            this.btCancel = new System.Windows.Forms.Button();
            this.btOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Номер пункта";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(208, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(110, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Родительский пункт";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 42);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(94, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Название пункта";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 134);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(84, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Текст шаблона";
            // 
            // tbNumber
            // 
            this.tbNumber.Location = new System.Drawing.Point(96, 12);
            this.tbNumber.Name = "tbNumber";
            this.tbNumber.Size = new System.Drawing.Size(82, 20);
            this.tbNumber.TabIndex = 1;
            // 
            // cbParentNumber
            // 
            this.cbParentNumber.FormattingEnabled = true;
            this.cbParentNumber.Location = new System.Drawing.Point(324, 12);
            this.cbParentNumber.Name = "cbParentNumber";
            this.cbParentNumber.Size = new System.Drawing.Size(98, 21);
            this.cbParentNumber.TabIndex = 2;
            // 
            // tbNameItem
            // 
            this.tbNameItem.Location = new System.Drawing.Point(4, 58);
            this.tbNameItem.Multiline = true;
            this.tbNameItem.Name = "tbNameItem";
            this.tbNameItem.Size = new System.Drawing.Size(418, 64);
            this.tbNameItem.TabIndex = 3;
            // 
            // rtbTemplateItem
            // 
            this.rtbTemplateItem.Location = new System.Drawing.Point(4, 150);
            this.rtbTemplateItem.Name = "rtbTemplateItem";
            this.rtbTemplateItem.Size = new System.Drawing.Size(418, 177);
            this.rtbTemplateItem.TabIndex = 4;
            this.rtbTemplateItem.Text = "";
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(347, 333);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(75, 23);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "Отмена";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // btOK
            // 
            this.btOK.Location = new System.Drawing.Point(266, 333);
            this.btOK.Name = "btOK";
            this.btOK.Size = new System.Drawing.Size(75, 23);
            this.btOK.TabIndex = 5;
            this.btOK.Text = "OK";
            this.btOK.UseVisualStyleBackColor = true;
            this.btOK.Click += new System.EventHandler(this.btOK_Click);
            // 
            // RPDItemView
            // 
            this.AcceptButton = this.btOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(426, 361);
            this.Controls.Add(this.btOK);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.rtbTemplateItem);
            this.Controls.Add(this.tbNameItem);
            this.Controls.Add(this.cbParentNumber);
            this.Controls.Add(this.tbNumber);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximumSize = new System.Drawing.Size(442, 400);
            this.MinimumSize = new System.Drawing.Size(442, 370);
            this.Name = "RPDItemView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ItemRPDView";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbNumber;
        private System.Windows.Forms.ComboBox cbParentNumber;
        private System.Windows.Forms.TextBox tbNameItem;
        private System.Windows.Forms.RichTextBox rtbTemplateItem;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Button btOK;
    }
}