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
using AndroidX.Preference;
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
using Stream = System.IO.Stream;
using View = Android.Views.View;

namespace iosKeyboardTest.Android {
    [Service(Name = "com.CompanyName.MyInputMethodService")]
    public class MyInputMethodService : 
        InputMethodService, 
        IKeyboardInputConnection_android, 
        IOnTouchListener,
        IAssetLoader,
        IMainThread,
        ITriggerTouchEvents {
        #region Private Variables
        // from https://learn.microsoft.com/en-us/answers/questions/252318/creating-a-custom-android-keyboard
        private KeyboardFlags? _lastFlags;
        private ClipboardListener _cbListener;
        private EditorInfo _lastEditorInfo = default;
        AudioManager _audioManager;

        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces

        #region IAssetLoader Implementation
        Stream IAssetLoader.LoadStream(string path) {
            AssetManager assets = ApplicationContext.Assets;
            return assets.Open(path);
        }
        #endregion

        #region IMainThread Implementation
        void IMainThread.Post(Action action) {
            MainThread.BeginInvokeOnMainThread(action);
        }
        #endregion


        #region IOnTouchListener Implementation
        KeyGestureListener KeyGestureListener { get; set; }
        Dictionary<int, Point> _touches { get; set; } = [];
        public bool OnTouch(View v, MotionEvent e) {
            bool handled = true;
            var changed_touches = e.GetMotions(_touches);
            foreach(var ct in changed_touches) {
                var touch_e = new TouchEventArgs(ct.loc / Android.KeyboardView.Scaling, ct.eventType, ct.id.ToString());
                OnPointerChanged?.Invoke(this, touch_e);
            }
            return handled;
        }
        #endregion

        #region IKeyboardInputConnection Implementation
        void IKeyboardInputConnection.OnShowPreferences(object args) {
            StartPrefActivity();
        }
        IMainThread IKeyboardInputConnection.MainThread =>
            this;
        IAssetLoader IKeyboardInputConnection.AssetLoader =>
            this;
        ISharedPrefService IKeyboardInputConnection.SharedPrefService =>
            PrefManager;
        ITextMeasurer IKeyboardInputConnection.TextMeasurer =>
            KeyboardView;

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
            if (flags.HasFlag(KeyboardFeedbackFlags.Click)) {
                PlaySound(SoundEffect.KeyClick);
            }
            if(flags.HasFlag(KeyboardFeedbackFlags.Return)) {
                PlaySound(SoundEffect.Return);
            }
        }


        public event EventHandler<TouchEventArgs> OnPointerChanged;
        public event EventHandler<TextRange> OnCursorChanged;
        public event EventHandler OnFlagsChanged;
        public event EventHandler OnDismissed;
        #endregion

        #endregion

        #region Properties
        MyPrefManager PrefManager { get; set; }
        bool IS_PLATFORM_MODE => true;
        KeyboardView KeyboardView { get; set; }
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

        
        public override void OnUpdateSelection(int oldSelStart, int oldSelEnd, int newSelStart, int newSelEnd, int candidatesStart, int candidatesEnd) {
            base.OnUpdateSelection(oldSelStart, oldSelEnd, newSelStart, newSelEnd, candidatesStart, candidatesEnd);
            //MainThread.BeginInvokeOnMainThread(async() => {
            //    //    // BUG when OnUpdateSelection is called actual insert isn't changed yet, wait a sec before signalling handler
            //    await Task.Delay(150);
            //    OnCursorChanged?.Invoke(this, EventArgs.Empty);
            //});
            var info = GetTextInfo();
            info.Select(newSelStart, newSelEnd - newSelStart);
            OnCursorChanged?.Invoke(this, info);
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
            this.OnFlagsChanged?.Invoke(this, EventArgs.Empty);

            if (KeyboardView is KeyboardView kbv) {
                kbv.RemapRenderers();
            }
        }
        View CreateAdKeyboard() {
            KeyGestureListener = new KeyGestureListener(this);
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
            if(PrefManager == null) {
                PrefManager = new MyPrefManager(this);
            }
            AndroidDisplayInfo.Init(this.Window.Window,IsPortrait());

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
            if(IS_PLATFORM_MODE) {
                kbf |= KeyboardFlags.PlatformView;
            } 
            
            kbf |= IsPortrait() ? KeyboardFlags.Portrait : KeyboardFlags.Landscape;

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


            // does android have floating keyboard? 
            kbf |= KeyboardFlags.FullLayout;

            //kbf |= KeyboardFlags.EmojiKey;
            //kbf |= KeyboardFlags.ShowPopups |
            //        KeyboardFlags.KeyBorders |
            //        //KeyboardFlags.NumberRow |
            //        KeyboardFlags.AutoCap |
            //        KeyboardFlags.DoubleTapSpace |
            //        KeyboardFlags.CursorControl |
            //        KeyboardFlags.Vibrate |
            //        KeyboardFlags.Sound;

            if(PrefManager == null) {
                Init();
            }

            kbf = PrefManager.UpdateFlags(kbf);
            _lastFlags = kbf;
            return kbf;
        }

        #region Helpers

        public void StartPrefActivity() {
            Intent prefIntent = new Intent(this, typeof(MyPreferencesActivity));
            prefIntent.AddFlags(ActivityFlags.NewTask);
            StartActivity(prefIntent);
        }

        bool IsPortrait() {
            return Resources.Configuration.Orientation == Orientation.Portrait;
        }
        void Vibrate() {
            if(PrefManager.VibrateDurMs == 0) {
                return;
            }
            Microsoft.Maui.Devices.Vibration.Vibrate(PrefManager.VibrateDurMs);
        }
        void PlaySound(SoundEffect sound) {
            if(PrefManager.SoundVol == 0) {
                return;
            }
            if (_audioManager == null && 
                CurrentContext != null &&
                CurrentContext.GetSystemService(Context.AudioService) is AudioManager am) {
                _audioManager = am;
            }
            _audioManager.PlaySoundEffect(sound,PrefManager.SoundVol);
        }

        TextRange GetTextInfo() {
            if (this.CurrentInputConnection == null) {
                return default;
            }
            string pre_text = (this as IKeyboardInputConnection).GetLeadingText(-1, -1);
            string sel_text = this.CurrentInputConnection.GetSelectedText(0) ?? string.Empty;
            string post_text = GetTrailingText(-1, -1);
            string total_text = pre_text + sel_text + post_text;
            int sel_len = sel_text.Length == total_text.Length ? 0 : sel_text.Length;
            //return (total_text, (pre_text.Length, sel_len));
            return new TextRange(total_text, pre_text.Length, sel_len);
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
