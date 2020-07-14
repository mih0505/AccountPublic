using Account.DAL.Entities;
using AccountRPD.BL.DTOs;
using AccountRPD.Infrastucture;
using AccountRPD.Interfaces.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using License = Account.DAL.Entities.License;

namespace AccountRPD.Views
{
    public partial class RPDView : Form, IRPDView
    {
        private IDictionary<DecanatCompetence, Competence> competences;
        private IDictionary<DecanatCompetence, IEnumerable<CompetenceGrade>> competenceDescriptions;

        public string RPDName
        {
            get => Text;
            set => Text = value;
        }

        public string FacultyTitle
        {
            get => tbFacultyTitle.Text;
            set => tbFacultyTitle.Text = value;
        }

        public string DepartmentTitle
        {
            get => tbDepartmentTitle.Text;
            set => tbDepartmentTitle.Text = value;
        }

        public string DisciplineTitle
        {
            get => tbDisciplineTitle.Text;
            set => tbDisciplineTitle.Text = value;
        }

        public string ProfileTitle
        {
            get => tbProfileTitle.Text;
            set => tbProfileTitle.Text = value;
        }

        public IEnumerable<Member> MembersList
        {
            get => dgvMembersList.DataSource as IEnumerable<Member>;
            set => dgvMembersList.DataSource = value;
        }

        public string Results
        {
            get => rtbResults.Text;
            set => rtbResults.Text = value;
        }

        public string DisciplinePlace
        {
            get => rtbDisciplinePlace.Text;
            set => rtbDisciplinePlace.Text = value;
        }

        public string DepartmentChief
        {
            get => tbDepartmentChief.Text;
            set => tbDepartmentChief.Text = value;
        }

        public DateTime ApprovalDateRPD
        {
            get => dtpApprovalDateRPD.Value;
            set => dtpApprovalDateRPD.Value = value;
        }

        public string ProtocolNumberRPD
        {
            get => tbProtocolNumberRPD.Text;
            set => tbProtocolNumberRPD.Text = value.ToString();
        }

        public IDictionary<DecanatCompetence, Competence> Competences
        {
            get => competences;
            set
            {
                competences = value;
                UpdateCompetencesTreeView();
            }
        }

        public KeyValuePair<string, string>[] HoursDivision
        {
            get => dgvHoursDivision.DataSource as KeyValuePair<string, string>[];
            set => dgvHoursDivision.DataSource = value;
        }

        public Competence SelectedCompetence
        {
            get
            {
                var decanatCompetenceId = tvCompetences.SelectedNode?.Parent?.Tag as int?;

                return Competences.FirstOrDefault(Competence => Competence.Key.Id.Equals(decanatCompetenceId)).Value;
            }

            set
            {
                var decanatCompetenceId = tvCompetences.SelectedNode.Parent?.Tag as int?;
                var decanatCompetence = Competences.FirstOrDefault(Competence => Competence.Key.Id.Equals(decanatCompetenceId)).Key;

                Competences.Remove(decanatCompetence);
                Competences.Add(decanatCompetence, value);
            }
        }

        public TreeNode SelectedCompetenceNode
        {
            get => tvCompetences.SelectedNode;
        }

        public string CompetenceName
        {
            get => tbCompetenceName.Text;
            set => tbCompetenceName.Text = value;
        }

        public string CompetenceStage
        {
            get => tbCompetenceStage.Text;
            set => tbCompetenceStage.Text = value;
        }

        public IEnumerable<ThematicPlan> ThematicPlans
        {
            get => dgvThematicPlans.DataSource as IEnumerable<ThematicPlan>;
            set => dgvThematicPlans.DataSource = value;
        }

        public ThematicPlan SelectedThematicPlan => dgvThematicPlans.CurrentRow?.DataBoundItem as ThematicPlan;

        public IEnumerable<string> LessonsTypes
        {
            get => cbLessonsTypes.DataSource as IEnumerable<string>;
            set => cbLessonsTypes.DataSource = value;
        }

        public string SelectedLessonType
        {
            get => cbLessonsTypes.SelectedItem as string;
            set => cbLessonsTypes.SelectedItem = value;
        }

