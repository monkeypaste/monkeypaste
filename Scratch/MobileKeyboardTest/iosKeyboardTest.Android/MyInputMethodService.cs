using Android;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Health.Connect.DataTypes;
using Android.InputMethodServices;
using Android.Media;
using Android.OS;
using Android.Telephony;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Avalonia;
using Avalonia.Android;
using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using Xamarin.Essentials;
using static Android.Views.View;
using Color = Android.Graphics.Color;
using Debug = System.Diagnostics.Debug;
using Exception = System.Exception;
using Keycode = Android.Views.Keycode;
using Orientation = Android.Content.Res.Orientation;
using View = Android.Views.View;

namespace iosKeyboardTest.Android {
    [Service(Name = "com.CompanyName.MyInputMethodService")]
    public class MyInputMethodService : InputMethodService, IKeyboardInputConnection_android, ITriggerTouchEvents, IOnTouchListener {
        #region Private Variables
        // from https://learn.microsoft.com/en-us/answers/questions/252318/creating-a-custom-android-keyboard
        private KeyboardFlags? _lastFlags;
        private ClipboardListener _cbListener;
        private EditorInfo _lastEditorInfo = default;
        Thread bgThread1,bgThread2;
        Queue<TouchEventArgs> Touch1Events { get; set; } = [];
        Queue<TouchEventArgs> Touch2Events { get; set; } = [];
        Queue<KeyboardFlags> NewFlags { get; set; } = [];
        public static Queue<(IKeyboardRenderSource, IKeyboardViewRenderer)> PendingRenderers { get; set; } = [];
        int downCount = 0;
        #endregion

        #region Constants
        #endregion

        #region Statics
        public static bool IS_MULTI_THREAD_MODE = false;
        #endregion

        #region Interfaces

        #region IKeyboardInputConnection Implementation
        public KeyboardFlags Flags =>
            GetFlags(_lastEditorInfo);
        string IKeyboardInputConnection.GetLeadingText(int offset, int len) =>
            GetTextAroundCursor(offset, len, true);
        void IKeyboardInputConnection.OnText(string text) {
            if (this.CurrentInputConnection == null ||
                string.IsNullOrEmpty(text)) {
                return;
            }
            try {
                this.CurrentInputConnection.CommitText(text, 1);
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.ToString());
            }
        }

        void IKeyboardInputConnection.OnBackspace(int count) {
            //if (this.CurrentInputConnection == null) {
            //    return;
            //}
            for (int i = 0; i < count; i++) {
                try {
                    string selectedText = this.CurrentInputConnection.GetSelectedText(0);

                    bool success = false;
                    if (string.IsNullOrEmpty(selectedText)) {
                        success = this.CurrentInputConnection.DeleteSurroundingText(1, 0);
                    } else {
                        success = this.CurrentInputConnection.CommitText(string.Empty, 1);
                    }
                    if(success) {
                        continue;
                    }
                    // send actual backspace key event when nothing to delete from conn,
                    // conn method doesn't auto remove Google Keep checkboxes like default keyboard does
                    // NOTE not sure if below does fix it...
                    this.SendDownUpKeyEvents(Keycode.Del);
                }
                catch (Exception ex) {
                    Debug.WriteLine(ex.ToString());
                }
            }
        }

        void IKeyboardInputConnection.OnDone() {
            this.SendDownUpKeyEvents(Keycode.Enter);
        }

        void IKeyboardInputConnection.OnNavigate(int dx, int dy) {
            if (this.CurrentInputConnection == null) {
                return;
            }

            int? x_dir = dx == 0 ? null : dx > 0 ? 1 : -1;
            int? y_dir = dy == 0 ? null : dy > 0 ? 1 : -1;

            if (x_dir.HasValue) {
                Keycode kc = x_dir > 0 ? Keycode.DpadRight : Keycode.DpadLeft;
                for (int i = 0; i < (int)Math.Abs(dx); i++) {
                    this.SendDownUpKeyEvents(kc);
                }
            }
            if (y_dir.HasValue) {
                Keycode kc = y_dir > 0 ? Keycode.DpadDown : Keycode.DpadUp;
                for (int i = 0; i < (int)Math.Abs(dy); i++) {
                    this.SendDownUpKeyEvents(kc);
                }
            }
        }
        void IKeyboardInputConnection.OnFeedback(KeyboardFeedbackFlags flags) {
            if (flags.HasFlag(KeyboardFeedbackFlags.Vibrate)) {
                Vibrate();
            }
            if (flags.HasFlag(KeyboardFeedbackFlags.Click) ||
                flags.HasFlag(KeyboardFeedbackFlags.Return)) {
                Task.Run(() => {
                    // from https://stackoverflow.com/a/54656188
                    var sound = new MediaActionSound();
                    var mast = flags.HasFlag(KeyboardFeedbackFlags.Click) ? MediaActionSoundType.ShutterClick : MediaActionSoundType.StartVideoRecording;
                    sound.Play(mast);
                });
            }
        }


