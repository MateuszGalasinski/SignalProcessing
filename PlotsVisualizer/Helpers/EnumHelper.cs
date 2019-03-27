using SignalProcessing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlotsVisualizer.Helpers
{
    public static class SignalTypeEnumHelper
    {
        public static IEnumerable<string> GetAllValues(Type t)
        {
            if (t != typeof(Types.SignalType))
                throw new ArgumentException($"{nameof(t)} must be an enum type");

            return Enum.GetValues(t).Cast<Types.SignalType>().Where(s => s != Types.SignalType.Composed).Select(e => e.ToString()).ToList();
        }
    }
}