        public IDictionary<string, IEnumerable<ThematicContent>> ThematicContentsDictionary { get; set; }

        public IEnumerable<ThematicContent> ThematicContents
        {
            get => dgvThematicContents.DataSource as IEnumerable<ThematicContent>;
            set
            {
                dgvThematicContents.DataSource = value;
                UpdateThematicContentsDataGridView();
            }
        }

        public string ThematicContentValue { get; private set; }

        public ThematicContent SelectedThematicContent => dgvThematicContents.CurrentRow?.DataBoundItem as ThematicContent;

        public string TotalLectures
        {
            get => lbTotalLectures.Text;
            set => lbTotalLectures.Text = value;
        }

        public string TotalPractices
        {
            get => lbTotalPractices.Text;
            set => lbTotalPractices.Text = value;
        }

        public string TotalLabs
        {
            get => lbTotalLabs.Text;
            set => lbTotalLabs.Text = value;
        }

        public string TotalIndividualWorks
        {
            get => lbTotalIndividualWorks.Text;
            set => lbTotalIndividualWorks.Text = value;
        }

        public string TotalPlanLectures
        {
            get => lbTotalPlanLectures.Text;
            set => lbTotalPlanLectures.Text = value;
        }

        public string TotalPlanPractices
        {
            get => lbTotalPlanPractices.Text;
            set => lbTotalPlanPractices.Text = value;
        }

        public string TotalPlanLabs
        {
            get => lbTotalPlanLabs.Text;
            set => lbTotalPlanLabs.Text = value;
        }

        public string TotalPlanIndividualWorks
        {
            get => lbTotalPlanIndividualWorks.Text;
            set => lbTotalPlanIndividualWorks.Text = value;
        }

        public bool IsIndependentWorkLoadFromWord
        {
            get => isIndependentWorkLoadFromWord.Checked;
            set => isIndependentWorkLoadFromWord.Checked = value;
        }

        public bool IsIndependentWorkInsertText
        {
            get => isIndependentWorkInsertText.Checked;
            set => isIndependentWorkInsertText.Checked = value;
        }

        public string IndependentWorkInsertionFilePath
        {
            get => independentWorkOpenFileDialog.FileName;
            set => independentWorkFilePath.Text = value;
        }

        public string IndependentWorkInsertionText
        {
            get => independentWorkText.Text;
            set => independentWorkText.Text = value;
        }

        public IDictionary<DecanatCompetence, IEnumerable<CompetenceGrade>> CompetenceGrades
        {
            get
            {
                return competenceDescriptions;
            }

            set
            {
                competenceDescriptions = value;
                UpdateCompetenceGradesControls();
            }
        }

        public IEnumerable<CompetenceGrade> SelectedCompetenceGrades
        {
            get
            {
                var competenceGradeId = competenceDesc.TabPages[competenceDesc.SelectedIndex].Tag as int?;

                return CompetenceGrades.FirstOrDefault(KeyValuePair => KeyValuePair.Key.Id.Equals(competenceGradeId)).Value;
            }
        }

        public bool IsCheckWorkLoadFromWord
        {
            get => isCheckWorkLoadFromWord.Checked;
            set => isCheckWorkLoadFromWord.Checked = value;
        }

        public bool IsCheckWorkInsertText
        {
            get => isCheckWorkInsertionText.Checked;
            set => isCheckWorkInsertionText.Checked = value;
        }

        public string CheckWorkInsertionFilePath
        {
            get => checkWorkOpenFileDialog.FileName;
            set => checkWorkFilePath.Text = value;
        }

        public string CheckWorkInsertionText
        {
            get => checkWorkText.Text;
            set => checkWorkText.Text = value;
        }

        public bool IsAssessmentLoadFromWord
        {
            get => isAssessmentLoadFromWord.Checked;
            set => isAssessmentLoadFromWord.Checked = value;
        }

        public bool IsAssessmentInsertText
        {
            get => isAssessmentInsertionText.Checked;
            set => isAssessmentInsertionText.Checked = value;
        }

        public string AssessmentInsertionFilePath
        {
            get => assessmentOpenFileDialog.FileName;
            set => assessmentFilePath.Text = value;
        }