        public event EventHandler<TouchEventArgs> OnPointerChanged;
        public event EventHandler<(string,(int,int))> OnCursorChanged;
        public event EventHandler OnFlagsChanged;
        public event EventHandler OnDismissed;
        #endregion

        #endregion

        #region Properties
        Dictionary<int, Point> _touches { get; set; } = [];
        bool IS_PLATFORM_MODE => true;
        View KeyboardView { get; set; }
        AvaloniaView AvView { get; set; }
        Context CurrentContext =>
            IS_PLATFORM_MODE ? this.Window.Context : MainActivity.Instance;
        #endregion

        #region Events
        #endregion

        #region Constructors
        
        public MyInputMethodService() : base()
        {
        }
        #endregion

        #region Public Methods
        public override View? OnCreateInputView() {
            Init();
            if(IS_PLATFORM_MODE) {
                return CreateAdKeyboard();
            }
            return CreateAvKeyboard();
        }

        public bool OnTouch(View v, MotionEvent e) {
            var tet =
                e.Action.IsDown() ?
                    TouchEventType.Press :
                    e.Action.IsMove() ?
                        TouchEventType.Move :
                        e.Action.IsUp() ?
                            TouchEventType.Release :
                            TouchEventType.None;


            if (tet == TouchEventType.None) {
                // what motion event is it?
                return true;
            }
            if (KeyboardView is KeyboardView kbv &&
                kbv.DC.IsShiftOnLock) {
                kbv.PrintStats();
            }
            //int newDownCount = downCount +
            //    tet == TouchEventType.Press ?
            //        1 :
            //        tet == TouchEventType.Release ?
            //            -1 : 0;


            Avalonia.Point p = e.GetMotionPoint(_touches) / Android.KeyboardView.Scaling;
            var touch_e = new TouchEventArgs(p, tet);
            if(IS_MULTI_THREAD_MODE) {
                Touch1Events.Enqueue(touch_e);
            } else {
                OnPointerChanged?.Invoke(this, touch_e);
            }

            if (e.Action.IsUp() && e.PointerCount == 1) {
                // ensure touch lookup empty between breakpoints
                _touches.Clear();
            }
            return true;
        }
        public override void OnUpdateSelection(int oldSelStart, int oldSelEnd, int newSelStart, int newSelEnd, int candidatesStart, int candidatesEnd) {
            base.OnUpdateSelection(oldSelStart, oldSelEnd, newSelStart, newSelEnd, candidatesStart, candidatesEnd);
            //MainThread.BeginInvokeOnMainThread(async() => {
            //    //    // BUG when OnUpdateSelection is called actual insert isn't changed yet, wait a sec before signalling handler
            //    await Task.Delay(150);
            //    OnCursorChanged?.Invoke(this, EventArgs.Empty);
            //});
            var info = GetTextInfo();

            OnCursorChanged?.Invoke(this, (info.text,(newSelStart,(newSelEnd-newSelStart))));
        }
        public override void OnConfigurationChanged(Configuration newConfig) {
            base.OnConfigurationChanged(newConfig);
            RefreshState();
        }
        public override void OnStartInput(EditorInfo attribute, bool restarting) {
            base.OnStartInput(attribute, restarting);

            _lastEditorInfo = attribute;
            var new_flags = Flags;
            RefreshState();
            _lastFlags = new_flags;

            if (new_flags.HasFlag(KeyboardFlags.PlatformView)) {
                return;
            }
            Dispatcher.UIThread.Post(async () => {
                if (AvView == null ||
                    AvView.Content is not Control c ||
                    c.DataContext is not KeyboardViewModel kbvm) {
                    return;
                }
                while (kbvm.IsBusy) {
                    await Task.Delay(100);
                }

                AvView.Invalidate();
            });
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        void RefreshState() {
            // reset display if orientation changed
            Init();
            // ntf flags probably changed
            if(IS_MULTI_THREAD_MODE) {
                if (_lastFlags is { } lf) {
                    NewFlags.Enqueue(lf);
                }
            } else {
                this.OnFlagsChanged?.Invoke(this, EventArgs.Empty);
            }

            if (KeyboardView is KeyboardView kbv) {
                kbv.RemapRenderers();
            }
        }
        View CreateAdKeyboard() {
            KeyboardView = new KeyboardView(this, this).SetDefaultProps("keyboardView");
            return KeyboardView;
        }

        View CreateAvKeyboard() {
            try {
                var kb_size = KeyboardViewModel.GetTotalSizeByScreenSize(AndroidDisplayInfo.ScaledSize, Flags.HasFlag(KeyboardFlags.Portrait));
                AvView = new AvaloniaView(CurrentContext) {
                    Focusable = false,
                    //Content = KeyboardFactory.CreateKeyboardView(this, kb_size, AndroidDisplayInfo.Scaling, out var unscaledSize)
                };
                Avalonia.Size unscaledSize = default;
                var cntr2 = (LinearLayout)LayoutInflater.Inflate(Resource.Layout.keyboard_layout_view, null);
                cntr2.AddView(AvView);
                cntr2.Focusable = false;
                var cntr = new KeyboardLinearLayout(CurrentContext, (int)unscaledSize.Height);
                cntr.AddView(cntr2);
                return cntr;
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.ToString());
            }
            return null;
        }

