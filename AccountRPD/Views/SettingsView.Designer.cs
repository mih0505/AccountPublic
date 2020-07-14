namespace AccountRPD.Views
{
    partial class SettingsView
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.btDeleteStandard = new System.Windows.Forms.Button();
            this.btEditStandard = new System.Windows.Forms.Button();
            this.btAddStandard = new System.Windows.Forms.Button();
            this.dgvStandards = new System.Windows.Forms.DataGridView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.btDeleteItem = new System.Windows.Forms.Button();
            this.btEditItem = new System.Windows.Forms.Button();
            this.btAddItem = new System.Windows.Forms.Button();
            this.dgvItemsRPD = new System.Windows.Forms.DataGridView();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cbStandards = new System.Windows.Forms.ComboBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStandards)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvItemsRPD)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(800, 450);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.btDeleteStandard);
            this.tabPage1.Controls.Add(this.btEditStandard);
            this.tabPage1.Controls.Add(this.btAddStandard);
            this.tabPage1.Controls.Add(this.dgvStandards);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(792, 424);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Стандарты";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // btDeleteStandard
            // 
            this.btDeleteStandard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btDeleteStandard.Location = new System.Drawing.Point(711, 64);
            this.btDeleteStandard.Name = "btDeleteStandard";
            this.btDeleteStandard.Size = new System.Drawing.Size(75, 23);
            this.btDeleteStandard.TabIndex = 1;
            this.btDeleteStandard.Text = "Удалить";
            this.btDeleteStandard.UseVisualStyleBackColor = true;
            this.btDeleteStandard.Click += new System.EventHandler(this.btDeleteStandard_Click);
            // 
            // btEditStandard
            // 
            this.btEditStandard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btEditStandard.Location = new System.Drawing.Point(711, 35);
            this.btEditStandard.Name = "btEditStandard";
            this.btEditStandard.Size = new System.Drawing.Size(75, 23);
            this.btEditStandard.TabIndex = 1;
            this.btEditStandard.Text = "Изменить";
            this.btEditStandard.UseVisualStyleBackColor = true;
            this.btEditStandard.Click += new System.EventHandler(this.btEditStandard_Click);
            // 
            // btAddStandard
            // 
            this.btAddStandard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btAddStandard.Location = new System.Drawing.Point(711, 6);
            this.btAddStandard.Name = "btAddStandard";
            this.btAddStandard.Size = new System.Drawing.Size(75, 23);
            this.btAddStandard.TabIndex = 1;
            this.btAddStandard.Text = "Добавить";
            this.btAddStandard.UseVisualStyleBackColor = true;
            this.btAddStandard.Click += new System.EventHandler(this.btAddStandard_Click);
            // 
            // dgvStandards
            // 
            this.dgvStandards.AllowUserToAddRows = false;
            this.dgvStandards.AllowUserToDeleteRows = false;
            this.dgvStandards.AllowUserToResizeColumns = false;
            this.dgvStandards.AllowUserToResizeRows = false;
            this.dgvStandards.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvStandards.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvStandards.Location = new System.Drawing.Point(3, 3);
            this.dgvStandards.MultiSelect = false;
            this.dgvStandards.Name = "dgvStandards";
            this.dgvStandards.ReadOnly = true;
            this.dgvStandards.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvStandards.Size = new System.Drawing.Size(702, 418);
            this.dgvStandards.TabIndex = 0;
            this.dgvStandards.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.StandardsListDataBindingComplete);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.btDeleteItem);
            this.tabPage2.Controls.Add(this.btEditItem);
            this.tabPage2.Controls.Add(this.btAddItem);
            this.tabPage2.Controls.Add(this.dgvItemsRPD);
            this.tabPage2.Controls.Add(this.label2);
            this.tabPage2.Controls.Add(this.label1);
            this.tabPage2.Controls.Add(this.cbStandards);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(792, 424);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Содержание РПД";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // btDeleteItem
            // 
            this.btDeleteItem.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btDeleteItem.Location = new System.Drawing.Point(711, 126);
            this.btDeleteItem.Name = "btDeleteItem";
            this.btDeleteItem.Size = new System.Drawing.Size(75, 23);
            this.btDeleteItem.TabIndex = 3;
            this.btDeleteItem.Text = "Удалить";
            this.btDeleteItem.UseVisualStyleBackColor = true;
            this.btDeleteItem.Click += new System.EventHandler(this.btDeleteItem_Click);
            // 
            // btEditItem
            // 
            this.btEditItem.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btEditItem.Location = new System.Drawing.Point(711, 97);
            this.btEditItem.Name = "btEditItem";
            this.btEditItem.Size = new System.Drawing.Size(75, 23);
            this.btEditItem.TabIndex = 4;
            this.btEditItem.Text = "Изменить";
            this.btEditItem.UseVisualStyleBackColor = true;
            this.btEditItem.Click += new System.EventHandler(this.btEditItem_Click);
            // 
            // btAddItem
            // 
            this.btAddItem.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btAddItem.Location = new System.Drawing.Point(711, 68);
            this.btAddItem.Name = "btAddItem";
            this.btAddItem.Size = new System.Drawing.Size(75, 23);
            this.btAddItem.TabIndex = 5;
            this.btAddItem.Text = "Добавить";
            this.btAddItem.UseVisualStyleBackColor = true;
            this.btAddItem.Click += new System.EventHandler(this.btAddItem_Click);
            // 
            // dgvItemsRPD
            // 
            this.dgvItemsRPD.AllowUserToAddRows = false;
            this.dgvItemsRPD.AllowUserToDeleteRows = false;
            this.dgvItemsRPD.AllowUserToResizeColumns = false;
            this.dgvItemsRPD.AllowUserToResizeRows = false;
            this.dgvItemsRPD.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvItemsRPD.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvItemsRPD.Location = new System.Drawing.Point(11, 68);
            this.dgvItemsRPD.MultiSelect = false;
            this.dgvItemsRPD.Name = "dgvItemsRPD";
            this.dgvItemsRPD.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvItemsRPD.Size = new System.Drawing.Size(694, 348);
            this.dgvItemsRPD.TabIndex = 2;
            this.dgvItemsRPD.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.ItemsListDataBindingComplete);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Содержание РПД";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(149, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Образовательный стандарт";
            // 
            // cbStandards
            // 
            this.cbStandards.FormattingEnabled = true;
            this.cbStandards.Location = new System.Drawing.Point(163, 17);
            this.cbStandards.Name = "cbStandards";
            this.cbStandards.Size = new System.Drawing.Size(144, 21);
            this.cbStandards.TabIndex = 0;
            this.cbStandards.SelectionChangeCommitted += new System.EventHandler(this.cbStandards_SelectionChangeCommitted);
            // 
            // SettingsView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tabControl1);
            this.Name = "SettingsView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Настройки";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvStandards)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvItemsRPD)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.DataGridView dgvStandards;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.DataGridView dgvItemsRPD;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbStandards;
        private System.Windows.Forms.Button btDeleteStandard;
        private System.Windows.Forms.Button btEditStandard;
        private System.Windows.Forms.Button btAddStandard;
        private System.Windows.Forms.Button btDeleteItem;
        private System.Windows.Forms.Button btEditItem;
        private System.Windows.Forms.Button btAddItem;
    }
}