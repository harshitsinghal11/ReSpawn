using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ReSpawn.Helpers
{
    public class StringToImageSourceConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            try
            {
                string? path = value as string;
                if (string.IsNullOrEmpty(path)) return GetFallback();
                if (!File.Exists(path)) return GetFallback();

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.EndInit();
                return bitmap;
            }
            catch
            {
                return GetFallback();
            }
        }

        private static BitmapImage GetFallback()
        {
            try
            {
                return new BitmapImage(new Uri(
                    "pack://application:,,,/Assets/default-icon.png"));
            }
            catch { return new BitmapImage(); }
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}