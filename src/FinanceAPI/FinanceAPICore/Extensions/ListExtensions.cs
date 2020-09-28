using System.Collections.Generic;
using System.Linq;

namespace FinanceAPICore.Extensions
{
    public static class ListExtensions
    {
        public static T FirstOrDefault<T>(this IEnumerable<T> source, T alternate)
        {
            try
            {
                return source.First();
            }
            catch
            {
                return alternate;
            }
        }
    }
}