using Avalonia.Controls;
using PropertyChanged;
using System;
using X11;
using AvWindow = Avalonia.Controls.Window;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvX11Window : AvWindow {
        public IntPtr XdndAware;
        public IntPtr XdndSelection;
        public IntPtr XdndTypeList;
        public IntPtr XdndActionCopy;
        public IntPtr XdndEnter;
        public IntPtr XdndPosition;
        public IntPtr XdndStatus;
        public IntPtr XdndLeave;
        public IntPtr XdndDrop;
        public IntPtr XdndFinished;

        //[AtomAlternativeName("text/uri-list")]
        public IntPtr MimeTextUriList;
        //[AtomAlternativeName("text/plain")]
        public IntPtr MimeText;
        //[AtomAlternativeName("text/plain;charset=utf-8")]
        public IntPtr MimeTextUtf8;

        public MpAvX11Window() : base() {
            //Atom.
            //Xlib.XChangeProperty(_x11.Display, _handle, _x11.Atoms.XdndAware, _x11.Atoms.XA_ATOM,
            //    32, PropertyMode.Replace, new[] { Xdnd.ProtocolVersion }, 1);
        }

    }
}
