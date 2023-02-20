using Android.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia.Android {
    public static class MpAvAdCommonExtensions {
        public static MpMainWindowOrientationType ToPortableOrientationType(this SurfaceOrientation so) {
            switch (so) {
                case SurfaceOrientation.Rotation0:
                    return MpMainWindowOrientationType.Left;
                case SurfaceOrientation.Rotation90:
                    return MpMainWindowOrientationType.Bottom;
                case SurfaceOrientation.Rotation180:
                    return MpMainWindowOrientationType.Right;
                case SurfaceOrientation.Rotation270:
                    return MpMainWindowOrientationType.Top;
            }
            return MpMainWindowOrientationType.Left;
        }

    }
}
