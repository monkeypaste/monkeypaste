using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using System;
using System.Diagnostics;

namespace MonkeyPaste.Avalonia {
    public enum MpPointerEventType {
        None = 0,
        Down,
        Up,
        Enter,
        Leave,
        Move
    }
    public static class MpAvPointerInputHelpers {
        public static RoutedEvent ToRoutedEvent(this MpPointerEventType pet, RoutingStrategies routeType = RoutingStrategies.Direct) {
            RoutedEvent re = default;
            switch (pet) {
                case MpPointerEventType.Down:
                    re = InputElement.PointerPressedEvent;
                    break;
                case MpPointerEventType.Up:
                    re = InputElement.PointerReleasedEvent;
                    break;
                case MpPointerEventType.Enter:
                    re = InputElement.PointerEnteredEvent;
                    break;
                case MpPointerEventType.Leave:
                    re = InputElement.PointerExitedEvent;
                    break;
                case MpPointerEventType.Move:
                    re = InputElement.PointerMovedEvent;
                    break;
            }
            return re;

        }

        public static RoutedEventArgs CreatePointerEventArgs(
            RoutedEvent eventType,
            Interactive interactive,
            MpPoint mp,
            KeyModifiers kmf,
            bool isLocalMp,
            bool isLeftButton = true) {
            Control control = interactive as Control;
            if (control == null) {
                return null;
            }
            Control vroot = control.GetVisualRoot() as Control;
            if (vroot == null) {
                // needs root
                MpDebug.Break();
                return null;
            }
            var root_mp = mp.TranslatePoint(vroot, isLocalMp);
            Pointer pointer = new Pointer(Pointer.GetNextFreeId(), PointerType.Mouse, true);
            RoutedEventArgs out_event;

            RawInputModifiers rim = isLeftButton ? RawInputModifiers.LeftMouseButton : RawInputModifiers.RightMouseButton;
            PointerUpdateKind puk =
                isLeftButton && eventType == InputElement.PointerPressedEvent ?
                    PointerUpdateKind.LeftButtonPressed :
                    isLeftButton && eventType == InputElement.PointerReleasedEvent ?
                        PointerUpdateKind.LeftButtonReleased :
                        !isLeftButton && eventType == InputElement.PointerPressedEvent ?
                            PointerUpdateKind.RightButtonPressed :
                            PointerUpdateKind.RightButtonReleased;

            //#pragma warning disable CS0618 // Type or member is obsolete
            if (eventType == InputElement.PointerPressedEvent) {
                out_event = new PointerPressedEventArgs(
                    source: control,
                    pointer: pointer,
                    rootVisual: vroot,
                    rootVisualPosition: root_mp.ToAvPoint(),
                    timestamp: (ulong)DateTime.Now.Ticks,
                    properties: new PointerPointProperties(rim, puk),
                    modifiers: kmf);
            } else if (eventType == InputElement.PointerReleasedEvent) {
                out_event = new PointerReleasedEventArgs(
                    control,
                    pointer,
                    vroot,
                    root_mp.ToAvPoint(),
                    (ulong)DateTime.Now.Ticks,
                    new PointerPointProperties(rim, puk),
                    kmf,
                    isLeftButton ? MouseButton.Left : MouseButton.Right);
            } else {
                out_event = new PointerEventArgs(
                    eventType,
                    control,
                    pointer,
                    vroot,
                    root_mp.ToAvPoint(),
                    (ulong)DateTime.Now.Ticks,
                    new PointerPointProperties(),
                    kmf);
            }
            //#pragma warning restore CS0618 // Type or member is obsolete

            return out_event;
        }
    }
}
