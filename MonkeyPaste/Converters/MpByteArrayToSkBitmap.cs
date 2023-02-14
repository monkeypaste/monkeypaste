using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
//using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpByteArrayToSkBitmap : IValueConverter {

        public byte[] Image2Byte(SKBitmap bitmap) {
            var temp = bitmap.Bytes;

            //var bm = SKBitmap.Decode(temp);

            return temp;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value == null || value.GetType() != typeof(byte[])) {
                Console.WriteLine(@"MpByteArrayToSkBitmap.Convert Error value is not byte[]");
                return null;
            }
            var byteArrayIn = value as byte[];
            using (var ms = new MemoryStream(byteArrayIn)) {
                ms.Seek(0, SeekOrigin.Begin);
                using (var inputStream = new SKManagedStream(ms)) {
                    inputStream.Seek(0);
                    var bmp = SKBitmap.Decode(inputStream);
                    return bmp;
                }
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null || value.GetType() != typeof(SKBitmap)) {
                Console.WriteLine(@"MpByteArrayToSkBitmap.ConvertBack Error value is not SKBitmap");
                return null;
            }

            return (value as SKBitmap).Bytes;
        }
    }
}
    
