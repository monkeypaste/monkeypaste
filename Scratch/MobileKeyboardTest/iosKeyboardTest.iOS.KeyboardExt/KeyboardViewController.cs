using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.iOS;
using Avalonia.Media;
using CoreFoundation;
using CoreGraphics;
using ObjCRuntime;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UIKit;
using Thickness = Avalonia.Thickness;

namespace iosKeyboardTest.iOS.KeyboardExt {
#pragma warning disable CA1010
    public partial class KeyboardViewController : UIInputViewController, IKeyboardInputConnection_ios, IAvaloniaViewController {
#pragma warning restore CA1010

        #region Private Variables
        UIButton nextKeyboardButton = null;
        UIButton showErrorButton = null;
        UIStackView outerStackView = null;
        UIStackView innerStackView = null;
        UITextView noFullAccessLabelView = null;

        CGSize keyboardSize = new CGSize(1000, 300);
        KeyboardView kb = null;
        AvaloniaView kbv = null;
        AppDelegate ad = null;

        string error {
            get => MainViewModel.ErrorText;
            set {
                var val = value ?? string.Empty;
                MainViewModel.ErrorText += Environment.NewLine + val;
                if(showErrorButton != null) {
                    showErrorButton.SetTitle(MainViewModel.ErrorText, UIControlState.Normal);
                }
            }
        }

        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces

        #region IAvaloniaViewController Implementation

        private UIStatusBarStyle? _preferredStatusBarStyle;
        private bool? _prefersStatusBarHidden;
        public Thickness SafeAreaPadding { get; private set; }

        public event EventHandler SafeAreaPaddingChanged;

