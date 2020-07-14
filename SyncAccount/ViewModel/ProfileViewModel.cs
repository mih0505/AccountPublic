using System;
using System.Collections.Generic;

namespace SyncAccount.ViewModel
{
    public class ProfileViewModel
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Boss { get; set; }
        public int? DecanatID { get; set; }
        public int? FacultyId { get; set; }
        public string Code1 { get; set; }
        public int? DirectionOfTrainingId { get; set; }
        public string Qualification { get; set; }
        public int? DepartmentId { get; set; }
        public bool Acceptance { get; set; }
        public double? Period { get; set; }
    }

    class ProfileComparer : IEqualityComparer<ProfileViewModel>
    {
        // Products are equal if their names and product numbers are equal.
        public bool Equals(ProfileViewModel x, ProfileViewModel y)
        {
            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the products' properties are equal.
            return x.Name == y.Name && x.ShortName == y.ShortName
                && x.Boss == y.Boss && x.DecanatID == y.DecanatID
                 && x.FacultyId == y.FacultyId && x.Code1 == y.Code1
                 && x.DirectionOfTrainingId == y.DirectionOfTrainingId && x.Qualification == y.Qualification
                 && x.DepartmentId == y.DepartmentId && x.Acceptance == y.Acceptance && x.Period == y.Period;
        }

        public int GetHashCode(ProfileViewModel Profile)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(Profile, null)) return 0;

            //Get hash code for the Name field if it is not null. 
            int hashName = Profile.Name.GetHashCode();
            int hashShortName = Profile.ShortName.GetHashCode();
            int hashBoss = Profile.Boss.GetHashCode();            
            int hashDecanatID = Profile.DecanatID.GetHashCode();
            int hashFacultyId = Profile.FacultyId.GetHashCode();
            int hashDirectionOfTrainingId = Profile.DirectionOfTrainingId.GetHashCode();
            int hashCode1 = Profile.Code1.GetHashCode();
            int hashQualification = Profile.Qualification.GetHashCode();
            int hashDepartmentId = Profile.DepartmentId.GetHashCode();
            int hashAcceptance = Profile.Acceptance.GetHashCode();
            int hashPeriod = Profile.Period.GetHashCode();
            //Calculate the hash code for the product.            
            return hashName ^ hashShortName ^ hashBoss ^ hashDecanatID ^ hashFacultyId ^ hashCode1 ^
                hashDirectionOfTrainingId ^ hashQualification ^ hashDepartmentId ^ hashAcceptance ^ hashPeriod;
        }
    }
}