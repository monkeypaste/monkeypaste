using Android;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Hardware.Lights;
using Android.Icu.Util;
using Android.InputMethodServices;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Avalonia;
using Avalonia.Android;
using Avalonia.Layout;
using Microsoft.Maui.Devices;
using System;
using System.Diagnostics;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using Color = Android.Graphics.Color;
using Debug = System.Diagnostics.Debug;
using Exception = System.Exception;
using Keycode = Android.Views.Keycode;
using View = Android.Views.View;

namespace iosKeyboardTest.Android {
    [Service(Name = "com.CompanyName.MyInputMethodService")]
    public class MyInputMethodService : InputMethodService, IKeyboardInputConnection
    {
        // from https://learn.microsoft.com/en-us/answers/questions/252318/creating-a-custom-android-keyboard

        private ClipboardListener _cbListener;
        
        public MyInputMethodService() : base()
        {
        }
        
        public override View? OnCreateInputView()
        {
            try
            {
                var kb_size = KeyboardViewModel.GetTotalSizeByScreenSize(AndroidDisplayInfo.ScaledSize);
                var av = new AvaloniaView(MainActivity.Instance)
                {
                    Focusable = false,
                    Content = KeyboardViewModel.CreateKeyboardView(this, kb_size, AndroidDisplayInfo.Scaling, out var unscaledSize)
                };
                var cntr2 = (LinearLayout)LayoutInflater.Inflate(Resource.Layout.keyboard_layout_view, null);
                cntr2.AddView(av);
                cntr2.Focusable = false;
                var cntr = new KeyboardLinearLayout(MainActivity.Instance, (int)unscaledSize.Height);
                cntr.AddView(cntr2);
                return cntr;
            } catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            return null;
        }
        public override void OnUpdateSelection(int oldSelStart, int oldSelEnd, int newSelStart, int newSelEnd, int candidatesStart, int candidatesEnd) {
            base.OnUpdateSelection(oldSelStart, oldSelEnd, newSelStart, newSelEnd, candidatesStart, candidatesEnd);
            this.OnCursorChanged?.Invoke(this, EventArgs.Empty);
        }


        private void StartClipboardChecker(Context context, string source)
        {
            Debug.WriteLine($"Clipboard watcher started [{source}]");
            _cbListener = new ClipboardListener(context);
            _cbListener.Start();


            void OnCbChanged(object sender, object e)
            {
                var test = this.CurrentInputConnection.GetSelectedTextFormatted(GetTextFlags.WithStyles);
                Debug.WriteLine($"Clipboard changed [{source}]");
                var pc = _cbListener.ClipboardManager.PrimaryClip;
                for(int i = 0; i < pc.ItemCount; i++)
                {

                }
                var test2 = _cbListener.ClipboardManager.PrimaryClipDescription;
                var test3 = _cbListener.ClipboardManager.Text;
                var test4 = _cbListener.ClipboardManager.TextFormatted.ToString();

            }

            _cbListener.OnClipboardChanged += OnCbChanged;
        }
        void Vibrate() {
            Vibration.Vibrate(MyInputConfig.Instance.VibrationLevel);
        }

        #region IKeyboardInputConnection
        public KeyboardFlags Flags {
            get {
                var kbf = KeyboardFlags.None;
                return kbf;
            }
        }
        (string text,(int start,int len)) GetTextInfo() {
            if (this.CurrentInputConnection == null) {
                return default;
            }
            string pre_text = GetLeadingText(-1);
            string sel_text = this.CurrentInputConnection.GetSelectedText(0) ?? string.Empty;
            string post_text = GetTrailingText(-1);
            string total_text = pre_text + sel_text + post_text;
            int sel_len = sel_text.Length == total_text.Length ? 0 : sel_text.Length;
            return (total_text, (pre_text.Length, sel_len));
        }

