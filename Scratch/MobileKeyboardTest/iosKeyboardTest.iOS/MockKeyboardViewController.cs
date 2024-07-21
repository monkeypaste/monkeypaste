using Avalonia;
using CoreGraphics;
using System;
using System.Text;
using UIKit;
using static System.Net.Mime.MediaTypeNames;

namespace iosKeyboardTest.iOS {
    public partial class MockKeyboardViewController : UIViewController, IKeyboardInputConnection_ios, ITriggerTouchEvents {
        KeyboardView KeyboardView { get; set; }
        UITextView InputTextBox { get; set; }
        public override void ViewDidLoad() {
            base.ViewDidLoad();

            InputTextBox = new UITextView();
            //InputTextBox.InputView = new UIView();
            //InputTextBox.InputAccessoryView = new UIView();
            InputTextBox.Editable = true;
            InputTextBox.Text = "Whats up yo";
            double w1 = 300;
            double h1 = 300;
            double x1 = (UIScreen.MainScreen.Bounds.Width / 2) - (w1/2);
            double y1 = h1 / 2;
            InputTextBox.Frame = new CGRect(x1, y1, w1, h1);
            View.AddSubview(InputTextBox);


            var tbb = new UIButton(UIButtonType.System);
            tbb.SetTitle("Test", UIControlState.Normal);
            tbb.SizeToFit();
            tbb.BackgroundColor = UIColor.Purple;
            tbb.Layer.CornerRadius = 10;
            tbb.TranslatesAutoresizingMaskIntoConstraints = false;
            double w2 = 100;
            double h2 = 40;
            double x2 = (UIScreen.MainScreen.Bounds.Width / 2) - (w2 / 2);
            double y2 = y1 + h1 + 10;
            tbb.Frame = new CGRect(x2, y2, w2, h2);
            View.AddSubview(tbb);
            tbb.TouchUpInside += (s, e) => {
                if(KeyboardView == null) {
                    return;
                }
                KeyboardView.Render(true);
            };

            //KeyboardView = new KeyboardView(this);
            //View.AddSubview(KeyboardView);

            //KeyboardView.OnTouchEvent += (s, e) => {
            //    OnPointerChanged?.Invoke(this, e);
            //};
        }

        public bool NeedsInputModeSwitchKey { get; }

        public void OnInputModeSwitched() {
            //thrownew NotImplementedException();
        }

        public event EventHandler OnCursorChanged;
        public event EventHandler OnFlagsChanged;
        public event EventHandler OnDismissed;

        public string GetLeadingText(int offset, int len) {
            if (InputTextBox == null) {
                return string.Empty;
            }
            string pre_text = InputTextBox.Text.Substring(0, (int)InputTextBox.GetOffsetFromPosition(InputTextBox.BeginningOfDocument, InputTextBox.SelectedTextRange.Start));
            if (offset < 0) {
                offset = pre_text.Length;
            }
            var sb = new StringBuilder();
            for (int i = 0; i < offset; i++) {
                int pre_text_idx = pre_text.Length - 1 - i;
                if (pre_text_idx < 0) {
                    break;
                }
                sb.Insert(0, pre_text[pre_text_idx]);
            }
            return sb.ToString();
        }

        public void OnText(string text) {
            if (InputTextBox == null) {
                return;
            }
            InputTextBox.ReplaceText(InputTextBox.SelectedTextRange, text);
        }

        public void OnBackspace(int count) {
            if (InputTextBox == null) {
                return;
            }
            var rng = InputTextBox.SelectedTextRange;
            
            int sidx = Math.Max(0, (int)InputTextBox.GetOffsetFromPosition(InputTextBox.BeginningOfDocument, rng.Start));
            int eidx = Math.Max(0, (int)InputTextBox.GetOffsetFromPosition(InputTextBox.BeginningOfDocument, rng.End));
            int len = Math.Max(0, eidx - sidx);
            if (len == 0) {
                if (sidx == 0) {
                    return;
                }
                int old_idx = sidx;
                InputTextBox.Text = InputTextBox.Text.Substring(0, sidx - 1) + InputTextBox.Text.Substring(eidx);
                //InputTextBox.CaretIndex = Math.Max(0, old_idx - 1);
                var new_pos = InputTextBox.GetPosition(rng.Start, -1);
                if(new_pos != null) {
                    InputTextBox.SelectedTextRange = InputTextBox.GetTextRange(new_pos, new_pos);
                }
            } else {
                InputTextBox.ReplaceText(InputTextBox.SelectedTextRange, string.Empty);
            }
        }

        public void OnDone() {
            //thrownew NotImplementedException();
        }

        public void OnNavigate(int dx, int dy) {
            //thrownew NotImplementedException();
        }

        public void OnFeedback(KeyboardFeedbackFlags flags) {
            //throw new NotImplementedException();
        }

        public KeyboardFlags Flags { 
            get {
                var kbf = KeyboardFlags.None;
                kbf |= KeyboardFlags.Mobile;
                kbf |= KeyboardFlags.PlatformView;
                kbf |= KeyboardFlags.FreeText;
                kbf |= KeyboardFlags.iOS;
                return kbf;
            }
        }

        public event EventHandler<TouchEventArgs> OnPointerChanged;
    }
}