        public override void ViewDidLayoutSubviews() {
            base.ViewDidLayoutSubviews();
            var size = View?.Frame.Size ?? default;
            var frame = View?.SafeAreaLayoutGuide.LayoutFrame ?? default;
            var safeArea = new Thickness(frame.Left, frame.Top, size.Width - frame.Right, size.Height - frame.Bottom);
            if (SafeAreaPadding != safeArea) {
                SafeAreaPadding = safeArea;
                SafeAreaPaddingChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <inheritdoc/>
        public override bool PrefersStatusBarHidden() {
            return _prefersStatusBarHidden ??= base.PrefersStatusBarHidden();
        }

        /// <inheritdoc/>
        public override UIStatusBarStyle PreferredStatusBarStyle() {
            // don't set _preferredStatusBarStyle value if it's null, so we can keep "default" there instead of actual app style.
            return _preferredStatusBarStyle ?? base.PreferredStatusBarStyle();
        }

        UIStatusBarStyle IAvaloniaViewController.PreferredStatusBarStyle {
            get => _preferredStatusBarStyle ?? UIStatusBarStyle.Default;
            set {
                _preferredStatusBarStyle = value;
                SetNeedsStatusBarAppearanceUpdate();
            }
        }

        bool IAvaloniaViewController.PrefersStatusBarHidden {
            get => _prefersStatusBarHidden ?? false; // false is default on ios/ipados
            set {
                _prefersStatusBarHidden = value;
                SetNeedsStatusBarAppearanceUpdate();
            }
        }
        #endregion

        #region iosIKeyboardInputConnection Implementation
        bool IKeyboardInputConnection_ios.NeedsInputModeSwitchKey =>
            this.NeedsInputModeSwitchKey;

        public void OnInputModeSwitched() {
            AdvanceToNextInputMode();
        }
        public void OnText(string text) {
            TextDocumentProxy.InsertText(text);
        }

        public void OnDelete() {
            TextDocumentProxy.DeleteBackward();
        }

        public void OnDone() {
            View.EndEditing(true);
        }

        public void OnNavigate(int deltaX, int deltaY) {
            string all_text =
                TextDocumentProxy.DocumentContextBeforeInput +
                TextDocumentProxy.SelectedText +
                TextDocumentProxy.DocumentContextAfterInput;
            //var lines = all_text.Split(new string[] {Environment.NewLine})
            //TextDocumentProxy.AdjustTextPositionByCharacterOffset
        }

        #endregion

        #endregion

        #region Properties
        #endregion

        #region Constructors
        public KeyboardViewController(IntPtr handle) : base(handle) {
            // don't do stuff here
        }
        #endregion

        #region Public Methods
        public override void DidReceiveMemoryWarning() {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
        public override void UpdateViewConstraints() {
            base.UpdateViewConstraints();

            // Add custom view sizing constraints here
        }
        public override void LoadView() {
            base.LoadView();
            if(this.HasFullAccess) {
                InitAvAndKeyboard();
            }
            
        }
        public override void ViewDidLoad() {
            base.ViewDidLoad();
            if(this.HasFullAccess) {
                SetupKeyboardView();
            } else {
                SetupFallbackView();
            }
        }
        public override void TextWillChange(IUITextInput textInput) {
            // The app is about to change the document's contents. Perform any preparation here.
            //base.TextWillChange(textInput);
        }

        public override void TextDidChange(IUITextInput textInput) {
            // The app has just changed the document's contents, the document context has been updated.
            UIColor textColor = null;

            if (TraitCollection.UserInterfaceStyle == UIUserInterfaceStyle.Dark) {
                textColor = UIColor.White;
            } else {
                textColor = UIColor.Black;
            }

            nextKeyboardButton.SetTitleColor(textColor, UIControlState.Normal);
            //base.TextDidChange(textInput);
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        private void InitAvAndKeyboard() {
            try {
                ad = new AppDelegate();
                ad.FinishedLaunching(UIApplication.SharedApplication, null);
                //kb = KeyboardMainViewModel.CreateKeyboardView(this, iosDisplayInfo.ScaledSize, iosDisplayInfo.Scaling, out var unscaledSize);
                //keyboardSize = new CGSize(unscaledSize.Width / iosDisplayInfo.Scaling, unscaledSize.Height / iosDisplayInfo.Scaling);
                kbv = new AvaloniaView() {
                    Content = new Avalonia.Controls.Border() {
                        Background = Brushes.Pink,
                        Width = 600,
                        Height = 600
                    }
                };
                //kbv.InitWithController(this);
                //if (Application.Current.ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform) {
                //    singleViewPlatform.MainView = kb;
                //}
            }
            catch (Exception ex) {
                error = string.IsNullOrWhiteSpace(ex.Message) ? "EMPTY ERROR" : ex.Message;
            }
        }
        void AddKeyboardView() {
            try {
                if(kbv == null) {
                    InitAvAndKeyboard();
                }
                outerStackView.AddArrangedSubview(kbv);
                NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]{
                        kbv.HeightAnchor.ConstraintGreaterThanOrEqualTo(keyboardSize.Height),
                        });
            }
            catch (Exception ex) {
                //  isKeyboardReady = false;
                error = string.IsNullOrWhiteSpace(ex.Message) ? "EMPTY ERROR" : ex.Message;
            }
        }
        void SetupFallbackView() {
            outerStackView = new UIStackView() {
                BackgroundColor = UIColor.Magenta,
                Axis = UILayoutConstraintAxis.Vertical,
                TranslatesAutoresizingMaskIntoConstraints = false,
                Spacing = 5
            };
            View.AddSubview(outerStackView);

            AddKeyboardView();

            NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]{
                    outerStackView.HeightAnchor.ConstraintGreaterThanOrEqualTo(300),
                    outerStackView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                    outerStackView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                    outerStackView.LeftAnchor.ConstraintEqualTo(View.LeftAnchor),
                    outerStackView.RightAnchor.ConstraintEqualTo(View.RightAnchor),
                    });

            noFullAccessLabelView = new UITextView();
            noFullAccessLabelView.Text = "Full access is required. Enable in Settings->General->Keyboard->iosKeyboardTest";
            noFullAccessLabelView.TextContainer.LineBreakMode = UILineBreakMode.WordWrap;// NSLineBreakByWordWrapping;
            noFullAccessLabelView.Font = UIFont.PreferredHeadline;
            noFullAccessLabelView.TranslatesAutoresizingMaskIntoConstraints = true;
            outerStackView.AddArrangedSubview(noFullAccessLabelView);


            innerStackView = new UIStackView() {
                BackgroundColor = UIColor.Purple,
                Axis = UILayoutConstraintAxis.Horizontal,
                TranslatesAutoresizingMaskIntoConstraints = false,
                Spacing = 5
            };
            outerStackView.AddArrangedSubview(innerStackView);
            NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]{
                innerStackView.LeftAnchor.ConstraintEqualTo(outerStackView.LeftAnchor),
                innerStackView.RightAnchor.ConstraintEqualTo(outerStackView.RightAnchor),
                });


