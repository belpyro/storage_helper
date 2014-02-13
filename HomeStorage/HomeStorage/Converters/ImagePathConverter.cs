using System;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Data;
using Microsoft.Phone;

namespace HomeStorage.Converters
{
    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                MemoryStream memStream;

                if (value == null || string.IsNullOrEmpty(value.ToString()) || !myIsolatedStorage.FileExists(value.ToString()))
                {
                    return PictureDecoder.DecodeJpeg(
                        Application.GetResourceStream(new Uri("Assets/default.jpg", UriKind.Relative)).Stream, 256, 256);
                }

                using (var file = myIsolatedStorage.OpenFile(value.ToString(), FileMode.Open))
                {
                    memStream = new MemoryStream((int)file.Length);
                    file.CopyTo(memStream, 2000);
                    memStream.Seek(0, SeekOrigin.Begin);
                }  

                return memStream.Length <= 0 ? PictureDecoder.DecodeJpeg(
                    Application.GetResourceStream(new Uri("Assets/default.jpg", UriKind.Relative)).Stream, 256, 256) : PictureDecoder.DecodeJpeg(memStream, 256, 256);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
