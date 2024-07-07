using Android;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Hardware.Lights;
using Android.InputMethodServices;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;

using Avalonia.Android;
using Avalonia.Layout;

using System;
using System.Diagnostics;
using Color = Android.Graphics.Color;
using Exception = System.Exception;
using View = Android.Views.View;

namespace iosKeyboardTest.Android
{
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
                var av = new AvaloniaView(MainActivity.Instance)
                {
                    Focusable = false,
                    Content = KeyboardMainViewModel.CreateKeyboardView(this, AndroidDisplayInfo.ScaledSize, AndroidDisplayInfo.Scaling, out var unscaledSize)
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
        #region IKeyboardInputConnection

        public void OnText(string text)
        {
            if(this.CurrentInputConnection == null)
            {
                return;
            }

            this.CurrentInputConnection.CommitText(text, 1);
        }

        public void OnDelete()
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

        public void OnDone()
        {
            if(this.CurrentInputConnection == null)
            {
                return;
            }
            this.CurrentInputConnection.SendKeyEvent(new KeyEvent(KeyEventActions.Down, global::Android.Views.Keycode.Enter));
        }
        #endregion
    }
}
