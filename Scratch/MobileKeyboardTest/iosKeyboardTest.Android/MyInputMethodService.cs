using Android;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Hardware.Lights;
using Android.Icu.Util;
using Android.InputMethodServices;
using Android.Media;
using Android.OS;
using Android.Telephony;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Avalonia.Android;
using Avalonia.Layout;
using Avalonia.Styling;
using Java.Util;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using static Android.Provider.MediaStore.Audio;
using static System.Net.Mime.MediaTypeNames;
using Debug = System.Diagnostics.Debug;
using Exception = System.Exception;
using Keycode = Android.Views.Keycode;
using View = Android.Views.View;

namespace iosKeyboardTest.Android {
    [Service(Name = "com.CompanyName.MyInputMethodService")]
    public class MyInputMethodService : InputMethodService, IKeyboardInputConnection, ITriggerTouchEvents {
        #region Private Variables
        // from https://learn.microsoft.com/en-us/answers/questions/252318/creating-a-custom-android-keyboard
        private KeyboardFlags? _lastFlags;
        private ClipboardListener _cbListener;
        private EditorInfo _lastEditorInfo = default;
        private AdKeyboardView _keyboardView = default;
        private Handler _handler;
        #endregion

        #region Constants
        #endregion

        #region Statics
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
            if (this.CurrentInputConnection == null) {
                return;
            }
            try {

                string selectedText = this.CurrentInputConnection.GetSelectedText(0);

                if (string.IsNullOrEmpty(selectedText)) {
                    this.CurrentInputConnection.DeleteSurroundingText(count, 0);
                } else {
                    this.CurrentInputConnection.CommitText(string.Empty, 1);
                }
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.ToString());
            }
        }

        void IKeyboardInputConnection.OnDone() {
            if (this.CurrentInputConnection == null) {
                return;
            }
            this.CurrentInputConnection.SendKeyEvent(new KeyEvent(KeyEventActions.Down, Keycode.Enter));
            this.CurrentInputConnection.SendKeyEvent(new KeyEvent(KeyEventActions.Up, Keycode.Enter));
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

        public event EventHandler OnCursorChanged;
        public event EventHandler OnFlagsChanged;
        public event EventHandler OnDismissed;
        #endregion

        #endregion

        #region Properties
        Context CurrentContext =>
            MainActivity.Instance; //this.Window.Context;
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
            //return CreateAdKeyboard();
            return CreateAvKeyboard();
        }
        public override void OnUpdateSelection(int oldSelStart, int oldSelEnd, int newSelStart, int newSelEnd, int candidatesStart, int candidatesEnd) {
            base.OnUpdateSelection(oldSelStart, oldSelEnd, newSelStart, newSelEnd, candidatesStart, candidatesEnd);
            this.OnCursorChanged?.Invoke(this, EventArgs.Empty);
        }
        public override void OnStartInput(EditorInfo attribute, bool restarting) {
            base.OnStartInput(attribute, restarting);
            _lastEditorInfo = attribute;
            var new_flags = Flags;
            //if (_lastFlags.Value != new_flags) {
                this.OnFlagsChanged?.Invoke(this, EventArgs.Empty);
            //}
            _lastFlags = new_flags;
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private void _keyboardView_OnMotionEvent(object sender, MotionEvent e) {
            if (e == null) {
                return;
            }
            double x = e.GetX() / AndroidDisplayInfo.Scaling;
            double y = e.GetY() / AndroidDisplayInfo.Scaling;
            Avalonia.Point p = new Avalonia.Point(x, y);
            var tet =
                e.Action == MotionEventActions.Down ?
                    TouchEventType.Press :
                    e.Action == MotionEventActions.Move ?
                        TouchEventType.Move :
                        e.Action == MotionEventActions.Up ?
                            TouchEventType.Release :
                            TouchEventType.None;

            OnPointerChanged?.Invoke(this, new TouchEventArgs(p, tet));
        }
        View CreateAdKeyboard() {
            _keyboardView = new AdKeyboardView(this, this);
            _keyboardView.OnMotionEvent += _keyboardView_OnMotionEvent;

            //Timer timer = new Timer();
            //timer.ScheduleAtFixedRate(new RenderTask(_keyboardView,new Handler()), (long)0, (long)(1000d / 60d));

            return _keyboardView;
        }

        View CreateAvKeyboard() {
            try {
                var kb_size = KeyboardViewModel.GetTotalSizeByScreenSize(AndroidDisplayInfo.ScaledSize);
                var av = new AvaloniaView(CurrentContext) {
                    Focusable = false,
                    Content = KeyboardFactory.CreateKeyboardView(this, kb_size, AndroidDisplayInfo.Scaling, out var unscaledSize)
                };
                var cntr2 = (LinearLayout)LayoutInflater.Inflate(Resource.Layout.keyboard_layout_view, null);
                cntr2.AddView(av);
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
            //_handler = new Handler();
            this.Window.CancelEvent += Window_CancelEvent;
            this.Window.DismissEvent += Window_DismissEvent;
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

            kbf |= AndroidDisplayInfo.IsPortrait ? KeyboardFlags.Portrait : KeyboardFlags.Landscape;

            // does android have floating keyboard? 
            kbf |= KeyboardFlags.FullLayout;

            if (_lastEditorInfo != null) {
                string input_type_str = _lastEditorInfo.InputType.ToString().ToLower();
                if(input_type_str.Contains("number") || input_type_str.Contains("date") || input_type_str.Contains("phone")) {
                    kbf |= KeyboardFlags.Numbers;
                } else if(input_type_str.Contains("email")) {
                    kbf |= KeyboardFlags.Email;
                } else if(input_type_str.Contains("uri")) {
                    kbf |= KeyboardFlags.Url;
                } else {
                    kbf |= KeyboardFlags.FreeText;
                }

                if(_lastEditorInfo.ImeOptions.HasFlag(ImeFlags.NavigateNext)) {
                    kbf |= KeyboardFlags.Next;
                }
            }

            if (CurrentContext != null &&
                CurrentContext.GetSystemService(Context.UiModeService) is UiModeManager uimm) {
                kbf |= uimm.NightMode == UiNightMode.Yes ? KeyboardFlags.Dark : KeyboardFlags.Light;
            }
            //if(App.Current is { } app) {
            //    if (app.ActualThemeVariant == ThemeVariant.Dark) {
            //        kbf |= KeyboardFlags.Dark;
            //    } else {
            //        kbf |= KeyboardFlags.Light;
            //    }
            //}

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
        void Vibrate() {
            Vibration.Vibrate(3);
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

        public event EventHandler<TouchEventArgs> OnPointerChanged;

        #endregion
        #endregion

    }

    public class RenderTask : TimerTask {
        View _view;
        Handler _handler;
        public RenderTask(View view, Handler handler) {
            _view = view;
            _handler = handler;
        }
        public override void Run() {
            _handler.Post(() => {
                _view.Invalidate();
            });
            
        }
    }
}
