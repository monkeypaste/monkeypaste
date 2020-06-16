using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
//using Gtk;

namespace MpWpfApp {   
    public class MpKeyboardHook : IDisposable {        
        /// <summary>
        /// Represents the window that is used internally to get the messages.
        /// </summary>
        private class Window : NativeWindow, IDisposable {
            private static int WM_HOTKEY = 0x0312;

            public Window()  {
                // create the handle for the window.
                this.CreateHandle(new CreateParams());                
            }

            /// <summary>
            /// Overridden to get the notifications.
            /// </summary>
            /// <param name="m"></param>
            protected override void WndProc(ref Message m) {
                // check if we got a hot key pressed.
                if (m.Msg == WM_HOTKEY) {
                    // get the keys.
                    Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                    ModifierKeys modifier = (ModifierKeys)((int)m.LParam & 0xFFFF);

                    // invoke the event to notify the parent.
                    KeyPressed(this,new KeyPressedEventArgs(modifier,key));
                }
                base.WndProc(ref m);
            }
            public event EventHandler<KeyPressedEventArgs> KeyPressed;

            #region IDisposable Members

            public void Dispose()
            {
                this.DestroyHandle();
            }

            #endregion
        }
        private Window _window;
        private int _currentId;
        private const int GrabModeAsync = 1;
        private const int KeyPress = 2;
        private bool _isEventRaised = false;

        private ModifierKeys modifierWin;
        /*private Gdk.ModifierType modifier;
        private Gdk.Key key;*/
        private Keys keyWin;
        private bool _isRegistered = false;

        public MpKeyboardHook() {
            if(MpCompatibility.IsRunningOnMono()) {
                InitGdk();
            } else {
                InitWin();
            }            
        }
        
        private void InitWin() {
            _window = new Window();
            // register the event of the inner native window.
            _window.KeyPressed += delegate (object sender,KeyPressedEventArgs args) {
                if(KeyPressed != null) {
                    //_isEventRaised = false;
                    KeyPressed(this,args);
                }
            };
            
        }
        /// <summary>
        /// Registers a hot key in the system.
        /// </summary>
        /// <param name="modifier">The modifiers that are associated with the hot key.</param>
        /// <param name="key">The key itself that is associated with the hot key.</param>
        public void RegisterHotKey(ModifierKeys modifier, Keys key) {            
            if(MpCompatibility.IsRunningOnMono()) {
                /*modifierWin = modifier;
                keyWin = key;
                RegisterHotKeyGdk(MpCompatibility.GetGdkModifierFromWin(modifier),key);*/
            } else {
                RegisterHotKeyWin(modifier,key);
            }
        }
        
        private void RegisterHotKeyWin(ModifierKeys modifier,Keys key) {
            // increment the counter.
            _currentId = _currentId + 1;
            // register the hot key.
            if(!WinApi.RegisterHotKey(_window.Handle,_currentId,(uint)modifier,(uint)key)) {
                 throw new InvalidOperationException("Couldn’t register the hot key.");
                _isRegistered = false;
                return;
            }
            _isRegistered = true;
        }
        public bool IsRegistered() {
            return _isRegistered;
        }
        public void UnregisterHotKey() {
            if(MpCompatibility.IsRunningOnMono()) {
                UnregisterHotKeyGdk();
            }
            else {
                UnregisterHotKeyWin();
            }
            _isRegistered = false;
        }
        
        private void UnregisterHotKeyWin() {
            // unregister all the registered hot keys.
            for(int i = _currentId;i > 0;i--) {
                WinApi.UnregisterHotKey(_window.Handle,i);
            }
            _window.KeyPressed -= delegate (object sender,KeyPressedEventArgs args) {
                if(KeyPressed != null) {
                    KeyPressed(this,args);
                }
            };
        }
        private void InitGdk() {
            Gdk.Window rootWin = Gdk.Global.DefaultRootWindow;
            IntPtr xDisplay = GetXDisplay(rootWin);
        }
        private void UnregisterHotKeyGdk() {
            /*Gdk.Window rootWin = Gdk.Global.DefaultRootWindow;
            IntPtr xDisplay = GetXDisplay(rootWin);
            GdkApi.XUngrabKey(
                 xDisplay,
                 (int)key,
                 (uint)modifier,
                 GetXWindow(rootWin));*/
        }
        /*private Gdk.FilterReturn FilterFunction(IntPtr xEvent,Gdk.Event evnt) {
            XKeyEvent xKeyEvent = (XKeyEvent)Marshal.PtrToStructure(xEvent,typeof(XKeyEvent));

            if(xKeyEvent.type == KeyPress) {
                if(xKeyEvent.keycode == (int)key && xKeyEvent.state == (uint)modifier) {
                    KeyPressed(this,new KeyPressedEventArgs(modifierWin,keyWin));
                }
            }
            return Gdk.FilterReturn.Continue;
        }
        private void RegisterHotKeyGdk(Gdk.ModifierType modifier,Keys key) {
            Gdk.Window rootWin = Gdk.Global.DefaultRootWindow;
            IntPtr xDisplay = GetXDisplay(rootWin);
            this.key = (Gdk.Key)GdkApi.XKeysymToKeycode(xDisplay,(int)this.key);
            rootWin.AddFilter(new Gdk.FilterFunc(FilterFunction));
            this.modifier = modifier;
            GdkApi.XGrabKey(
             xDisplay,
             (int)key,
             (uint)modifier,
             GetXWindow(rootWin),
             false,
             GrabModeAsync,
             GrabModeAsync
            );
            _isRegistered = true;
        }*/
        /// <summary>
        /// A hot key has been pressed.
        /// </summary>
        public event EventHandler<KeyPressedEventArgs> KeyPressed;
        #region IDisposable Members

        public void Dispose() {
            UnregisterHotKey();
            // dispose the inner native window.
            if(!MpCompatibility.IsRunningOnMono()) {
                _window.Dispose();
            }            
        }

        #endregion
        private static IntPtr GetXDisplay(Gdk.Window window) {
            return GdkApi.gdk_x11_drawable_get_xdisplay(GdkApi.gdk_x11_window_get_drawable_impl(window.Handle));
        }
        private static IntPtr GetXWindow(Gdk.Window window) {
            return GdkApi.gdk_x11_drawable_get_xid(window.Handle);
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct XKeyEvent
        {
            public short type;
            public uint serial;
            public short send_event;
            public IntPtr display;
            public uint window;
            public uint root;
            public uint subwindow;
            public uint time;
            public int x, y;
            public int x_root, y_root;
            public uint state;
            public uint keycode;
            public short same_screen;
        }       
    }

    /// <summary>
    /// Event Args for the event that is fired after the hot key has been pressed.
    /// </summary>
    public class KeyPressedEventArgs : EventArgs {
        private ModifierKeys _modifier;
        private Keys _key;

        internal KeyPressedEventArgs(ModifierKeys modifier, Keys key) {
            _modifier = modifier;
            _key = key;
        }

        public ModifierKeys Modifier {
            get { return _modifier; }
        }

        public Keys Key {
            get { return _key; }
        }
    }

    /// <summary>
    /// The enumeration of possible modifiers.
    /// </summary>
    [Flags]
    public enum ModifierKeys : uint {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Win = 8
    }
}
