using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiGen
{
    public static class ExtensionMethods
    {
        public static T? FirstOrDefaultNullable<T>(this IEnumerable<T> source) where T : struct
        {
            foreach (var item in source)
                return item;
            return null;
        }

        public static T? LastOrDefaultNullable<T>(this IEnumerable<T> source) where T : struct
        {
            T? last = null;
            foreach (var item in source)
                last = item;
            return last;
        }
    }
}