        public string AssessmentInsertionText
        {
            get => assessmentInsertText.Text;
            set => assessmentInsertText.Text = value;
        }

        public bool CanEditState
        {
            get => cbRPDTypes.Enabled;
            set => cbRPDTypes.Enabled = value;
        }

        public bool CanEditAuthor
        {
            get => cbAuthors.Enabled;
            set => cbAuthors.Enabled = value;
        }

        public IEnumerable<BasicLiterature> BasicLiteratures
        {
            get => dgvBasicEduLiteratures.DataSource as IEnumerable<BasicLiterature>;
            set => dgvBasicEduLiteratures.DataSource = value;
        }

        public IEnumerable<AdditionalLiterature> AdditionalLiteratures
        {
            get => dgvAdditionalLiteratures.DataSource as IEnumerable<AdditionalLiterature>;
            set => dgvAdditionalLiteratures.DataSource = value;
        }

        public IEnumerable<LibrarySystem> LibrarySystems
        {
            get => dgvLibrarySystems.DataSource as IEnumerable<LibrarySystem>;
            set => dgvLibrarySystems.DataSource = value;
        }

        public IEnumerable<InternetResource> InternetResources
        {
            get => dgvInternet.DataSource as IEnumerable<InternetResource>;
            set => dgvInternet.DataSource = value;
        }

        public IEnumerable<License> Licenses
        {
            get => dgvLicenses.DataSource as IEnumerable<License>;
            set => dgvLicenses.DataSource = value;
        }

        public IEnumerable<MaterialBase> MaterialBases
        {
            get => dgvMaterialsBase.DataSource as IEnumerable<MaterialBase>;
            set => dgvMaterialsBase.DataSource = value;
        }

        public IEnumerable<TeacherDTO> Authors
        {
            get => cbAuthors.DataSource as IEnumerable<TeacherDTO>;
            set => cbAuthors.DataSource = value;
        }

        public TeacherDTO SelectedAuthor
        {
            get => cbAuthors.SelectedItem as TeacherDTO;
            set => cbAuthors.SelectedValue = value.Id;
        }

        public IEnumerable<string> RPDTypes
        {
            get => cbRPDTypes.DataSource as IEnumerable<string>;
            set => cbRPDTypes.DataSource = value;
        }

        public string SelectedRPDType
        {
            get => cbRPDTypes.SelectedItem as string;
            set => cbRPDTypes.SelectedItem = value;
        }

        public int EducationStandardId { get; set; }

        public event EventHandler ViewInitializedHandler;
        public event EventHandler DisciplineTitleTextChangedHandler;
        public event EventHandler CompetenceStageBeforeSelectHandler;
        public event EventHandler CompetenceNameBeforeSelectHandler;
        public event TreeViewEventHandler CompetenceSelectionChangedHandler;
        public event EventHandler AddSectionHandler;
        public event EventHandler AddTopicHandler;
        public event EventHandler RemoveSectionOrTopicHandler;
        public event DataGridViewCellEventHandler ThematicPlanEndEditHandler;
        public event EventHandler LessonTypesSelectHandler;
        public event DataGridViewCellEventHandler ThematicContentEndEditHandler;
        public event EventHandler SaveClickHandler;

        public RPDView()
        {
            InitializeComponent();
        }

        public void ShowView()
        {
            MdiParent = Controller.Application.Context.MainForm;

            if (EducationStandardId.Equals(2))
            {
                tabPage2.Parent = null;
                tabPage3.Text = "РП-1";
                label11.Text = label11.Text.Replace(".2", string.Empty);
                label19.Text = label19.Text.Replace("Место", "Цели и место");
            }
            else
            {
                label29.Visible = false;
                tbCompetenceName.Visible = false;
                tableLayoutPanel2.SetColumn(label15, 0);
                tableLayoutPanel2.SetColumnSpan(label15, 2);
                tableLayoutPanel2.SetColumn(tbCompetenceStage, 0);
                tableLayoutPanel2.SetColumnSpan(tbCompetenceStage, 2);
            }

            cbAuthors.ValueMember = "Id";
            cbAuthors.DisplayMember = "Shortname";

            ViewInitializedHandler?.Invoke(this, EventArgs.Empty);

            Show();
        }