        string GetTextAroundCursor(int n, bool isBefore) {
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
                if (cur_count == n) {
                    break;
                }
                cur_count++;
            }
            return last_text;
        }
        string GetTrailingText(int n) =>
            GetTextAroundCursor(n, false);
        public string GetLeadingText(int n) =>
            GetTextAroundCursor(n, true);
        void IKeyboardInputConnection.OnText(string text)
        {
            if(this.CurrentInputConnection == null)
            {
                return;
            }

            this.CurrentInputConnection.CommitText(text, 1);
        }

        void IKeyboardInputConnection.OnDelete()
        {
            if(this.CurrentInputConnection == null)
            {
                return;
            }
            string selectedText = this.CurrentInputConnection.GetSelectedText(0);

            if(string.IsNullOrEmpty(selectedText))
            {
                this.CurrentInputConnection.DeleteSurroundingText(1, 0);
            } else
            {
                this.CurrentInputConnection.CommitText(string.Empty, 1);
            }
        }

        void IKeyboardInputConnection.OnDone()
        {
            if(this.CurrentInputConnection == null)
            {
                return;
            }
            this.CurrentInputConnection.SendKeyEvent(new KeyEvent(KeyEventActions.Down, Keycode.Enter));
            this.CurrentInputConnection.SendKeyEvent(new KeyEvent(KeyEventActions.Up, Keycode.Enter));
        }

        void IKeyboardInputConnection.OnNavigate(int dx, int dy) {
            if(this.CurrentInputConnection == null) {
                return;
            }
            Debug.WriteLine($"CC: {dx} {dy}");
            //var ti = GetTextInfo();
            //int line_count = 1 + ti.text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).Length;

            int? x_dir = dx == 0 ? null : dx > 0 ? 1 : -1;
            int? y_dir = dy == 0 ? null : dy > 0 ? 1 : -1;
            //int? new_start_idx = default;
            if (x_dir.HasValue) {
                for (int i = 0; i < (int)Math.Abs(dx); i++) {
                    //if (new_start_idx == null) {
                    //    new_start_idx = x_dir > 0 ? ti.Item2.start + ti.Item2.len : ti.Item2.start;
                    //}
                    //new_start_idx += x_dir;
                    //if(new_start_idx < 0 || new_start_idx >= ti.text.Length) {
                    //    break;
                    //}
                    //new_start_idx = Math.Clamp(new_start_idx.Value, 0, ti.text.Length);

                    //this.CurrentInputConnection.SetSelection(new_start_idx.Value, new_start_idx.Value);
                    //Debug.WriteLine($"SelIdx: {new_start_idx.Value}");
                    Keycode kc = x_dir > 0 ? Keycode.DpadLeft : Keycode.DpadRight;
                    //this.CurrentInputConnection.SendKeyEvent(new KeyEvent(KeyEventActions.Down, kc));
                    //this.CurrentInputConnection.SendKeyEvent(new KeyEvent(KeyEventActions.Up, kc));
                    this.SendDownUpKeyEvents(kc);
                }
            }
            if (y_dir.HasValue) {
                //int min_edge_offset = 30;
                for (int i = 0; i < (int)Math.Abs(dy); i++) {
                    //if (new_start_idx == null) {
                    //    new_start_idx = y_dir > 0 ? ti.Item2.start + ti.Item2.len : ti.Item2.start;
                    //}
                    //if(y_dir < 0 && new_start_idx < min_edge_offset) {
                    //    // trying to go up

                    //    break;
                    //}
                    //new_start_idx += y_dir;
                    //if (new_start_idx < 0 || new_start_idx >= ti.text.Length) {
                    //    break;
                    //}
                    Keycode kc = y_dir > 0 ? Keycode.DpadDown : Keycode.DpadUp;
                    //this.CurrentInputConnection.SendKeyEvent(new KeyEvent(KeyEventActions.Down, kc));
                    //this.CurrentInputConnection.SendKeyEvent(new KeyEvent(KeyEventActions.Up, kc));
                    this.SendDownUpKeyEvents(kc);
                }
            }
        }
        void IKeyboardInputConnection.OnVibrateRequest() =>
            Vibrate();

        public event EventHandler OnCursorChanged;
        #endregion
    }
}
