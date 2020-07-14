namespace AccountRPD.Interfaces
{
    public interface IManagerState
    {
        int StudyYearIndex { get; set; }

        int DepartmentIndex { get; set; }

        int DisciplineIndex { get; set; }

        int PlanIndex { get; set; }

        int RPDIndex { get; set; }

        int LastIndexAtRPDs { get; set; }

        bool IsFirstLoad { get; set; }

        bool HideNotActualPlans { get; set; }

        string FilterText { get; set; }
    }
}
