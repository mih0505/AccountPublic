using System;
using System.Collections.Generic;

namespace SyncAccount.ViewModel
{
    public class DepartmentViewModel
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Boss { get; set; }
        public int? DecanatID { get; set; }
        public int? FacultyId { get; set; }
        public int? Number { get; set; }
    }

    class DepartmentComparer : IEqualityComparer<DepartmentViewModel>
    {
        // Products are equal if their names and product numbers are equal.
        public bool Equals(DepartmentViewModel x, DepartmentViewModel y)
        {
            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the products' properties are equal.
            return x.Name == y.Name && x.ShortName == y.ShortName
                && x.Boss == y.Boss && x.DecanatID == y.DecanatID
                 && x.FacultyId == y.FacultyId && x.Number == y.Number;
        }

        public int GetHashCode(DepartmentViewModel Department)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(Department, null)) return 0;

            //Get hash code for the Name field if it is not null. 
            int hashName = Department.Name.GetHashCode();
            int hashShortName = Department.ShortName.GetHashCode();
            int hashBoss = Department.Boss.GetHashCode();            
            int hashDecanatID = Department.DecanatID.GetHashCode();
            int hashFacultyId = Department.FacultyId.GetHashCode();
            int hashNumber = Department.Number.GetHashCode();

            //Calculate the hash code for the product.            
            return hashName ^ hashShortName ^ hashBoss ^ hashDecanatID ^ hashFacultyId ^ hashNumber;
        }
    }
}