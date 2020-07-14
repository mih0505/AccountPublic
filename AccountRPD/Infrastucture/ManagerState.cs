using AccountRPD.Interfaces;

namespace AccountRPD.Infrastucture
{
    public class ManagerState : IManagerState
    {
        public int StudyYearIndex { get; set; }

        public int DepartmentIndex { get; set; }

        public int DisciplineIndex { get; set; }

        public int PlanIndex { get; set; }

        public int RPDIndex { get; set; }

        public int LastIndexAtRPDs { get; set; }

        public bool IsFirstLoad { get; set; } = true;

        public bool HideNotActualPlans { get; set; } = false;

        public string FilterText { get; set; }
    }
}
