using Android.Views;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia.Android {
    public static class MpAvAdInputExtensions {
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
    }
}
