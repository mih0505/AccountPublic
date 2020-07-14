using System.Collections.Generic;
using System.Linq;

namespace AccountRPD.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool IsEmpty(this IEnumerable<object> enumerable)
        {
            return enumerable is null || enumerable.Count().Equals(0);
        }

        public static bool Contains(this IEnumerable<object> enumerable, int index)
        {
            return index >= 0 && index < enumerable.Count();
        }
    }
}