        void Init() {
            AndroidDisplayInfo.Init(this.Window.Window,IsPortrait());

            this.Window.CancelEvent += Window_CancelEvent;
            this.Window.DismissEvent += Window_DismissEvent;
            if (bgThread1 == null) {
                bgThread1 = new Thread(TouchBgThread);
                bgThread1.Start();
            }
        }
        void TouchBgThread() {
            while(true) {
                while(Touch1Events.TryPeek(out var te)) {
                    Touch1Events.Dequeue();
                    //MainThread.BeginInvokeOnMainThread(()=> OnPointerChanged?.Invoke(this, te));
                    OnPointerChanged?.Invoke(this, te);
                }
                while(NewFlags.TryPeek(out var fe)) {
                    NewFlags.Dequeue();
                    //MainThread.BeginInvokeOnMainThread(()=>OnFlagsChanged?.Invoke(this, EventArgs.Empty));
                    OnFlagsChanged?.Invoke(this, EventArgs.Empty);
                }
                while (PendingRenderers.TryPeek(out var pr)) {
                    PendingRenderers.Dequeue();
                    //MainThread.BeginInvokeOnMainThread(() => pr.Item1.SetRenderer(pr.Item2));
                    pr.Item1.SetRenderer(pr.Item2);
                }
                Thread.Sleep(50);
            }
        }

        private void Window_DismissEvent(object sender, EventArgs e) {
            // this is called:
            // 1. when the keyboard is toggled to disabled in 'Manage Keyboards'
            // 2. When a different keyboard is selected

            this.OnDismissed?.Invoke(this, EventArgs.Empty);
        }

        private void Window_CancelEvent(object sender, EventArgs e) {
            this.OnDismissed?.Invoke(this, EventArgs.Empty);
        }

