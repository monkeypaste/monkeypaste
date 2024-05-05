using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System.IO;

namespace MonkeyPaste.Avalonia {
    public class MpAvWindowIcon :
#if WINDOWED
        Bitmap
#else
        WindowIcon
#endif 
        {
#if WINDOWED
        public MpAvWindowIcon(string fileName) : base(fileName) {
        }

        public MpAvWindowIcon(Stream stream) : base(stream) {
        }

        public MpAvWindowIcon(PixelFormat format, AlphaFormat alphaFormat, nint data, PixelSize size, Vector dpi, int stride) : base(format, alphaFormat, data, size, dpi, stride) {
        }

        protected MpAvWindowIcon(IBitmapImpl impl) : base(impl) {
        } 
#else
        public MpAvWindowIcon(Bitmap bitmap) : base(bitmap) {
        }

        public MpAvWindowIcon(string fileName) : base(fileName) {
        }

        public MpAvWindowIcon(Stream stream) : base(stream) {
        }
#endif

    }
}
