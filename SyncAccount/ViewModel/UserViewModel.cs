using System;
using System.Collections.Generic;

namespace SyncAccount.ViewModel
{
    public class UserViewModel
    {
        public int? DecanatId { get; set; }
        public string Lastname { get; set; }
        public string Firstname { get; set; }
        public string Middlename { get; set; }
        public int? idGroupDecanat { get; set; }
        public int? idFacultyDecanat { get; set; }
        public string UserName { get; set; }
        public string NumberOfRecordBook { get; set; }
        public bool Sex { get; set; }
        public string Bases { get; set; }
        public DateTime? DateBlocked { get; set; }
        public string Email { get; set; }
        public string GroupName { get; set; }
        public DateTime? BirthDate { get; set; }
    }

    class StudentComparer : IEqualityComparer<UserViewModel>
    {
        // Products are equal if their names and product numbers are equal.
        public bool Equals(UserViewModel x, UserViewModel y)
        {
            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the products' properties are equal.
            return x.DecanatId == y.DecanatId && x.Firstname == y.Firstname && x.idFacultyDecanat == y.idFacultyDecanat
                && x.idGroupDecanat == y.idGroupDecanat && x.Lastname == y.Lastname && x.Middlename == y.Middlename && x.NumberOfRecordBook == y.NumberOfRecordBook
                && x.Sex == y.Sex && x.UserName == y.UserName && x.Bases == y.Bases && x.BirthDate == y.BirthDate;
        }

        // If Equals() returns true for a pair of objects 
        // then GetHashCode() must return the same value for these objects.

        public int GetHashCode(UserViewModel student)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(student, null)) return 0;

            //Get hash code for the Name field if it is not null.
            int hashDecanatId = student.DecanatId.GetHashCode();
            int hashFirstname = student.Firstname.GetHashCode();
            int hashIdFacultyDecanat = student.idFacultyDecanat.GetHashCode();
            int hashIdGroupDecanat = student.idGroupDecanat.GetHashCode();
            int hashLastname = student.Lastname.GetHashCode();
            int hashMiddlename = 0;
            if (student.Middlename != null)
                hashMiddlename = student.Middlename.GetHashCode();
            int hashNumberOfRecordBook = student.NumberOfRecordBook.GetHashCode();
            int hashSex = student.Sex.GetHashCode();
            int hashBases = student.Bases.GetHashCode();
            int hashUserName = student.UserName.GetHashCode();
            int hashBirthDate = 0;
            if (student.BirthDate != null)
                hashBirthDate = student.BirthDate.GetHashCode();
            //Calculate the hash code for the product.
            if (student.Middlename != null && student.BirthDate != null)
                return hashDecanatId ^ hashFirstname ^ hashIdFacultyDecanat ^ hashIdGroupDecanat ^ hashBirthDate
                    ^ hashLastname ^ hashMiddlename ^ hashNumberOfRecordBook ^ hashSex ^ hashBases ^ hashUserName;
            if (student.Middlename != null && student.BirthDate == null)
                return hashDecanatId ^ hashFirstname ^ hashIdFacultyDecanat ^ hashIdGroupDecanat 
                    ^ hashLastname ^ hashMiddlename ^ hashNumberOfRecordBook ^ hashSex ^ hashBases ^ hashUserName;
            if (student.Middlename == null && student.BirthDate != null)
                return hashDecanatId ^ hashFirstname ^ hashIdFacultyDecanat ^ hashIdGroupDecanat
                    ^ hashLastname ^ hashBirthDate ^ hashNumberOfRecordBook ^ hashSex ^ hashBases ^ hashUserName;
            else
                return hashDecanatId ^ hashFirstname ^ hashIdFacultyDecanat ^ hashIdGroupDecanat
                    ^ hashLastname ^ hashNumberOfRecordBook ^ hashSex ^ hashBases ^ hashUserName;
        }

    }

    class StudentComparerMoodle : IEqualityComparer<UserViewModel>
    {
        // Products are equal if their names and product numbers are equal.
        public bool Equals(UserViewModel x, UserViewModel y)
        {
            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the products' properties are equal.
            bool result = x.DecanatId == y.DecanatId && x.Firstname == y.Firstname && x.Lastname == y.Lastname
                            && x.Middlename == y.Middlename && x.Email == y.Email && x.GroupName == y.GroupName
                            && x.BirthDate == y.BirthDate;
            return result;
        }

        // If Equals() returns true for a pair of objects 
        // then GetHashCode() must return the same value for these objects.

        public int GetHashCode(UserViewModel student)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(student, null)) return 0;

            //Get hash code for the Name field if it is not null.            
            int hashDecanatId = student.DecanatId.GetHashCode();
            int hashFirstname = student.Firstname.GetHashCode();
            int hashLastname = student.Lastname.GetHashCode();
            int hashEmail = student.Email.GetHashCode();
            int hashGroupName = student.GroupName.GetHashCode();
            int hashMiddlename = 0;            
            if (student.Middlename != null)
                hashMiddlename = student.Middlename.GetHashCode();
            //Calculate the hash code for the product.
            if (student.Middlename != null)
                return hashDecanatId ^ hashFirstname ^ hashLastname ^ hashMiddlename ^ hashEmail ^ hashGroupName;
            else
                return hashDecanatId ^ hashFirstname ^ hashLastname ^ hashEmail ^ hashGroupName;
        }
    }
}