        KeyboardFlags GetFlags(EditorInfo info) {
            var kbf = KeyboardFlags.Android;
            if(IS_PLATFORM_MODE) {
                kbf |= KeyboardFlags.PlatformView;
            } 
            
            kbf |= IsPortrait() ? KeyboardFlags.Portrait : KeyboardFlags.Landscape;

            // does android have floating keyboard? 
            kbf |= KeyboardFlags.FullLayout;

            kbf |= KeyboardFlags.EmojiKey;
            kbf |= KeyboardFlags.ShowPopups |
                    KeyboardFlags.KeyBorders |
                    //KeyboardFlags.NumberRow |
                    KeyboardFlags.AutoCap |
                    KeyboardFlags.DoubleTapSpace |
                    KeyboardFlags.CursorControl |
                    KeyboardFlags.Vibrate |
                    KeyboardFlags.Sound;

            if (_lastEditorInfo != null) {
                var ime_type_lookup = new Dictionary<KeyboardFlags, InputTypes[]>() {
                    {
                        KeyboardFlags.Email,
                        [InputTypes.TextVariationEmailAddress,InputTypes.TextVariationWebEmailAddress] 
                    },
                    {
                        KeyboardFlags.Url,
                        [InputTypes.TextVariationUri] 
                    },
                    {
                        KeyboardFlags.Pin,
                        [InputTypes.NumberVariationPassword]
                    },
                    {
                        KeyboardFlags.Digits,
                        [InputTypes.ClassPhone]
                    },
                    {
                        KeyboardFlags.Numbers,
                        [InputTypes.ClassDatetime,InputTypes.ClassNumber] 
                    },
                    {
                        KeyboardFlags.Normal,
                        [InputTypes.TextFlagMultiLine,InputTypes.TextFlagImeMultiLine] 
                    },
                    {
                        KeyboardFlags.Done,
                        [(InputTypes)0] 
                    },

                };
                foreach(var kvp in ime_type_lookup) {
                    foreach(var type in kvp.Value) {
                        if(_lastEditorInfo.InputType.HasFlag(type)) {
                            kbf |= kvp.Key;
                            break;
                        }
                    }
                }

                if(_lastEditorInfo.ImeOptions.HasFlag(ImeFlags.NavigateNext)) {
                    kbf |= KeyboardFlags.Next;
                }
            }

            if (CurrentContext != null &&
                CurrentContext.GetSystemService(Context.UiModeService) is UiModeManager uimm) {
                kbf |= uimm.NightMode == UiNightMode.Yes ? KeyboardFlags.Dark : KeyboardFlags.Light;
            }
            kbf |= KeyboardFlags.Dark;

            if (CurrentContext != null &&
                CurrentContext.GetSystemService(Context.TelephonyService) is TelephonyManager tm) {
                if (tm.PhoneType == PhoneType.None) {
                    kbf |= KeyboardFlags.Tablet;
                } else {
                    kbf |= KeyboardFlags.Mobile;
                }
            }

            _lastFlags = kbf;
            return kbf;
        }

        #region Helpers

        void StartPrefActivity() {
            Intent prefIntent = new Intent(this, typeof(MainActivity));
            prefIntent.AddFlags(ActivityFlags.NewTask);
            StartActivity(prefIntent);
        }

        bool IsPortrait() {
            return Resources.Configuration.Orientation == Orientation.Portrait;
        }
        void Vibrate() {
            if(!_lastFlags.HasValue || !_lastFlags.Value.HasFlag(KeyboardFlags.Vibrate)) {
                return;
            }
            Microsoft.Maui.Devices.Vibration.Vibrate(3);
        }

        (string text, (int start, int len)) GetTextInfo() {
            if (this.CurrentInputConnection == null) {
                return default;
            }
            string pre_text = (this as IKeyboardInputConnection).GetLeadingText(-1, -1);
            string sel_text = this.CurrentInputConnection.GetSelectedText(0) ?? string.Empty;
            string post_text = GetTrailingText(-1, -1);
            string total_text = pre_text + sel_text + post_text;
            int sel_len = sel_text.Length == total_text.Length ? 0 : sel_text.Length;
            return (total_text, (pre_text.Length, sel_len));
        }
        string GetTextAroundCursor(int offset, int len, bool isBefore) {
            if (this.CurrentInputConnection == null) {
                return string.Empty;
            }
            string last_text = string.Empty;
            var sb = new StringBuilder();
            int cur_count = 1;
            while (true) {
                string cur_text = isBefore ?
                    CurrentInputConnection.GetTextBeforeCursor(cur_count, 0) :
                    CurrentInputConnection.GetTextAfterCursor(cur_count, 0);
                if (string.IsNullOrEmpty(cur_text) ||
                    cur_text == last_text) {
                    // NOTE when n is before beginning of text it'll just return as far as it can go
                    break;
                }
                last_text = cur_text;
                if (cur_count == offset) {
                    break;
                }
                cur_count++;
            }
            return last_text;
        }
        string GetTrailingText(int offset, int len) =>
            GetTextAroundCursor(offset, len, false);

        #endregion

        #region Clipbaord
        private void StartClipboardChecker(Context context, string source) {
            Debug.WriteLine($"Clipboard watcher started [{source}]");
            _cbListener = new ClipboardListener(context);
            _cbListener.Start();


            void OnCbChanged(object sender, object e) {
                var test = this.CurrentInputConnection.GetSelectedTextFormatted(GetTextFlags.WithStyles);
                Debug.WriteLine($"Clipboard changed [{source}]");
                var pc = _cbListener.ClipboardManager.PrimaryClip;
                for (int i = 0; i < pc.ItemCount; i++) {

                }
                var test2 = _cbListener.ClipboardManager.PrimaryClipDescription;
                var test3 = _cbListener.ClipboardManager.Text;
                var test4 = _cbListener.ClipboardManager.TextFormatted.ToString();

            }

            _cbListener.OnClipboardChanged += OnCbChanged;
        }



        #endregion

        #endregion

    }
}
