using Account.DAL.Entities;
using AccountRPD.EventArguments;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace AccountRPD.Interfaces.Views
{
    public interface IManagerView : IView
    {
        IEnumerable<string> StudyYears { get; set; }
        string SelectedStudyYear { get; set; }
        int SelectedStudyYearIndex { get; set; }
        IEnumerable<DecanatDepartment> Departments { get; set; }
        DecanatDepartment SelectedDepartment { get; }
        int SelectedDepartmentIndex { get; set; }
        IEnumerable<DecanatDiscipline> Disciplines { get; set; }
        DecanatDiscipline SelectedDiscipline { get; }
        int SelectedDisciplineIndex { get; set; }
        string FilterText { get; set; }
        IEnumerable<DecanatPlan> Plans { get; set; }
        DecanatPlan SelectedPlan { get; }
        int SelectedPlanIndex { get; set; }
        bool HideNotActualPlans { get; set; }
        IEnumerable<RPD> RPDs { get; set; }
        RPD SelectedRPD { get; }
        int SelectedRPDIndex { get; set; }
        string AssessmentFilePath { get; }
        string RPDFilePath { get; }

        event EventHandler InitializedHandler;
        event EventHandler<SelectionChangedEventArgs> SelectionChangedHandler;
        event EventHandler<PermissionCheckEventArgs> PermissionCheckHandler;
        event EventHandler DisciplineFilterClearedHandler;
        event EventHandler RPDCreateHandler;
        event EventHandler RPDEditHandler;
        event CancelEventHandler RPDRemoveHandler;
        event CancelEventHandler RPDExportHandler;
        event CancelEventHandler AssessmentExportHandler;
        event EventHandler ClosingHandler;
    }
}
