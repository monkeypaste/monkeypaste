using System;
using System.Collections.Generic;
using System.Diagnostics;

using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Xamarin.Forms.Platform.UWP;
using static MonkeyPaste.UWP.MpShellEx;

namespace MonkeyPaste.UWP {
    public class MpImageHelper {
        private static readonly Lazy<MpImageHelper> _Lazy = new Lazy<MpImageHelper>(() => new MpImageHelper());
        public static MpImageHelper Instance { get { return _Lazy.Value; } }

        public BitmapSource GetIconImage(string sourcePath) {
            if (!File.Exists(sourcePath)) {
                if (!Directory.Exists(sourcePath)) {
                    //return (BitmapSource)new BitmapImage(new Uri(@"pack://application:,,,/Resources/Images/monkey (2).png"));
                    //return ConvertBitmapToBitmapSource(System.Drawing.SystemIcons.Question.ToBitmap());
                    return ConvertBitmapToBitmapSource(System.Drawing.SystemIcons.Exclamation.ToBitmap());
                } else {
                    return GetBitmapFromFolderPath(sourcePath, IconSizeEnum.MediumIcon32);
                }

            }
            return GetBitmapFromFilePath(sourcePath, IconSizeEnum.MediumIcon32);
        }


        public BitmapSource ConvertBitmapToBitmapSource(System.Drawing.Bitmap bitmap) {
            using System.IO.MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            byte[] byteImage = ms.ToArray();
            using (InMemoryRandomAccessStream mras = new InMemoryRandomAccessStream()) {
                using (DataWriter writer = new DataWriter(mras.GetOutputStreamAt(0))) {
                    writer.WriteBytes(byteImage);
                    writer.StoreAsync().GetResults();
                }

                var image = new BitmapImage();
                image.SetSource(mras);
                return image;
            }
            //var img = new BitmapImage();
            //img.SetSource(ms);


            //var SigBase64 = Convert.ToBase64String(byteImage); // Get Base64

           // var bytes = new ImageConverter().Convert(bitmap, typeof(byte[]),null,null) as byte[];
            //using (var stream = new Windows.Storage.Streams.InMemoryRandomAccessStream()) {
            //    bitmap.Save(stream.AsStream(), ImageFormat.Png);//choose the specific image format by your own bitmap source
            //    Windows.Graphics.Imaging.BitmapDecoder decoder = AsyncHelpers.RunSync<BitmapDecoder>(() => Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream).AsTask());
            //    SoftwareBitmap softwareBitmap = AsyncHelpers.RunSync<SoftwareBitmap>(() => decoder.GetSoftwareBitmapAsync().AsTask());
            //    return System.Drawing.Imaging.Conver softwareBitmap as BitmapSource;
            //}

            //using (System.IO.MemoryStream memory = new System.IO.MemoryStream()) {
            //    bitmap.Save(memory, ImageFormat.Png);
            //    memory.Position = 0;
            //    BitmapImage bitmapImage = new BitmapImage();
            //    bitmapImage.BeginInit();
            //    bitmapImage.StreamSource = memory;
            //    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            //    bitmapImage.EndInit();
            //    bitmapImage.Freeze();
            //    return bitmapImage as BitmapSource;
            //}
        }
    }
}
