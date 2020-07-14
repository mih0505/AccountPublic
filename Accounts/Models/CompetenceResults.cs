using System;

namespace Accounts.Models
{
    public class CompetenceResults : IEquatable<CompetenceResults>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Result { get; set; }

        public bool Equals(CompetenceResults other)
        {
            if (Code == other.Code && Name == other.Name)
                return true;

            return false;
        }

        public override int GetHashCode()
        {
            int hashCode = Code == null ? 0 : Code.GetHashCode();
            int hashName = Name == null ? 0 : Name.GetHashCode();

            return hashCode ^ hashName;
        }
    }
}