using System;
using Microsoft.UI.Xaml.Data;

namespace StudentManagementApp.Converters
{
    public class DateTimeToDateTimeOffsetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime dt)
            {
                // If the date is the default DateTime, return the DateTimeOffset's minimum value.
                if (dt == DateTime.MinValue)
                {
                    return DateTimeOffset.MinValue;
                }
                return new DateTimeOffset(dt);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTimeOffset dto)
            {
                return dto.DateTime;
            }
            return value;
        }
    }
}
