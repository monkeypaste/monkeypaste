using MonkeyPaste.Common;
using SharpHook;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace MonkeyPaste.Avalonia {

    public static class MpDebuggerHelper {
        public static void Break() {
            MpAvGlobalInputHook.Instance.Stop();
            Thread.Sleep(1000);
            Debugger.Break();
            MpAvGlobalInputHook.Instance.Start();
        }
    }
    public class MpAvGlobalInputHook : INotifyPropertyChanged {
        private SimpleGlobalHook _hook;
        private Thread _thread;

        private bool _isRunning = false;

        #region Statics

        private static MpAvGlobalInputHook _instance;
        public static MpAvGlobalInputHook Instance => _instance ?? (_instance = new MpAvGlobalInputHook());

        #endregion

        #region Properties

        #region State

        #region Mouse
        public MpPoint GlobalMouseLocation { get; private set; } = MpPoint.Zero;

        public MpPoint? GlobalMouseLeftButtonDownLocation { get; private set; } = null;

        public bool GlobalIsLeftButtonPressed { get; private set; } = false;

        public bool GlobalIsRightButtonPressed { get; private set; } = false;

        #endregion

        #region Keyboard

        public bool GlobalIsShiftDown { get; private set; } = false;
        public bool GlobalIsControlDown { get; private set; } = false;
        public bool GlobalIsAltDown { get; private set; } = false;

        #endregion

        #endregion

        #endregion

        #region Events


        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Mouse Events

        public event EventHandler<MouseWheelHookEventArgs> OnGlobalMouseWheelScroll;

        public event EventHandler<MouseHookEventArgs> OnGlobalMouseMove;

        public event EventHandler<MouseHookEventArgs> OnGlobalMousePressed;
        public event EventHandler<MouseHookEventArgs> OnGlobalMouseReleased;

        public event EventHandler<MouseHookEventArgs> OnGlobalMouseClicked;

        public event EventHandler<MouseHookEventArgs> OnGlobalMouseDragged;

        #endregion

        #region Keyboard Events

        public event EventHandler<KeyboardHookEventArgs> OnGlobalKeyPressed;
        public event EventHandler<KeyboardHookEventArgs> OnGlobalKeyReleased;

        #endregion

        #endregion

        #region Constructors
        private MpAvGlobalInputHook() {
            
        }

        #endregion

        #region Public Methods

        public void Init() {
            Start();
        }

        public void Start() {
            if (_thread == null) {
                _thread = new Thread(
                    new ParameterizedThreadStart(x => GlobalInputListenerThread()));
                _thread.Priority = ThreadPriority.BelowNormal;
            } else {

            }

            _isRunning = true;
            _thread.Start();

        }

        public void Stop() {
            _isRunning = false;
            _thread = null;
        }
        #endregion

        #region Private Methods

        [STAThread]
        private void GlobalInputListenerThread() {
            if (_hook == null) {
                _hook = new SimpleGlobalHook(); 

                _hook.MouseWheel += Hook_MouseWheel;

                _hook.MouseMoved += Hook_MouseMoved;

                _hook.MousePressed += Hook_MousePressed;
                _hook.MouseReleased += Hook_MouseReleased;

                _hook.MouseClicked += Hook_MouseClicked;

                _hook.MouseDragged += Hook_MouseDragged;

                _hook.KeyPressed += Hook_KeyPressed;
                _hook.KeyReleased += Hook_KeyReleased;
            }


            _hook.RunAsync();

            while (_isRunning) {
                Thread.Sleep(100);
            }

            _hook.MouseWheel -= Hook_MouseWheel;

            _hook.MouseMoved -= Hook_MouseMoved;

            _hook.MousePressed -= Hook_MousePressed;
            _hook.MouseReleased -= Hook_MouseReleased;

            _hook.MouseClicked -= Hook_MouseClicked;

            _hook.MouseDragged -= Hook_MouseDragged;

            _hook.KeyPressed -= Hook_KeyPressed;
            _hook.KeyReleased -= Hook_KeyReleased;

            _hook.Dispose();
            _hook = null;
        }



        #region Mouse Event Handlers

        private void Hook_MouseWheel(object? sender, MouseWheelHookEventArgs e) {
            //MpConsole.WriteLine("Scroll wheel: " + e.Data.Rotation);
            OnGlobalMouseWheelScroll?.Invoke(typeof(MpAvGlobalInputHook).ToString(), e);
        }

        private void Hook_MouseMoved(object? sender, MouseHookEventArgs e) {
            MpPoint unscaled_mp = new MpPoint(e.Data.X, e.Data.Y);
            double scale = MpPlatformWrapper.Services.ScreenInfoCollection.PixelScaling;
            GlobalMouseLocation = new MpPoint(Math.Max(0, (double)e.Data.X / scale), Math.Max(0, (double)e.Data.Y / scale));

            OnGlobalMouseMove?.Invoke(typeof(MpAvGlobalInputHook).ToString(), e);
        }


        private void Hook_MousePressed(object sender, MouseHookEventArgs e) {
            if (e.Data.Button == SharpHook.Native.MouseButton.Button1) {
                GlobalIsLeftButtonPressed = true;
                GlobalMouseLeftButtonDownLocation = GlobalMouseLocation;
            } else if (e.Data.Button == SharpHook.Native.MouseButton.Button2) {
                GlobalIsRightButtonPressed = true;
            } else {
                MpConsole.WriteTraceLine("Unknown mouse button pressed: " + e.Data.Button);
            }

            OnGlobalMousePressed?.Invoke(sender, e);
        }
        private void Hook_MouseReleased(object sender, MouseHookEventArgs e) {
            if (e.Data.Button == SharpHook.Native.MouseButton.Button1) {
                GlobalIsLeftButtonPressed = false;
                GlobalMouseLeftButtonDownLocation = null;
            } else if (e.Data.Button == SharpHook.Native.MouseButton.Button2) {
                GlobalIsRightButtonPressed = false;
            } else {
                MpConsole.WriteTraceLine("Unknown mouse button released: " + e.Data.Button);
            }

            OnGlobalMouseReleased?.Invoke(sender, e);
        }

        private void Hook_MouseClicked(object sender, MouseHookEventArgs e) {
            OnGlobalMouseClicked?.Invoke(sender, e);
        }

        private void Hook_MouseDragged(object sender, MouseHookEventArgs e) {
            OnGlobalMouseDragged?.Invoke(sender, e);
        }

        #endregion

        #region Keyboard EventHadlers


        private void Hook_KeyReleased(object sender, KeyboardHookEventArgs e) {
            if (e.Data.KeyCode == SharpHook.Native.KeyCode.VcLeftShift ||
               e.Data.KeyCode == SharpHook.Native.KeyCode.VcRightShift) {
                GlobalIsShiftDown = false;
            }
            if (e.Data.KeyCode == SharpHook.Native.KeyCode.VcLeftAlt ||
               e.Data.KeyCode == SharpHook.Native.KeyCode.VcRightAlt) {
                GlobalIsAltDown = false;
            }
            if (e.Data.KeyCode == SharpHook.Native.KeyCode.VcLeftControl ||
               e.Data.KeyCode == SharpHook.Native.KeyCode.VcRightControl) {
                GlobalIsControlDown = false;
            }

            OnGlobalKeyPressed?.Invoke(sender, e);
        }

        private void Hook_KeyPressed(object sender, KeyboardHookEventArgs e) {
            if (e.Data.KeyCode == SharpHook.Native.KeyCode.VcLeftShift ||
               e.Data.KeyCode == SharpHook.Native.KeyCode.VcRightShift) {
                GlobalIsShiftDown = true;
            }
            if (e.Data.KeyCode == SharpHook.Native.KeyCode.VcLeftAlt ||
               e.Data.KeyCode == SharpHook.Native.KeyCode.VcRightAlt) {
                GlobalIsAltDown = true;
            }
            if (e.Data.KeyCode == SharpHook.Native.KeyCode.VcLeftControl ||
               e.Data.KeyCode == SharpHook.Native.KeyCode.VcRightControl) {
                GlobalIsControlDown = true;
            }

            OnGlobalKeyReleased?.Invoke(sender, e);
        }

        #endregion

        #endregion
    }

}
