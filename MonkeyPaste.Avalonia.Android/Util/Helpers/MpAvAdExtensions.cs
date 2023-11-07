using Android.Graphics;
using Android.Views;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.IO;

namespace MonkeyPaste.Avalonia.Android {
    public static class MpAvAdExtensions {
        public static MpPointerEventType ToPointerEventType(this MotionEventActions mea) {
            switch (mea) {
                case MotionEventActions.Up:
                    return MpPointerEventType.Release;
                case MotionEventActions.Move:
                    return MpPointerEventType.Move;
                case MotionEventActions.Down:
                    return MpPointerEventType.Press;
                case MotionEventActions.HoverEnter:
                    return MpPointerEventType.Enter;
                case MotionEventActions.Outside:
                    return MpPointerEventType.Leave;
            }
            return MpPointerEventType.None;
        }

        public static string ToBase64Str(this Bitmap bitmap) {
            using var ms = new MemoryStream();
            bitmap.Compress(Bitmap.CompressFormat.Png, 100, ms);
            var buffer = new byte[ms.Length];
            ms.Read(buffer, 0, (int)ms.Length);
            return buffer.ToBase64String();
        }
    }
}
