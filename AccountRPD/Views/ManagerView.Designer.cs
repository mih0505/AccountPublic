namespace AccountRPD.Views
{
    partial class ManagerView
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ManagerView));
            this.studyYearsComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.rpdsDataGrid = new System.Windows.Forms.DataGridView();
            this.disciplinesDataGrid = new System.Windows.Forms.DataGridView();
            this.label5 = new System.Windows.Forms.Label();
            this.plansDataGrid = new System.Windows.Forms.DataGridView();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.hideNotActualPlansCheckBox = new System.Windows.Forms.CheckBox();
            this.departmentsComboBox = new System.Windows.Forms.ComboBox();
            this.disciplineFilterTextBox = new System.Windows.Forms.TextBox();
            this.disciplineFilterSearchButton = new System.Windows.Forms.Button();
            this.disciplineFilterClearButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.exportButton = new System.Windows.Forms.Button();
            this.rpdRemoveButton = new System.Windows.Forms.Button();
            this.rpdCreateButton = new System.Windows.Forms.Button();
            this.rpdEditButton = new System.Windows.Forms.Button();
            this.exitButton = new System.Windows.Forms.Button();
            this.assessmentSaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.rpdSaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rpdsDataGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.disciplinesDataGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.plansDataGrid)).BeginInit();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // studyYearsComboBox
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.studyYearsComboBox, 3);
            this.studyYearsComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.studyYearsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.studyYearsComboBox.FormattingEnabled = true;
            this.studyYearsComboBox.Location = new System.Drawing.Point(4, 21);
            this.studyYearsComboBox.Margin = new System.Windows.Forms.Padding(4);
            this.studyYearsComboBox.Name = "studyYearsComboBox";
            this.studyYearsComboBox.Size = new System.Drawing.Size(401, 25);
            this.studyYearsComboBox.TabIndex = 1;
            this.studyYearsComboBox.SelectedValueChanged += new System.EventHandler(this.ComboBoxSelectionChanged);
            // 
            // label2
            // 
            this.label2.AutoEllipsis = true;
            this.label2.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.label2, 3);
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(4, 0);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(401, 17);
            this.label2.TabIndex = 0;
            this.label2.Text = "Учебный год";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.AutoEllipsis = true;
            this.label1.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.label1, 2);
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(433, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(404, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Кафедра";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 8;
            this.tableLayoutPanel2.SetColumnSpan(this.tableLayoutPanel1, 6);
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.rpdsDataGrid, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.disciplinesDataGrid, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.plansDataGrid, 5, 4);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label4, 5, 3);
            this.tableLayoutPanel1.Controls.Add(this.hideNotActualPlansCheckBox, 7, 3);
            this.tableLayoutPanel1.Controls.Add(this.studyYearsComboBox, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label1, 5, 0);
            this.tableLayoutPanel1.Controls.Add(this.departmentsComboBox, 5, 1);
            this.tableLayoutPanel1.Controls.Add(this.disciplineFilterTextBox, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.disciplineFilterSearchButton, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.disciplineFilterClearButton, 2, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 7;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1132, 629);
            this.tableLayoutPanel1.TabIndex = 9;
            // 
            // rpdsDataGrid
            // 
            this.rpdsDataGrid.AllowUserToAddRows = false;
            this.rpdsDataGrid.AllowUserToDeleteRows = false;
            this.rpdsDataGrid.AllowUserToResizeRows = false;
            this.rpdsDataGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.rpdsDataGrid.BackgroundColor = System.Drawing.SystemColors.Control;
            this.rpdsDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.tableLayoutPanel1.SetColumnSpan(this.rpdsDataGrid, 8);
            this.rpdsDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rpdsDataGrid.Location = new System.Drawing.Point(3, 376);
            this.rpdsDataGrid.MultiSelect = false;
            this.rpdsDataGrid.Name = "rpdsDataGrid";
            this.rpdsDataGrid.ReadOnly = true;
            this.rpdsDataGrid.RowHeadersVisible = false;
            this.rpdsDataGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.rpdsDataGrid.Size = new System.Drawing.Size(1126, 250);
            this.rpdsDataGrid.TabIndex = 2;
            this.rpdsDataGrid.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.RPDsDataGridCellFormatting);
            this.rpdsDataGrid.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.RPDsDataGridBindingComplete);
            this.rpdsDataGrid.SelectionChanged += new System.EventHandler(this.DataGridSelectionChanged);
            // 
            // disciplinesDataGrid
            // 
            this.disciplinesDataGrid.AllowUserToAddRows = false;
            this.disciplinesDataGrid.AllowUserToDeleteRows = false;
            this.disciplinesDataGrid.AllowUserToResizeRows = false;
            this.disciplinesDataGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.disciplinesDataGrid.BackgroundColor = System.Drawing.SystemColors.Control;
            this.disciplinesDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.tableLayoutPanel1.SetColumnSpan(this.disciplinesDataGrid, 3);
            this.disciplinesDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.disciplinesDataGrid.Location = new System.Drawing.Point(3, 103);
            this.disciplinesDataGrid.MultiSelect = false;
            this.disciplinesDataGrid.Name = "disciplinesDataGrid";
            this.disciplinesDataGrid.ReadOnly = true;
            this.disciplinesDataGrid.RowHeadersVisible = false;
            this.disciplinesDataGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.disciplinesDataGrid.Size = new System.Drawing.Size(403, 250);
            this.disciplinesDataGrid.TabIndex = 1;
            this.disciplinesDataGrid.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.DataGridBindingComplete);
            this.disciplinesDataGrid.SelectionChanged += new System.EventHandler(this.DataGridSelectionChanged);
            // 
            // label5
            // 
            this.label5.AutoEllipsis = true;
            this.label5.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.label5, 8);
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Location = new System.Drawing.Point(3, 356);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(1126, 17);
            this.label5.TabIndex = 4;
            this.label5.Text = "Список рабочих программ для выбранной дисциплины:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // plansDataGrid
            // 
            this.plansDataGrid.AllowUserToAddRows = false;
            this.plansDataGrid.AllowUserToDeleteRows = false;
            this.plansDataGrid.AllowUserToResizeRows = false;
            this.plansDataGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.plansDataGrid.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.plansDataGrid.BackgroundColor = System.Drawing.SystemColors.Control;
            this.plansDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.tableLayoutPanel1.SetColumnSpan(this.plansDataGrid, 3);
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.plansDataGrid.DefaultCellStyle = dataGridViewCellStyle1;
            this.plansDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.plansDataGrid.Location = new System.Drawing.Point(432, 103);
            this.plansDataGrid.MultiSelect = false;
            this.plansDataGrid.Name = "plansDataGrid";
            this.plansDataGrid.ReadOnly = true;
            this.plansDataGrid.RowHeadersVisible = false;
            this.plansDataGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.plansDataGrid.Size = new System.Drawing.Size(697, 250);
            this.plansDataGrid.TabIndex = 1;
            this.plansDataGrid.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.DataGridBindingComplete);
            this.plansDataGrid.SelectionChanged += new System.EventHandler(this.DataGridSelectionChanged);
            // 
            // label3
            // 
            this.label3.AutoEllipsis = true;
            this.label3.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.label3, 3);
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(3, 50);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(403, 17);
            this.label3.TabIndex = 6;
            this.label3.Text = "Список дисциплин для выбранной кафедры:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.label4, 2);
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(433, 67);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(404, 33);
            this.label4.TabIndex = 0;
            this.label4.Text = "Список учебных планов для выбранной дисциплины:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // hideNotActualPlansCheckBox
            // 
            this.hideNotActualPlansCheckBox.AutoSize = true;
            this.hideNotActualPlansCheckBox.Dock = System.Windows.Forms.DockStyle.Right;
            this.hideNotActualPlansCheckBox.Location = new System.Drawing.Point(844, 70);
            this.hideNotActualPlansCheckBox.Name = "hideNotActualPlansCheckBox";
            this.hideNotActualPlansCheckBox.Size = new System.Drawing.Size(285, 27);
            this.hideNotActualPlansCheckBox.TabIndex = 6;
            this.hideNotActualPlansCheckBox.Text = "Отображать только актуальные планы";
            this.hideNotActualPlansCheckBox.UseVisualStyleBackColor = true;
            this.hideNotActualPlansCheckBox.CheckedChanged += new System.EventHandler(this.HideNotActualPlansCheckBoxCheckedChanged);
            // 
            // departmentsComboBox
            // 
            this.departmentsComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.departmentsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.departmentsComboBox.FormattingEnabled = true;
            this.departmentsComboBox.Location = new System.Drawing.Point(432, 20);
            this.departmentsComboBox.Name = "departmentsComboBox";
            this.departmentsComboBox.Size = new System.Drawing.Size(388, 25);
            this.departmentsComboBox.TabIndex = 5;
            this.departmentsComboBox.SelectedValueChanged += new System.EventHandler(this.ComboBoxSelectionChanged);
            // 
            // disciplineFilterTextBox
            // 
            this.disciplineFilterTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.disciplineFilterTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.disciplineFilterTextBox.Location = new System.Drawing.Point(3, 70);
            this.disciplineFilterTextBox.Name = "disciplineFilterTextBox";
            this.disciplineFilterTextBox.Size = new System.Drawing.Size(235, 24);
            this.disciplineFilterTextBox.TabIndex = 7;
            // 
            // disciplineFilterSearchButton
            // 
            this.disciplineFilterSearchButton.AutoSize = true;
            this.disciplineFilterSearchButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.disciplineFilterSearchButton.Location = new System.Drawing.Point(244, 70);
            this.disciplineFilterSearchButton.Name = "disciplineFilterSearchButton";
            this.disciplineFilterSearchButton.Size = new System.Drawing.Size(75, 27);
            this.disciplineFilterSearchButton.TabIndex = 8;
            this.disciplineFilterSearchButton.Text = "Поиск";
            this.disciplineFilterSearchButton.UseVisualStyleBackColor = true;
            this.disciplineFilterSearchButton.Click += new System.EventHandler(this.DisciplineFilterSearchButtonClick);
            // 
            // disciplineFilterClearButton
            // 
            this.disciplineFilterClearButton.AutoSize = true;
            this.disciplineFilterClearButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.disciplineFilterClearButton.Enabled = false;
            this.disciplineFilterClearButton.Location = new System.Drawing.Point(325, 70);
            this.disciplineFilterClearButton.Name = "disciplineFilterClearButton";
            this.disciplineFilterClearButton.Size = new System.Drawing.Size(81, 27);
            this.disciplineFilterClearButton.TabIndex = 9;
            this.disciplineFilterClearButton.Text = "Очистить";
            this.disciplineFilterClearButton.UseVisualStyleBackColor = true;
            this.disciplineFilterClearButton.Click += new System.EventHandler(this.DisciplineFilterClearButtonClick);
            this.disciplineFilterClearButton.Validating += new System.ComponentModel.CancelEventHandler(this.DisciplineFilterClearButtonValidating);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 6;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.exportButton, 3, 1);
            this.tableLayoutPanel2.Controls.Add(this.rpdRemoveButton, 2, 1);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.rpdCreateButton, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.rpdEditButton, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.exitButton, 5, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1138, 685);
            this.tableLayoutPanel2.TabIndex = 10;
            // 
            // exportButton
            // 
            this.exportButton.AutoSize = true;
            this.exportButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.exportButton.Location = new System.Drawing.Point(405, 638);
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(128, 44);
            this.exportButton.TabIndex = 13;
            this.exportButton.Text = "Экспортировать";
            this.exportButton.UseVisualStyleBackColor = true;
            this.exportButton.Click += new System.EventHandler(this.ExportButtonClick);
            this.exportButton.Validating += new System.ComponentModel.CancelEventHandler(this.RPDExportButtonValidating);
            // 
            // rpdRemoveButton
            // 
            this.rpdRemoveButton.AutoSize = true;
            this.rpdRemoveButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rpdRemoveButton.Location = new System.Drawing.Point(271, 638);
            this.rpdRemoveButton.Name = "rpdRemoveButton";
            this.rpdRemoveButton.Size = new System.Drawing.Size(128, 44);
            this.rpdRemoveButton.TabIndex = 12;
            this.rpdRemoveButton.Text = "Удалить";
            this.rpdRemoveButton.UseVisualStyleBackColor = true;
            this.rpdRemoveButton.Click += new System.EventHandler(this.RPDRemoveButtonClick);
            this.rpdRemoveButton.Validating += new System.ComponentModel.CancelEventHandler(this.RPDSpecialButtonsValidating);
            // 
            // rpdCreateButton
            // 
            this.rpdCreateButton.AutoSize = true;
            this.rpdCreateButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rpdCreateButton.Location = new System.Drawing.Point(3, 638);
            this.rpdCreateButton.Name = "rpdCreateButton";
            this.rpdCreateButton.Size = new System.Drawing.Size(128, 44);
            this.rpdCreateButton.TabIndex = 10;
            this.rpdCreateButton.Text = "Создать";
            this.rpdCreateButton.UseVisualStyleBackColor = true;
            this.rpdCreateButton.Click += new System.EventHandler(this.RPDCreateButtonClick);
            this.rpdCreateButton.Validating += new System.ComponentModel.CancelEventHandler(this.RPDCreateButtonValidating);
            // 
            // rpdEditButton
            // 
            this.rpdEditButton.AutoSize = true;
            this.rpdEditButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rpdEditButton.Location = new System.Drawing.Point(137, 638);
            this.rpdEditButton.Name = "rpdEditButton";
            this.rpdEditButton.Size = new System.Drawing.Size(128, 44);
            this.rpdEditButton.TabIndex = 11;
            this.rpdEditButton.Text = "Редактировать";
            this.rpdEditButton.UseVisualStyleBackColor = true;
            this.rpdEditButton.Click += new System.EventHandler(this.RPDEditButtonClick);
            this.rpdEditButton.Validating += new System.ComponentModel.CancelEventHandler(this.RPDSpecialButtonsValidating);
            // 
            // exitButton
            // 
            this.exitButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.exitButton.AutoSize = true;
            this.exitButton.Location = new System.Drawing.Point(1060, 638);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(75, 44);
            this.exitButton.TabIndex = 14;
            this.exitButton.Text = "Закрыть";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.ExitButtonClick);
            // 
            // assessmentSaveFileDialog
            // 
            this.assessmentSaveFileDialog.DefaultExt = "docx";
            this.assessmentSaveFileDialog.Filter = "Документ Word | *.docx";
            this.assessmentSaveFileDialog.Title = "Экспортировать оценочные материалы как...";
            // 
            // rpdSaveFileDialog
            // 
            this.rpdSaveFileDialog.DefaultExt = "docx";
            this.rpdSaveFileDialog.Filter = "Документ Word | *.docx";
            this.rpdSaveFileDialog.Title = "Экспортировать рабочую программу дисциплины как...";
            // 
            // ManagerView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1138, 685);
            this.Controls.Add(this.tableLayoutPanel2);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(1154, 724);
            this.Name = "ManagerView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Менеджер РПД";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ClosingExecute);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rpdsDataGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.disciplinesDataGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.plansDataGrid)).EndInit();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ComboBox studyYearsComboBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DataGridView plansDataGrid;
        private System.Windows.Forms.DataGridView disciplinesDataGrid;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DataGridView rpdsDataGrid;
        private System.Windows.Forms.CheckBox hideNotActualPlansCheckBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ComboBox departmentsComboBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button exportButton;
        private System.Windows.Forms.Button rpdRemoveButton;
        private System.Windows.Forms.Button rpdCreateButton;
        private System.Windows.Forms.Button rpdEditButton;
        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.TextBox disciplineFilterTextBox;
        private System.Windows.Forms.Button disciplineFilterSearchButton;
        private System.Windows.Forms.Button disciplineFilterClearButton;
        private System.Windows.Forms.SaveFileDialog assessmentSaveFileDialog;
        private System.Windows.Forms.SaveFileDialog rpdSaveFileDialog;
    }
}