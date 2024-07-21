
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

        UIStackView outerStackView = null;
        KeyboardView KeyboardView { get; set; }
        public override void ViewDidLoad() {
            base.ViewDidLoad();

            try {
                AddOuterContainer();
                AddKeyboard();

                View.NeedsUpdateConstraints();
                View.Layer.SetNeedsDisplay();
                View.Layer.DisplayIfNeeded();

                KeyboardView.OnTouchEvent += (s, e) => {
                    OnPointerChanged?.Invoke(this, e);
                };
            }
            catch(Exception ex) {
                SetError(ex.StackTrace.ToString());
            }
        }
        void AddKeyboard() {
            KeyboardView = new KeyboardView(this);
            KeyboardView.TranslatesAutoresizingMaskIntoConstraints = false;
            outerStackView.AddArrangedSubview(KeyboardView);
            NSLayoutConstraint.ActivateConstraints([
                KeyboardView.WidthAnchor.ConstraintEqualTo((nfloat)KeyboardView.DC.TotalWidth),
                KeyboardView.HeightAnchor.ConstraintEqualTo((nfloat)KeyboardView.DC.TotalHeight),
                ]);
            KeyboardView.Render(true);
        }
        void AddOuterContainer() {
            outerStackView = new UIStackView() {
                BackgroundColor = UIColor.Magenta,
                Axis = UILayoutConstraintAxis.Vertical,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };
            View.AddSubview(outerStackView);
            NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]{
                    //outerStackView.HeightAnchor.ConstraintGreaterThanOrEqualTo(100),
                    outerStackView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                    outerStackView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                    outerStackView.LeftAnchor.ConstraintEqualTo(View.LeftAnchor),
                    outerStackView.RightAnchor.ConstraintEqualTo(View.RightAnchor),
                    });
        }
        void SetError(string er) {
            error = er;
            if(showErrorButton == null) {
                AddDebugButton();
            }
            showErrorButton.SetTitle(error, UIControlState.Normal);
            showErrorButton.SizeToFit();
            OnText(er);
        }
        void AddDebugButton() {
            showErrorButton = new UIButton(UIButtonType.System);
            showErrorButton.SetTitle(error, UIControlState.Normal);
            showErrorButton.SizeToFit();
            showErrorButton.TranslatesAutoresizingMaskIntoConstraints = false;
            showErrorButton.TouchUpInside += (s, e) => {
            };
            View.AddSubview(showErrorButton);
            NSLayoutConstraint.ActivateConstraints([
                showErrorButton.LeftAnchor.ConstraintEqualTo(View.LeftAnchor),
                showErrorButton.RightAnchor.ConstraintEqualTo(View.RightAnchor),
                showErrorButton.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                ]);
        }

        public event EventHandler OnCursorChanged;
        public event EventHandler OnFlagsChanged;
        public event EventHandler OnDismissed;
        public event EventHandler<TouchEventArgs> OnPointerChanged;

        public string GetLeadingText(int offset, int len) {
            if(TextDocumentProxy == null ||
                TextDocumentProxy.DocumentContextBeforeInput is not { } pre_text) {
                return string.Empty;
            }

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
            if(TextDocumentProxy == null) {
                return;
            }
            this.TextDocumentProxy?.DeleteBackward();
        }

        bool IKeyboardInputConnection_ios.NeedsInputModeSwitchKey =>
            true;


        public void OnInputModeSwitched() {
            if (TextDocumentProxy == null) {
                return;
            }
            this.AdvanceToNextInputMode();
        }

        public void OnText(string text) {
            if (TextDocumentProxy == null) {
                return;
            }
            this.TextDocumentProxy?.InsertText(text);
        }

        public void OnDone() {
            this.DismissKeyboard();
        }

        public void OnNavigate(int dx, int dy) {

            if (TextDocumentProxy == null) {
                return;
            }
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

        public KeyboardFlags Flags {
            get {
                var kbf = KeyboardFlags.None;
                kbf |= KeyboardFlags.Portrait | KeyboardFlags.Mobile | KeyboardFlags.PlatformView;

                if(TextDocumentProxy != null) {
                    switch (TextDocumentProxy?.GetKeyboardType()) {
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
                }
                return kbf;
            }
        }
    }

}