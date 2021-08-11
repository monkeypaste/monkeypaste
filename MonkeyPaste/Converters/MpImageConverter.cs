using FFImageLoading.Forms;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpImageConverter : IValueConverter {
        public object Convert(object value, Type targetType = null, object parameter = null, CultureInfo culture = null) {
            //handles conversion for:
            //byte[]
            //string(base64)
            //SKBitmap
            //ImageSource
            //layered list of types
            //parameter can specify: size, quality, tint, layer settings

            if (value != null) {
                #region Gather Parameters
                Size outSize = Size.Zero;
                SKFilterQuality outQuality = SKFilterQuality.Medium;
                if (parameter != null) {
                    if (parameter is Size) {
                        outSize = (Size)parameter;
                    } else if (parameter is SKFilterQuality) {
                        outQuality = (SKFilterQuality)parameter;
                    } else if (parameter is object[]) {
                        foreach (var obj in (object[])parameter) {
                            if (obj is Size) {
                                outSize = (Size)obj;
                            }
                            if (obj is SKFilterQuality) {
                                outQuality = (SKFilterQuality)obj;
                            }
                        }
                    }
                }
                #endregion

                #region Create outObject
                object outObject = null;
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
                    } else if (targetType == typeof(CachedImage)) {
                        var imgSrc = (ImageSource)Convert(fromBytes, typeof(ImageSource), parameter);
                        outObject = new CachedImage() { Source = imgSrc };
                    } else if (targetType == typeof(string)) {
                        //byte[]->string
                        outObject = System.Convert.ToBase64String(fromBytes);
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
                        using var encoded = skImg.Encode();
                        using var stream = encoded.AsStream();
                        outObject = ImageSource.FromStream(() => stream);
                    } else if (targetType == typeof(string)) {
                        //SKBitmap->string
                        var bytes = (byte[])Convert(fromSkBitmap, typeof(byte[]), parameter);
                        outObject = System.Convert.ToBase64String(bytes);
                    }
                } else if (value is ImageSource fromImageSource) {
                    if (targetType == typeof(ImageSource)) {
                        //ImageSource->ImageSource
                        outObject = fromImageSource;
                    } else if (targetType == typeof(byte[])) {
                        //ImageSource->byte[]
                        if (value is StreamImageSource fromStreamImageSource) {
                            //StreamImageSource->byte[]
                            var streamFromImageSource = fromStreamImageSource.Stream(CancellationToken.None).Result;

                            if (streamFromImageSource == null) {
                                return null;
                            }
                            using var memoryStream = new MemoryStream();
                            streamFromImageSource.CopyTo(memoryStream);

                            outObject = memoryStream.ToArray();
                        } else {
                            //ImageSource->byte[]
                            var skBmp = Convert(fromImageSource, typeof(SKBitmap), parameter);
                            outObject = Convert(skBmp, typeof(byte[]), parameter);
                        }
                    } else if (targetType == typeof(SKBitmap)) {
                        //ImageSource->SKBitmap
                        //SKBitmapImageSource skBmpImgSrc = (SKBitmapImageSource)fromImageSource;
                        var bytes = (byte[])Convert(fromImageSource, typeof(byte[]), parameter);
                        var skBmp = new SKBitmap();
                        // pin the managed array so that the GC doesn't move it
                        var gcHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                        // install the pixels with the color type of the pixel data
                        var info = new SKImageInfo(2, 2, SKImageInfo.PlatformColorType, SKAlphaType.Unpremul);
                        skBmp.InstallPixels(info, gcHandle.AddrOfPinnedObject(), info.RowBytes, delegate { gcHandle.Free(); }, null);
                        outObject = skBmp;
                    } else if (targetType == typeof(SKBitmap)) {
                        //ImageSource->string
                        var bytes = (byte[])Convert(fromImageSource, typeof(byte[]), parameter);
                        outObject = System.Convert.ToBase64String(bytes);
                    }
                } else if(value is string fromStringBase64) {
                    if(targetType == typeof(ImageSource)) {
                        //string->ImageSource
                        var bytes = System.Convert.FromBase64String(fromStringBase64);
                        var stream = new MemoryStream(bytes);
                        outObject = ImageSource.FromStream(() => stream);
                    } else if(targetType == typeof(SKBitmap)) {
                        //string->SKBitmap
                        var imgSrc = Convert(fromStringBase64, typeof(ImageSource), parameter);
                        outObject = Convert(imgSrc, typeof(SKBitmap), parameter);
                    } else if (targetType == typeof(byte[])) {
                        //string->byte[]
                        var imgSrc = Convert(fromStringBase64, typeof(ImageSource), parameter);
                        outObject = Convert(imgSrc, typeof(byte[]), parameter);
                    }

                }
                #endregion

                #region Process Parameters
                // TODO add resizing, quality, tinting, layer etc. either here or in output creation
                #endregion

                return outObject;
            }
            return null;
        }


        public async Task<object> ConvertAsync(object value, Type targetType = null, object parameter = null, CultureInfo culture = null) {
            //handles conversion for:
            //byte[]
            //string(base64)
            //SKBitmap
            //ImageSource
            //layered list of types
            //parameter can specify: size, quality, tint, layer settings

            if (value != null) {
                #region Gather Parameters
                Size outSize = Size.Zero;
                SKFilterQuality outQuality = SKFilterQuality.Medium;
                if (parameter != null) {
                    if (parameter is Size) {
                        outSize = (Size)parameter;
                    } else if (parameter is SKFilterQuality) {
                        outQuality = (SKFilterQuality)parameter;
                    } else if (parameter is object[]) {
                        foreach (var obj in (object[])parameter) {
                            if (obj is Size) {
                                outSize = (Size)obj;
                            }
                            if (obj is SKFilterQuality) {
                                outQuality = (SKFilterQuality)obj;
                            }
                        }
                    }
                }
                #endregion

                #region Create outObject
                object outObject = null;
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
                    } else if (targetType == typeof(CachedImage)) {
                        var imgSrc = await ConvertAsync(fromBytes, typeof(ImageSource), parameter) as ImageSource;
                        outObject = new CachedImage() { Source = imgSrc };
                    } else if (targetType == typeof(string)) {
                        //byte[]->string
                        outObject = System.Convert.ToBase64String(fromBytes);
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
                        using var encoded = skImg.Encode();
                        using var stream = encoded.AsStream();
                        outObject = ImageSource.FromStream(() => stream);
                    } else if (targetType == typeof(string)) {
                        //SKBitmap->string
                        var bytes = await ConvertAsync(fromSkBitmap, typeof(byte[]), parameter) as byte[];
                        outObject = System.Convert.ToBase64String(bytes);
                    }
                } else if (value is ImageSource fromImageSource) {
                    if (targetType == typeof(ImageSource)) {
                        //ImageSource->ImageSource
                        outObject = fromImageSource;
                    } else if (targetType == typeof(byte[])) {
                        //ImageSource->byte[]
                        if (value is StreamImageSource fromStreamImageSource) {
                            //StreamImageSource->byte[]
                            var streamFromImageSource = await fromStreamImageSource.Stream(CancellationToken.None);

                            if (streamFromImageSource == null) {
                                return null;
                            }
                            using var memoryStream = new MemoryStream();
                            streamFromImageSource.CopyTo(memoryStream);

                            outObject = memoryStream.ToArray();
                        } else {
                            //ImageSource->byte[]
                            var skBmp = await ConvertAsync(fromImageSource, typeof(SKBitmap), parameter) as SKBitmap;
                            outObject = await ConvertAsync(skBmp, typeof(byte[]), parameter) as byte[];
                        }
                    } else if (targetType == typeof(SKBitmap)) {
                        //ImageSource->SKBitmap
                        //SKBitmapImageSource skBmpImgSrc = (SKBitmapImageSource)fromImageSource;
                        var bytes = await ConvertAsync(fromImageSource, typeof(byte[]), parameter) as byte[];
                        var skBmp = new SKBitmap();
                        // pin the managed array so that the GC doesn't move it
                        var gcHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                        // install the pixels with the color type of the pixel data
                        var info = new SKImageInfo(2, 2, SKImageInfo.PlatformColorType, SKAlphaType.Unpremul);
                        skBmp.InstallPixels(info, gcHandle.AddrOfPinnedObject(), info.RowBytes, delegate { gcHandle.Free(); }, null);
                        outObject = skBmp;
                    } else if (targetType == typeof(SKBitmap)) {
                        //ImageSource->string
                        var bytes = await ConvertAsync(fromImageSource, typeof(byte[]), parameter) as byte[];
                        outObject = System.Convert.ToBase64String(bytes);
                    }
                } else if (value is string fromStringBase64) {
                    if (targetType == typeof(ImageSource)) {
                        //string->ImageSource
                        var bytes = System.Convert.FromBase64String(fromStringBase64);
                        var stream = new MemoryStream(bytes);
                        outObject = ImageSource.FromStream(() => stream);
                    } else if (targetType == typeof(SKBitmap)) {
                        //string->SKBitmap
                        var imgSrc = await ConvertAsync(fromStringBase64, typeof(ImageSource), parameter) as ImageSource;
                        outObject = await ConvertAsync(imgSrc, typeof(SKBitmap), parameter) as SKBitmap;
                    } else if (targetType == typeof(byte[])) {
                        //string->byte[]
                        var imgSrc = await ConvertAsync(fromStringBase64, typeof(ImageSource), parameter) as ImageSource;
                        outObject = await ConvertAsync(imgSrc, typeof(byte[]), parameter) as byte[];
                    }

                }
                #endregion

                #region Process Parameters
                // TODO add resizing, quality, tinting, layer etc. either here or in output creation
                #endregion

                return outObject;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return null;
        }
    }
}

