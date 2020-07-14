using System;
using System.Collections.Generic;

namespace SyncAccount.ViewModel
{
    public class GroupViewModel
    {
        public string Name { get; set; }
        public int? DecanatID { get; set; }
        public int? ProfileId { get; set; }
        public int? Course { get; set; }
        public int? FacultyId { get; set; }
        public string AcademicYear { get; set; }
        public int? FormOfTrainingId { get; set; }
        public int? idPlanDecanat { get; set; }
        public string PlanNameDecanat { get; set; }
        public double? Period { get; set; }
    }

    class GroupComparer : IEqualityComparer<GroupViewModel>
    {
        // Products are equal if their names and product AcademicYears are equal.
        public bool Equals(GroupViewModel x, GroupViewModel y)
        {
            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the products' properties are equal.
            return x.Name == y.Name && x.ProfileId == y.ProfileId
                && x.Course == y.Course && x.DecanatID == y.DecanatID
                 && x.FacultyId == y.FacultyId && x.AcademicYear == y.AcademicYear
                 && x.FormOfTrainingId == y.FormOfTrainingId && x.idPlanDecanat == y.idPlanDecanat
                 && x.PlanNameDecanat == y.PlanNameDecanat && x.Period == y.Period;
        }

        public int GetHashCode(GroupViewModel group)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(group, null)) return 0;

            //Get hash code for the Name field if it is not null. 
            int hashName = group.Name.GetHashCode();
            int hashProfileId = group.ProfileId.GetHashCode();
            int hashCourse = group.Course.GetHashCode();            
            int hashDecanatID = group.DecanatID.GetHashCode();
            int hashFacultyId = group.FacultyId.GetHashCode();
            int hashAcademicYear = group.AcademicYear.GetHashCode();
            int hashFormOfTrainingId = group.FormOfTrainingId.GetHashCode();
            int hashIdPlanDecanat = group.idPlanDecanat.GetHashCode();
            int hashPlanNameDecanat = (group.PlanNameDecanat != null) ? group.PlanNameDecanat.GetHashCode() : String.Empty.GetHashCode(); ;
            int hashPeriod = group.Period.GetHashCode();
            //Calculate the hash code for the product.            
            return hashName ^ hashProfileId ^ hashCourse ^ hashDecanatID ^ hashFacultyId 
                ^ hashAcademicYear ^ hashFormOfTrainingId ^ hashIdPlanDecanat 
                ^ hashPlanNameDecanat ^ hashPeriod;
        }
    }
}