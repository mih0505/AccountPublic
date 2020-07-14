using Account.DAL.Entities;
using AccountRPD.Enums;

namespace AccountRPD.Extensions
{
    public static class ObjectExtensions
    {
        public static SelectionTypes GetSelectionType(this object value)
        {
            var selectionType = SelectionTypes.Undefined;

            if (value is string)
            {
                selectionType = SelectionTypes.StudyYear;
            }
            else if (value is DecanatDepartment)
            {
                selectionType = SelectionTypes.Department;
            }
            else if (value is DecanatDiscipline)
            {
                selectionType = SelectionTypes.Discipline;
            }
            else if (value is DecanatPlan)
            {
                selectionType = SelectionTypes.Plan;
            }
            else if (value is RPD)
            {
                selectionType = SelectionTypes.RPD;
            }

            return selectionType;
        }
    }
}
