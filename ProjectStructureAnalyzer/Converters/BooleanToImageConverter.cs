using System;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ProjectStructureAnalyzer
{
    public class BooleanToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool isDirectory)
            {
                string imagePath = isDirectory ? "/Images/folder.png" : "/Images/file.png";
                return new BitmapImage(new Uri(imagePath, UriKind.Relative));
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}