using Avalonia;
using CoreGraphics;
using System;
using System.Text;
using UIKit;
namespace iosKeyboardTest.iOS.KeyboardExt {
#pragma warning disable CA1010
    public partial class KeyboardViewController : UIInputViewController, IKeyboardInputConnection_ios_fallback, ITriggerTouchEvents_fallback {
#pragma warning restore CA1010

        UIImpactFeedbackGenerator vibrator;
        KeyboardView KeyboardView { get; set; }
        public override void ViewDidLoad() {
            base.ViewDidLoad();

            KeyboardView = new KeyboardView(this);
            View.AddSubview(KeyboardView);

            KeyboardView.OnTouchEvent += (s, e) => {
                OnPointerChanged?.Invoke(this, e);
            };
        }


        public event EventHandler OnCursorChanged;
        public event EventHandler OnFlagsChanged;
        public event EventHandler OnDismissed;
        public event EventHandler<TouchEventArgs_fallback> OnPointerChanged;

        public string GetLeadingText(int offset, int len) {
            string pre_text = TextDocumentProxy.DocumentContextBeforeInput;
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

        public void OnBackspace(int count) {
            this.TextDocumentProxy.DeleteBackward();
        }


        public void OnInputModeSwitched() {
            this.AdvanceToNextInputMode();
        }

        public void OnText(string text) {
            this.TextDocumentProxy.InsertText(text);
        }

        public void OnDone() {
            this.DismissKeyboard();
        }

        public void OnNavigate(int dx, int dy) {

            this.TextDocumentProxy.AdjustTextPositionByCharacterOffset(dx);
        }

        public void OnFeedback(KeyboardFeedbackFlags_fallback flags) {
            if (flags.HasFlag(KeyboardFeedbackFlags_fallback.Vibrate)) {
                // from https://stackoverflow.com/a/78032485/105028
                if (vibrator == null) {
                    vibrator = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Light);
                }
                vibrator.ImpactOccurred();                
            }
        }

        KeyboardFlags_fallback IKeyboardInputConnection_fallback.Flags {
            get {
                var kbf = KeyboardFlags_fallback.None;
                kbf |= KeyboardFlags_fallback.Portrait | KeyboardFlags_fallback.Mobile;

                switch(TextDocumentProxy.GetKeyboardType()) {
                    case UIKeyboardType.NumberPad:
                    case UIKeyboardType.NumbersAndPunctuation:
                    case UIKeyboardType.DecimalPad:
                    case UIKeyboardType.AsciiCapableNumberPad:
                    case UIKeyboardType.PhonePad:
                        kbf |= KeyboardFlags_fallback.Numbers;
                        break;
                    case UIKeyboardType.EmailAddress:
                        kbf |= KeyboardFlags_fallback.Email;
                        break;
                    case UIKeyboardType.WebSearch:
                        kbf |= KeyboardFlags_fallback.Search;
                        break;
                    case UIKeyboardType.Url:
                        kbf |= KeyboardFlags_fallback.Url;
                        break;
                    default:
                        kbf |= KeyboardFlags_fallback.FreeText;
                        break;
                }
                return kbf;
            }
        }
    }

}