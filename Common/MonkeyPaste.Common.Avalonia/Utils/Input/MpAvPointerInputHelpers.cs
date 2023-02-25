using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System;
using System.Diagnostics;

namespace MonkeyPaste.Common.Avalonia {
    public enum MpPointerEventType {
        None = 0,
        Press,
        Release,
        Enter,
        Leave,
        Move
    }
    public static class MpAvPointerInputHelpers {
        public static RoutedEvent ToRoutedEvent(this MpPointerEventType pet) {
            switch (pet) {
                case MpPointerEventType.Press:
                    return Control.PointerPressedEvent;

                case MpPointerEventType.Release:
                    return Control.PointerReleasedEvent;

                case MpPointerEventType.Enter:
                    return Control.PointerEnteredEvent;

                case MpPointerEventType.Leave:
                    return Control.PointerExitedEvent;

                case MpPointerEventType.Move:
                    return Control.PointerMovedEvent;
            }
            return null;
        }

        public static RoutedEventArgs SimulatePointerEventArgs(
            RoutedEvent eventType,
            Interactive interactive,
            MpPoint mp, MpKeyModifierFlags kmf, bool isLocalMp) {
            Control control = interactive as Control;
            if (control == null) {
                return null;
            }
            Control vroot = control.GetVisualRoot() as Control;
            if (vroot == null) {
                // needs root
                Debugger.Break();
                return null;
            }
            var root_mp = mp.TranslatePoint(vroot, isLocalMp);
            Pointer pointer = new Pointer(Pointer.GetNextFreeId(), PointerType.Mouse, true);
            RoutedEventArgs out_event;

#pragma warning disable CS0618 // Type or member is obsolete
            if (eventType == Control.PointerPressedEvent) {
                out_event = new PointerPressedEventArgs(
                    control,
                    pointer,
                    vroot,
                    root_mp.ToAvPoint(),
                    (ulong)DateTime.Now.Ticks,
                    new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed),
                    kmf.ToAvKeyModifiers());
            } else if (eventType == Control.PointerReleasedEvent) {
                out_event = new PointerReleasedEventArgs(
                    control,
                    pointer,
                    vroot,
                    root_mp.ToAvPoint(),
                    (ulong)DateTime.Now.Ticks,
                    new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonReleased),
                    kmf.ToAvKeyModifiers(), MouseButton.Left);
            } else {
                out_event = new PointerEventArgs(
                    eventType,
                    control,
                    pointer,
                    vroot,
                    root_mp.ToAvPoint(),
                    (ulong)DateTime.Now.Ticks,
                    new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed),
                    kmf.ToAvKeyModifiers());
            }
#pragma warning restore CS0618 // Type or member is obsolete


            return out_event;
        }
    }
}
