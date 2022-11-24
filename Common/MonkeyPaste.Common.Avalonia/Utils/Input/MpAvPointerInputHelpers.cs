using Avalonia;
using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Common;
using Avalonia.Interactivity;
using MonoMac.AppKit;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvPointerInputHelpers {
        public static PointerEventArgs SimulatePointerEventArgs(IInteractive interactive, MpPoint gmp, MpKeyModifierFlags kmf) {
            Control control = interactive as Control;
            if(control == null) {
                control = Application.Current.MainWindow();
                if(control == null) {
                    // needs control (i think)
                    Debugger.Break();
                    return null;
                }
            }
            Control vroot = control.GetVisualRoot() as Control;
            if(vroot == null) {
                // needs root
                Debugger.Break();
                return null;
            }
            var root_mp = gmp.TranslatePoint(vroot, false);
            Pointer pointer = new Pointer(Pointer.GetNextFreeId(), PointerType.Mouse, true);
            var pe = new PointerEventArgs(
                Control.PointerPressedEvent,
                control,
                pointer,
                vroot,
                root_mp.ToAvPoint(),
                (ulong)DateTime.Now.Ticks,
                new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed), kmf.ToAvKeyModifiers());

            return pe;
        }
    }
}
