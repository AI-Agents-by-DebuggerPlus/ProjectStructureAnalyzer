using System;
using System.Globalization;
using System.Windows.Data;

namespace ProjectStructureAnalyzer.Converters
{
    public class DirectoryToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isDirectory)
            {
                return isDirectory ? "📁" : "📄";
            }
            return "📄"; // Default to file icon
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}