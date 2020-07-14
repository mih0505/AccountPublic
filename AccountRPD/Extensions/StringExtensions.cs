using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountRPD.Extensions
{
    public static class StringExtensions
    {
        public static string Update(this string value)
        {
            var charArray = value.ToCharArray();
            return new string(charArray);
        }
    }
}
