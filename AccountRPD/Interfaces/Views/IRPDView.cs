using Account.DAL.Entities;
using AccountRPD.BL.DTOs;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace AccountRPD.Interfaces.Views
{
    public interface IRPDView : IView
    {
        string RPDName { get; set; }
        string FacultyTitle { get; set; }
        string DepartmentTitle { get; set; }
        string DisciplineTitle { get; set; }
        string ProfileTitle { get; set; }
        string DepartmentChief { get; set; }
        string Results { get; set; }
        string DisciplinePlace { get; set; }
        DateTime ApprovalDateRPD { get; set; }
        string ProtocolNumberRPD { get; set; }
        IEnumerable<Member> MembersList { get; set; }
        IDictionary<DecanatCompetence, Competence> Competences { get; set; }
        KeyValuePair<string, string>[] HoursDivision { get; set; }
        TreeNode SelectedCompetenceNode { get; }
        Competence SelectedCompetence { get; set; }
        string CompetenceName { get; set; }
        string CompetenceStage { get; set; }
        IEnumerable<ThematicPlan> ThematicPlans { get; set; }
        ThematicPlan SelectedThematicPlan { get; }
        IDictionary<string, IEnumerable<ThematicContent>> ThematicContentsDictionary { get; set; }
        IEnumerable<ThematicContent> ThematicContents { get; set; }
        string ThematicContentValue { get; }
        ThematicContent SelectedThematicContent { get; }
        IEnumerable<string> LessonsTypes { get; set; }
        string SelectedLessonType { get; set; }
        IDictionary<DecanatCompetence, IEnumerable<CompetenceGrade>> CompetenceGrades { get; set; }
        IEnumerable<CompetenceGrade> SelectedCompetenceGrades { get; }

        string TotalLectures { get; set; }
        string TotalPractices { get; set; }
        string TotalLabs { get; set; }
        string TotalIndividualWorks { get; set; }
        string TotalPlanLectures { get; set; }
        string TotalPlanPractices { get; set; }
        string TotalPlanLabs { get; set; }
        string TotalPlanIndividualWorks { get; set; }

        bool IsIndependentWorkLoadFromWord { get; set; }
        bool IsIndependentWorkInsertText { get; set; }
        string IndependentWorkInsertionFilePath { get; set; }
        string IndependentWorkInsertionText { get; set; }
        bool IsCheckWorkLoadFromWord { get; set; }
        bool IsCheckWorkInsertText { get; set; }
        string CheckWorkInsertionFilePath { get; set; }
        string CheckWorkInsertionText { get; set; }
        bool IsAssessmentLoadFromWord { get; set; }
        bool IsAssessmentInsertText { get; set; }
        string AssessmentInsertionFilePath { get; set; }
        string AssessmentInsertionText { get; set; }

        bool CanEditState { get; set; }
        bool CanEditAuthor { get; set; }

        IEnumerable<BasicLiterature> BasicLiteratures { get; set; }
        IEnumerable<AdditionalLiterature> AdditionalLiteratures { get; set; }
        IEnumerable<LibrarySystem> LibrarySystems { get; set; }
        IEnumerable<InternetResource> InternetResources { get; set; }
        IEnumerable<License> Licenses { get; set; }
        IEnumerable<MaterialBase> MaterialBases { get; set; }

        IEnumerable<TeacherDTO> Authors { get; set; }
        TeacherDTO SelectedAuthor { get; set; }

        IEnumerable<string> RPDTypes { get; set; }
        string SelectedRPDType { get; set; }

        int EducationStandardId { get; set; }

        event EventHandler ViewInitializedHandler;
        event EventHandler DisciplineTitleTextChangedHandler;
        event EventHandler CompetenceNameBeforeSelectHandler;
        event EventHandler CompetenceStageBeforeSelectHandler;
        event TreeViewEventHandler CompetenceSelectionChangedHandler;
        event EventHandler AddSectionHandler;
        event EventHandler AddTopicHandler;
        event EventHandler RemoveSectionOrTopicHandler;
        event DataGridViewCellEventHandler ThematicPlanEndEditHandler;
        event EventHandler LessonTypesSelectHandler;
        event DataGridViewCellEventHandler ThematicContentEndEditHandler;
        event EventHandler SaveClickHandler;
    }
}