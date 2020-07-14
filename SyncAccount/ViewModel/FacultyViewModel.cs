using System;
using System.Collections.Generic;

namespace SyncAccount.ViewModel
{
    public class FacultyViewModel
    {
        public string Name { get; set; }
        public string AliasFaculty { get; set; }
        public string Boss { get; set; }
        public int? DecanatID { get; set; }
    }

    class FacultyComparer : IEqualityComparer<FacultyViewModel>
    {
        // Products are equal if their names and product numbers are equal.
        public bool Equals(FacultyViewModel x, FacultyViewModel y)
        {
            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the products' properties are equal.
            return x.Name == y.Name && x.AliasFaculty == y.AliasFaculty
                && x.Boss == y.Boss && x.DecanatID == y.DecanatID;
        }

        public int GetHashCode(FacultyViewModel faculty)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(faculty, null)) return 0;

            //Get hash code for the Name field if it is not null. 
            int hashName = faculty.Name.GetHashCode();
            int hashAliasFaculty = faculty.AliasFaculty.GetHashCode();
            int hashBoss = faculty.Boss.GetHashCode();            
            int hashDecanatID = faculty.DecanatID.GetHashCode();

            //Calculate the hash code for the product.            
            return hashName ^ hashAliasFaculty ^ hashBoss ^ hashDecanatID;
        }
    }
}