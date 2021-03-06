﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace PlotsVisualizer.Helpers
{
    [ValueConversion(typeof(Enum), typeof(IEnumerable<string>))]
    public class EnumToCollectionConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return TypeEnumHelper.GetAllValues(value?.GetType());
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }

    public static class TypeEnumHelper
    {
        public static IEnumerable<string> GetAllValues(Type t)
        {
            if (!t.IsEnum)
                throw new ArgumentException($"{nameof(t)} must be an enum type");
            List<string> valueNames = new List<string>();
            foreach (var value in Enum.GetValues(t))
            {
                valueNames.Add(value.ToString());
            }

            return valueNames;
        }
    }
}