        private void UpdateCompetenceGradesControls()
        {
            competenceDesc.TabPages.Clear();

            var index = 0;
            foreach (var valuePair in CompetenceGrades)
            {
                competenceDesc.TabPages.Add(valuePair.Key.Code);
                competenceDesc.TabPages[index].Tag = valuePair.Key.Id;

                var dataGridView = new DataGridView()
                {
                    DataSource = valuePair.Value,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    AllowUserToOrderColumns = false,
                    AllowUserToResizeColumns = true,
                    AllowUserToResizeRows = true,
                    BackgroundColor = SystemColors.Control,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                    SelectionMode = DataGridViewSelectionMode.RowHeaderSelect,
                    Dock = DockStyle.Fill,
                    RowHeadersVisible = false
                };

                dataGridView.DataBindingComplete += CompetenceGradesDataBindingComplete;

                competenceDesc.TabPages[index].Controls.Add(dataGridView);

                index++;
            }
        }

        private void CompetenceGradesDataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            var dataGridView = sender as DataGridView;
            dataGridView.Columns["Stage"].ReadOnly = true;
            dataGridView.Columns["Id"].Visible = false;
            dataGridView.Columns["DecanatId"].Visible = false;
            dataGridView.Columns["RPDId"].Visible = false;
            dataGridView.Columns["RPD"].Visible = false;
            dataGridView.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font(dataGridView.ColumnHeadersDefaultCellStyle.Font, FontStyle.Bold);
        }

        private void UpdateThematicContentsDataGridView()
        {
            var thematicContentsDataTable = new DataTable();

            thematicContentsDataTable.Columns.Add("№ п/п", typeof(string));
            thematicContentsDataTable.Columns.Add("IsSection", typeof(bool));
            thematicContentsDataTable.Columns.Add("Наименование раздела/темы дисциплины", typeof(string));
            thematicContentsDataTable.Columns.Add("Содержание", typeof(string));

            foreach (var thematicContent in ThematicContents)
            {
                thematicContentsDataTable.Rows.Add(thematicContent.ThematicPlan.Number, thematicContent.ThematicPlan.IsSection, thematicContent.ThematicPlan.Topic, thematicContent.Content);
            }

            dgvThematicContents.DataSource = thematicContentsDataTable;
        }

        private void UpdateCompetencesTreeView()
        {
            tvCompetences.Nodes.Clear();

            foreach (var valuePair in Competences)
            {
                var stagesNodes = default(TreeNode[]);

                if (EducationStandardId.Equals(1))
                {
                    stagesNodes = new TreeNode[]
                    {
                        new TreeNode("Знать"),
                        new TreeNode("Уметь"),
                        new TreeNode("Владеть")
                    };
                }
                else
                {
                    stagesNodes = new TreeNode[]
                    {
                        new TreeNode($"{valuePair.Key.Code}.1"),
                        new TreeNode($"{valuePair.Key.Code}.2"),
                        new TreeNode($"{valuePair.Key.Code}.3")
                    };
                }

                var competenceNode = new TreeNode($"{valuePair.Key.Code}. {valuePair.Key.Content}", stagesNodes)
                {
                    Tag = valuePair.Key.Id
                };

                tvCompetences.Nodes.Add(competenceNode);
            }
        }

        private void OnRPDPartsDataGridViewDataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            var dataGridView = sender as DataGridView;
            dataGridView.Columns["Id"].Visible = false;
            dataGridView.Columns["RPDId"].Visible = false;
            dataGridView.Columns["RPD"].Visible = false;
            dataGridView.ColumnHeadersDefaultCellStyle.Font = new Font(dataGridView.ColumnHeadersDefaultCellStyle.Font, FontStyle.Bold);
        }

        private void DisciplineTitleTextChangedExecute(object sender, EventArgs e)
        {
            DisciplineTitleTextChangedHandler?.Invoke(sender, e);
        }

        private void CompetencesBeforeSelectExecute(object sender, EventArgs e)
        {
            CompetenceStageBeforeSelectHandler?.Invoke(sender, e);
            CompetenceNameBeforeSelectHandler?.Invoke(sender, e);
        }

        private void CompetenceStageTextChanged(object sender, EventArgs e)
        {
            CompetenceStageBeforeSelectHandler?.Invoke(sender, e);
        }

        private void CompetenceNameTextChanged(object sender, EventArgs e)
        {
            CompetenceNameBeforeSelectHandler?.Invoke(sender, e);
        }

        private void CompetenceSelectionChangedExecute(object sender, TreeViewEventArgs e)
        {
            CompetenceSelectionChangedHandler?.Invoke(sender, e);

            if (SelectedCompetenceNode.Parent != null)
            {
                var decanatCompetence = Competences.FirstOrDefault(Competence => Competence.Key.Id.Equals(SelectedCompetenceNode.Parent.Tag)).Key;
                var selectedNodeTitle = (EducationStandardId.Equals(1)) ? $"{decanatCompetence.Code}. {SelectedCompetenceNode.Text}" : $"{SelectedCompetenceNode.Text}";
                label31.Text = $"Выбранный узел: {selectedNodeTitle}";
                tbCompetenceName.Enabled = true;
                tbCompetenceStage.Enabled = true;
            }
            else
            {
                label31.Text = "Узел не выбран";
                tbCompetenceName.Text = string.Empty;
                tbCompetenceName.Enabled = false;
                tbCompetenceStage.Text = string.Empty;
                tbCompetenceStage.Enabled = false;
            }
        }

        private void MembersListDataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvMembersList.Columns["Id"].Visible = false;
            dgvMembersList.Columns["RPDId"].Visible = false;
            dgvMembersList.Columns["RPD"].Visible = false;
            dgvMembersList.Columns["TeacherId"].Visible = false;
            dgvMembersList.Columns["Teacher"].Visible = false;
            dgvMembersList.Columns["MemberType"].Visible = false;
            dgvMembersList.ColumnHeadersDefaultCellStyle.Font = new Font(dgvMembersList.ColumnHeadersDefaultCellStyle.Font, FontStyle.Bold);
        }

        private void HoursDivisionDataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvHoursDivision.Columns["Key"].HeaderText = "Форма работы";
            dgvHoursDivision.Columns["Value"].HeaderText = "Часы";
            dgvHoursDivision.ColumnHeadersDefaultCellStyle.Font = new Font(dgvHoursDivision.ColumnHeadersDefaultCellStyle.Font, FontStyle.Bold);
        }

        private void ThematicPlanDataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvThematicPlans.Columns["Id"].Visible = false;
            dgvThematicPlans.Columns["RpdId"].Visible = false;
            dgvThematicPlans.Columns["RPD"].Visible = false;
            dgvThematicPlans.Columns["IsSection"].Visible = false;
            dgvThematicPlans.Columns["ThematicContents"].Visible = false;
            dgvThematicPlans.Columns["Number"].ReadOnly = true;
            dgvThematicPlans.ColumnHeadersDefaultCellStyle.Font = new Font(dgvThematicPlans.ColumnHeadersDefaultCellStyle.Font, FontStyle.Bold);

            foreach (DataGridViewRow row in dgvThematicPlans.Rows)
            {
                var plan = row.DataBoundItem as ThematicPlan;

                if (plan.IsSection)
                {
                    dgvThematicPlans["Lecture", row.Index].ReadOnly = true;
                    dgvThematicPlans["Practice", row.Index].ReadOnly = true;
                    dgvThematicPlans["Lab", row.Index].ReadOnly = true;
                    dgvThematicPlans["IndividualWork", row.Index].ReadOnly = true;

                    dgvThematicPlans.Rows[row.Index].DefaultCellStyle.Font = new Font(dgvThematicPlans.Rows[row.Index].InheritedStyle.Font, FontStyle.Bold);
                }
            }
        }

        private void ThematicContentsDataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (dgvThematicContents.DataSource.GetType() != typeof(DataTable))
            {
                return;
            }

            dgvThematicContents.ColumnHeadersDefaultCellStyle.Font = new Font(dgvThematicContents.ColumnHeadersDefaultCellStyle.Font, FontStyle.Bold);
            dgvThematicContents.Columns["№ п/п"].ReadOnly = true;
            dgvThematicContents.Columns["IsSection"].Visible = false;
            dgvThematicContents.Columns["Наименование раздела/темы дисциплины"].ReadOnly = true;

            foreach (DataGridViewColumn column in dgvThematicContents.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.Programmatic;
            }

            foreach (DataGridViewRow row in dgvThematicContents.Rows)
            {
                if ((bool?)dgvThematicContents["IsSection", row.Index].Value ?? false)
                {
                    dgvThematicContents.Rows[row.Index].ReadOnly = true;
                    dgvThematicContents.Rows[row.Index].DefaultCellStyle.Font = new Font(dgvThematicContents.Rows[row.Index].InheritedStyle.Font, FontStyle.Bold);
                }
            }
        }

        private void AddSectionClickExecute(object sender, EventArgs e)
        {
            AddSectionHandler?.Invoke(sender, e);
        }

        private void AddTopicClickExecute(object sender, EventArgs e)
        {
            AddTopicHandler?.Invoke(sender, e);
        }

        private void RemoveSectionOrTopicClickExecute(object sender, EventArgs e)
        {
            if (dgvThematicPlans.IsCurrentCellInEditMode)
            {
                dgvThematicPlans.EndEdit();
            }

            RemoveSectionOrTopicHandler?.Invoke(sender, e);
        }

        private void ThematicPlansCellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            ThematicPlanEndEditHandler?.Invoke(sender, e);
            dgvThematicPlans.Refresh();
        }

        private void ThematicPlansCellParsing(object sender, DataGridViewCellParsingEventArgs e)
        {
            if (!e.DesiredType.Equals(typeof(double)))
            {
                return;
            }

            var value = e.Value.ToString();

            if (value.Contains("."))
            {
                var parsingValue = value.Replace(".", ",");

                if (double.TryParse(parsingValue, out double numberValue))
                {
                    e.Value = numberValue;
                }
                else
                {
                    e.Value = 0.0;
                }
            }

            e.ParsingApplied = true;
        }

        private void ViewLessonTypesSelectExecute(object sender, EventArgs e)
        {
            LessonTypesSelectHandler?.Invoke(sender, e);
        }

        private void ThematicContentsCellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvThematicContents.Columns[e.ColumnIndex].Name.Equals("Содержание"))
            {
                ThematicContentValue = dgvThematicContents[e.ColumnIndex, e.RowIndex].Value as string;
                ThematicContentEndEditHandler?.Invoke(sender, e);
            }
        }

        private void IndependentWorkOverviewClick(object sender, EventArgs e)
        {
            var dialogResult = independentWorkOpenFileDialog.ShowDialog();

            if (dialogResult.Equals(DialogResult.OK))
            {
                independentWorkFilePath.Text = independentWorkOpenFileDialog.FileName;
            }
        }

        private void IsIndependentWorkLoadFromWordCheckedChanged(object sender, EventArgs e)
        {
            independentWorkOverview.Enabled = isIndependentWorkLoadFromWord.Checked;
        }

        private void IsIndependentWorkInsertTextCheckedChanged(object sender, EventArgs e)
        {
            independentWorkText.Enabled = isIndependentWorkInsertText.Checked;
        }

        private void IsCheckWorkLoadFromWordCheckedChanged(object sender, EventArgs e)
        {
            checkWorkOverview.Enabled = isCheckWorkLoadFromWord.Checked;
        }

        private void IsCheckWorkInsertionTextCheckedChanged(object sender, EventArgs e)
        {
            checkWorkText.Enabled = isCheckWorkInsertionText.Checked;
        }

        private void CheckWorkOverviewClick(object sender, EventArgs e)
        {
            var dialogResult = checkWorkOpenFileDialog.ShowDialog();

            if (dialogResult.Equals(DialogResult.OK))
            {
                checkWorkFilePath.Text = checkWorkOpenFileDialog.FileName;
            }
        }

        private void IsAssessmentLoadFromWordCheckedChanged(object sender, EventArgs e)
        {
            assessmentOverview.Enabled = isAssessmentLoadFromWord.Checked;
        }

        private void IsAssessmentInsertionTextCheckedChanged(object sender, EventArgs e)
        {
            assessmentInsertText.Enabled = isAssessmentInsertionText.Checked;
        }

        private void AssessmentOverviewClick(object sender, EventArgs e)
        {
            var dialogResult = assessmentOpenFileDialog.ShowDialog();

            if (dialogResult.Equals(DialogResult.OK))
            {
                assessmentFilePath.Text = assessmentOpenFileDialog.FileName;
            }
        }

        private void OnCloseClick(object sender, EventArgs e)
        {
            var dialogResult = Controller.MessageService.ShowQuestion("Вы действительно хотите закрыть РПД без сохранения?", "Подтверждение");

            if (dialogResult == DialogResult.Yes)
            {
                CloseView();
            }
        }

        private void OnSaveClick(object sender, EventArgs e)
        {
            SaveClickHandler?.Invoke(sender, e);
        }

        private void TotalLecturesTextChanged(object sender, EventArgs e)
        {
            var total = double.Parse(TotalLectures);
            var planTotal = double.Parse(TotalPlanLectures);
            lbTotalLectures.ForeColor = (total > planTotal) ? Color.Red : Color.Black;
        }

        private void TotalPracticeTextChanged(object sender, EventArgs e)
        {
            var total = double.Parse(TotalPractices);
            var planTotal = double.Parse(TotalPlanPractices);
            lbTotalPractices.ForeColor = (total > planTotal) ? Color.Red : Color.Black;
        }

        private void TotalLabsTextChanged(object sender, EventArgs e)
        {
            var total = double.Parse(TotalLabs);
            var planTotal = double.Parse(TotalPlanLabs);
            lbTotalLabs.ForeColor = (total > planTotal) ? Color.Red : Color.Black;
        }

        private void TotalIndividualWorksTextChanged(object sender, EventArgs e)
        {
            var total = double.Parse(TotalIndividualWorks);
            var planTotal = double.Parse(TotalPlanIndividualWorks);
            lbTotalIndividualWorks.ForeColor = (total > planTotal) ? Color.Red : Color.Black;
        }

        private void UpdateContextItems(object sender, EventArgs e)
        {
            if (ActiveControl is TextBoxBase textBox)
            {
                var isTextBoxEmpty = string.IsNullOrEmpty(textBox.SelectedText);

                var copyItemName = nameof(copyItem);
                contextMenuStrip1.Items[copyItemName].Enabled = !isTextBoxEmpty;

                var cutItemName = nameof(cutItem);
                contextMenuStrip1.Items[cutItemName].Enabled = !isTextBoxEmpty;

                var isClipboardContainsText = Clipboard.ContainsText();

                var insertItemName = nameof(insertItem);
                contextMenuStrip1.Items[insertItemName].Enabled = isClipboardContainsText;
            }
        }

        private void CopyItemClick(object sender, EventArgs e)
        {
            if (ActiveControl is TextBoxBase textBox && !string.IsNullOrEmpty(textBox.SelectedText))
            {
                Clipboard.SetText(textBox.SelectedText);
            }
        }

        private void CutItemClick(object sender, EventArgs e)
        {
            if (ActiveControl is TextBoxBase textBox)
            {
                CopyItemClick(ActiveControl, e);
                var startIndex = textBox.SelectionStart;
                textBox.Text = textBox.Text.Replace(textBox.SelectedText, string.Empty);
                textBox.Select(startIndex, 0);
            }
        }

        private void InsertItemClick(object sender, EventArgs e)
        {
            if (ActiveControl is TextBoxBase textBox)
            {
                var startIndex = textBox.SelectionStart;
                var text = Clipboard.GetText();
                textBox.Text = textBox.Text.Insert(textBox.SelectionStart, text);
                textBox.Select(startIndex + text.Length, 0);
            }
        }

        private void SelectAllItemClick(object sender, EventArgs e)
        {
            if (ActiveControl is TextBoxBase textBox)
            {
                textBox.SelectAll();
            }
        }

        /* Внимание! Добавляйте новые методы перед этим комментарием */

        public void CloseView()
        {
            Close();
        }
    }
}
