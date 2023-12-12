using System.Collections.Generic;

namespace Extensions
{
    public static class ListExtensions
    {
        public static bool AddIfNotNull<T>(this List<T> list, T? value) where T : struct {
            if (!value.HasValue)
            {
                return false;
            }
            
            list.Add(value.Value);
            return true;
        }
    }
}