            nextKeyboardButton = new UIButton(UIButtonType.System);
            string title = kbv == null ? "NO KB" : "YUUUUUUP";
            nextKeyboardButton.SetTitle(title, UIControlState.Normal);
            nextKeyboardButton.SizeToFit();
            nextKeyboardButton.AddTarget(this, new Selector("advanceToNextInputMode"), UIControlEvent.TouchUpInside);
            innerStackView.AddArrangedSubview(nextKeyboardButton);


            showErrorButton = new UIButton(UIButtonType.System);
            showErrorButton.SetTitle(error, UIControlState.Normal);
            showErrorButton.SizeToFit();
            showErrorButton.TouchUpInside += (s, e) => {
                var alert = UIAlertController.Create(error, error, UIAlertControllerStyle.Alert);
                var ok = UIAlertAction.Create("OK", UIAlertActionStyle.Default, (x) => {
                    Debug.WriteLine("whatever");
                });
                alert.AddAction(ok);
                this.PresentViewController(alert, true, null);
            };
            innerStackView.AddArrangedSubview(showErrorButton);
        }
        void SetupKeyboardView() {
            outerStackView = new UIStackView() {
                BackgroundColor = UIColor.Magenta,
                Axis = UILayoutConstraintAxis.Vertical,
                TranslatesAutoresizingMaskIntoConstraints = false,
                Spacing = 5
            };
            View.AddSubview(outerStackView);

            AddKeyboardView();

            NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]{
                    outerStackView.HeightAnchor.ConstraintGreaterThanOrEqualTo(300),
                    outerStackView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                    outerStackView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                    outerStackView.LeftAnchor.ConstraintEqualTo(View.LeftAnchor),
                    outerStackView.RightAnchor.ConstraintEqualTo(View.RightAnchor),
                    });

            innerStackView = new UIStackView() {
                BackgroundColor = UIColor.Purple,
                Axis = UILayoutConstraintAxis.Horizontal,
                TranslatesAutoresizingMaskIntoConstraints = false,
                Spacing = 5
            };
            outerStackView.AddArrangedSubview(innerStackView);
            NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]{
                innerStackView.LeftAnchor.ConstraintEqualTo(outerStackView.LeftAnchor),
                innerStackView.RightAnchor.ConstraintEqualTo(outerStackView.RightAnchor),
                });


            nextKeyboardButton = new UIButton(UIButtonType.System);
            string title = kbv == null ? "NO KB" : "YUUUUUUP";
            nextKeyboardButton.SetTitle(title, UIControlState.Normal);
            nextKeyboardButton.SizeToFit();
            nextKeyboardButton.AddTarget(this, new Selector("advanceToNextInputMode"), UIControlEvent.TouchUpInside);
            innerStackView.AddArrangedSubview(nextKeyboardButton);


            showErrorButton = new UIButton(UIButtonType.System);
            showErrorButton.SetTitle(error, UIControlState.Normal);
            showErrorButton.SizeToFit();
            showErrorButton.TouchUpInside += (s, e) => {
                var alert = UIAlertController.Create(error, error, UIAlertControllerStyle.Alert);
                var ok = UIAlertAction.Create("OK", UIAlertActionStyle.Default, (x) => {
                    Debug.WriteLine("whatever");
                });
                alert.AddAction(ok);
                this.PresentViewController(alert, true, null);
            };
            innerStackView.AddArrangedSubview(showErrorButton);

            DispatchQueue.GetGlobalQueue(DispatchQueuePriority.Default).DispatchAsync(async () => {
                await Task.Delay(300);
                DispatchQueue.MainQueue.DispatchAsync(() => {
                });
            });
        }

        public override void ViewDidDisappear(bool animated) {
            base.ViewDidDisappear(animated);
            /*
            maybe try something like this if mem issues:

            for subview in view.subviews {
                subview.removeFromSuperview()
            }
            keyboardUIHostingController?.rootView = AnyView(EmptyView())
            */
        }
        #endregion

        #region Commands
        #endregion
    }
}
