
using System;
using System.Text;
using UIKit;
namespace iosKeyboardTest.iOS.KeyboardExt {
#pragma warning disable CA1010
    public partial class KeyboardViewController : UIInputViewController, IKeyboardInputConnection_ios, ITriggerTouchEvents {
#pragma warning restore CA1010
        string error = string.Empty;
        UIButton showErrorButton;
        UIImpactFeedbackGenerator vibrator; 
        KeyboardView KeyboardView { get; set; }
        public override void ViewDidLoad() {
            base.ViewDidLoad();

            try {
                KeyboardView = new KeyboardView(this);
                View.AddSubview(KeyboardView);

                KeyboardView.OnTouchEvent += (s, e) => {
                    OnPointerChanged?.Invoke(this, e);
                };
            }
            catch(Exception ex) {
                SetError(ex.Message);
            }
        }
        void SetError(string er) {
            error = er;
            if(showErrorButton == null) {
                AddDebugButton();
            }
            showErrorButton.SetTitle(error, UIControlState.Normal);
            showErrorButton.SizeToFit();
        }
        void AddDebugButton() {
            //if (innerStackView.Subviews.Contains(showErrorButton)) {
            //    return;
            //}
            showErrorButton = new UIButton(UIButtonType.System);
            showErrorButton.SetTitle(error, UIControlState.Normal);
            showErrorButton.SizeToFit();
            showErrorButton.TranslatesAutoresizingMaskIntoConstraints = false;
            showErrorButton.TouchUpInside += (s, e) => {
            };
            View.AddSubview(showErrorButton);
        }

        public event EventHandler OnCursorChanged;
        public event EventHandler OnFlagsChanged;
        public event EventHandler OnDismissed;
        public event EventHandler<TouchEventArgs> OnPointerChanged;

        public string GetLeadingText(int offset, int len) {
            string pre_text = TextDocumentProxy?.DocumentContextBeforeInput;
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
            this.TextDocumentProxy?.DeleteBackward();
        }


        public void OnInputModeSwitched() {
            this.AdvanceToNextInputMode();
        }

        public void OnText(string text) {
            this.TextDocumentProxy?.InsertText(text);
        }

        public void OnDone() {
            this.DismissKeyboard();
        }

        public void OnNavigate(int dx, int dy) {

            this.TextDocumentProxy?.AdjustTextPositionByCharacterOffset(dx);
        }

        public void OnFeedback(KeyboardFeedbackFlags flags) {
            if (flags.HasFlag(KeyboardFeedbackFlags.Vibrate)) {
                // from https://stackoverflow.com/a/78032485/105028
                if (vibrator == null) {
                    vibrator = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Light);
                }
                vibrator.ImpactOccurred();                
            }
        }

        KeyboardFlags IKeyboardInputConnection.Flags {
            get {
                var kbf = KeyboardFlags.None;
                kbf |= KeyboardFlags.Portrait | KeyboardFlags.Mobile | KeyboardFlags.PlatformView;

                switch(TextDocumentProxy?.GetKeyboardType()) {
                    case UIKeyboardType.NumberPad:
                    case UIKeyboardType.NumbersAndPunctuation:
                    case UIKeyboardType.DecimalPad:
                    case UIKeyboardType.AsciiCapableNumberPad:
                    case UIKeyboardType.PhonePad:
                        kbf |= KeyboardFlags.Numbers;
                        break;
                    case UIKeyboardType.EmailAddress:
                        kbf |= KeyboardFlags.Email;
                        break;
                    case UIKeyboardType.WebSearch:
                        kbf |= KeyboardFlags.Search;
                        break;
                    case UIKeyboardType.Url:
                        kbf |= KeyboardFlags.Url;
                        break;
                    default:
                        kbf |= KeyboardFlags.FreeText;
                        break;
                }
                return kbf;
            }
        }
    }

}