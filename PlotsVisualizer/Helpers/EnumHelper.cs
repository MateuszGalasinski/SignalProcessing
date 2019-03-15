using System;
using System.Collections.Generic;
using System.Linq;

namespace PlotsVisualizer.Helpers
{
    public static class EnumHelper
    {
        public static IEnumerable<string> GetAllValues(Type t)
        {
            if (!t.IsEnum)
                throw new ArgumentException($"{nameof(t)} must be an enum type");

            return Enum.GetValues(t).Cast<Enum>().Select(e => e.ToString()).ToList();
        }
    }
}
