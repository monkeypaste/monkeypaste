using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
//using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            //handles conversion for:
            //byte[]
            //SKBitmap
            //ImageSource

            //parameter can specify:
            //output dimenision
            if (value != null) {
                object outObject = null;
                Size outSize = Size.Zero;

                if (parameter != null) {
                    if (parameter is Size) {
                        outSize = (Size)parameter;
                    } else if (parameter is object[]) {
                        foreach (var obj in (object[])parameter) {
                            if (obj is Size) {
                                outSize = (Size)obj;
                            }
                        }
                    }

                }
                if (value is byte[] fromBytes) {
                    if (targetType == typeof(byte[])) {
                        //byte[]->byte[]
                        outObject = fromBytes;
                    } else if (targetType == typeof(SKBitmap)) {
                        //byte[]->SkBitmap
                        using (var ms = new MemoryStream(fromBytes)) {
                            ms.Seek(0, SeekOrigin.Begin);
                            using (var inputStream = new SKManagedStream(ms)) {
                                inputStream.Seek(0);
                                outObject = SKBitmap.Decode(inputStream);
                            }
                        }
                    } else if (targetType == typeof(ImageSource)) {
                        //byte[]->ImageSource
                        using (var stream = new MemoryStream(fromBytes)) {
                            outObject = ImageSource.FromStream(() => stream);
                        }
                    }
                } else if (value is SKBitmap fromSkBitmap) {
                    if (targetType == typeof(SKBitmap)) {
                        //SKBitmap->SkBitmap
                        outObject = fromSkBitmap;
                    } else if (targetType == typeof(byte[])) {
                        //SkBitmap->byte[]
                        outObject = fromSkBitmap.Bytes;
                    } else if (targetType == typeof(ImageSource)) {
                        //SKBitmap->ImageSource
                        using var skImg = SKImage.FromBitmap(fromSkBitmap);
                        // encode the image (defaults to PNG)
                        using var encoded = skImg.Encode();
                        // get a stream over the encoded data
                        using var stream = encoded.AsStream();
                        outObject = ImageSource.FromStream(() => stream);
                    }
                } else if (value is ImageSource fromImageSource) {
                    if (targetType == typeof(ImageSource)) {
                        //ImageSource->ImageSource
                        outObject = fromImageSource;
                    } else if (targetType == typeof(byte[])) {
                    } else if (targetType == typeof(ImageSource)) {
                    }
                }

            }
            return null;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }

    }

}
    
