using Avalonia;
using Avalonia.iOS;
using Avalonia.Media;
using Avalonia.Threading;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using System;
using UIKit;

namespace iosKeyboardTest.iOS.KeyboardExt {
    [Register("KeyboardViewController")]
    public class KeyboardViewController : UIInputViewController, iosIKeyboardInputConnection,IAvaloniaViewController {    
        #region Private Variables
        UIButton nextKeyboardButton;
        UIStackView stv;
        UIView kbv; 
        
        private UIStatusBarStyle? _preferredStatusBarStyle;
        private bool? _prefersStatusBarHidden;
        //UIView av;
        //DateTime ctorDt = default, loadDt = default;
        #endregion

        #region Constants
        #endregion

        #region Statics
        //static AvaloniaView KeyboardView { get; set; }
        //static KeyboardViewModel KeyboardViewModel =>
        //    KeyboardView == null || KeyboardView.Content == null ?
        //        null :
        //        (KeyboardView.Content as Control).DataContext as KeyboardViewModel;

        AvaloniaView CreateKeyboardView() {
            //var ui_kbv = Dispatcher.UIThread.Invoke(() => {
                //var kbv = KeyboardViewModel.CreateKeyboardView(this, iosDisplayInfo.ScaledSize);
                var ui_kbv = new AvaloniaView() {
                    //Content = kbv
                    Content = new Avalonia.Controls.Border() { Width = 600, Height = 600, Background = Brushes.Brown}
                };
                //var kb_size = new CGSize(kbv.Width * iosDisplayInfo.Scaling, kbv.Height * iosDisplayInfo.Scaling);
                //var kb_size = new CGSize(5000,5000);
                //ui_kbv.Frame = new CGRect(new(), kb_size);
                return ui_kbv;
            //});
            //return ui_kbv;
        }
        #endregion

        #region Interfaces

        #region IAvaloniaViewController Implementation

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
        bool iosIKeyboardInputConnection.NeedsInputModeSwitchKey =>
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

        public override void ViewDidLoad() {
            base.ViewDidLoad();
            //loadDt = DateTime.Now;
            if (View is AvaloniaView) {
                return;
            }
            //View = CreateKeyboardView();
            //View.AddSubview(av);
            //var nextKeyboardButtonLeftSideConstraint = NSLayoutConstraint.Create(av, NSLayoutAttribute.Left, NSLayoutRelation.Equal, View, NSLayoutAttribute.Left, 1.0f, 0.0f);
            //var nextKeyboardButtonBottomConstraint = NSLayoutConstraint.Create(av, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, View, NSLayoutAttribute.Bottom, 1.0f, 0.0f);
            //View.AddConstraints(new[] { nextKeyboardButtonLeftSideConstraint, nextKeyboardButtonBottomConstraint });


            nextKeyboardButton = new UIButton(UIButtonType.System);
            nextKeyboardButton.SetTitle($"Startup time: ", UIControlState.Normal);
            nextKeyboardButton.SizeToFit();
            nextKeyboardButton.TranslatesAutoresizingMaskIntoConstraints = false;
            nextKeyboardButton.AddTarget(this, new Selector("advanceToNextInputMode"), UIControlEvent.TouchUpInside);

            //Console.WriteLine($"ViewDidLoad called. View Rect: '{View.Bounds}'");
            stv = new UIStackView() {
                BackgroundColor = UIColor.Magenta,
                Axis = UILayoutConstraintAxis.Vertical,
                //TranslatesAutoresizingMaskIntoConstraints = false,
            };

            //var kbv = CreateKeyboardView();
            //stv.AddArrangedSubview(kbv);
            //var size = KeyboardView.Frame.Size;
            //stv.AddArrangedSubview(kbv);
            Dispatcher.UIThread.Invoke(() => {
                View = CreateKeyboardView();
                View.BackgroundColor = UIColor.Purple;
                //kbv.Frame = new CGRect(0, 0, 1000, 1000);
                //stv.AddSubview(kbv);
                //View.AddConstraints(new NSLayoutConstraint[] {
                //    CreateConstraint(kbv, View, NSLayoutAttribute.Left),
                //    //CreateConstraint(kbv, View, NSLayoutAttribute.Top),
                //    CreateConstraint(kbv, View, NSLayoutAttribute.Right),
                //    CreateConstraint(kbv, View, NSLayoutAttribute.Bottom)
                //});
                (View as AvaloniaView).InitWithController(this);
                //kbv.SizeToFit();
                //stv.AddArrangedSubview(nextKeyboardButton);

            });
            //stv.AddArrangedSubview(nextKeyboardButton);
            ////stv.Frame = new CGRect(0, 0, size.Width, size.Height + 50);
            ////stv.Frame = new CGRect(0, 0, 5000,5000);
            ////stv.Frame = kbv.Frame;

            //var subView = stv;

            //View.AddSubview(subView);
            ////View.Frame = new CGRect(0, 0, 500, 500);
            //View.BackgroundColor = UIColor.Orange;
            //View.AddConstraints(new NSLayoutConstraint[] {
            //    CreateConstraint(subView, View, NSLayoutAttribute.Left),
            //    CreateConstraint(subView, View, NSLayoutAttribute.Top),
            //    CreateConstraint(subView, View, NSLayoutAttribute.Right),
            //    CreateConstraint(subView, View, NSLayoutAttribute.Bottom)
            //});
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

        private NSLayoutConstraint CreateConstraint(UIView view, UIView parentView, NSLayoutAttribute attr) {
            return NSLayoutConstraint.Create(view, attr, NSLayoutRelation.Equal, parentView, attr, 1f, 0f);
        }
        #endregion

        #region Commands
        #endregion
    }

    public class AvaloniaInputViewController : UIInputViewController, IAvaloniaViewController {
        private UIStatusBarStyle? _preferredStatusBarStyle;
        private bool? _prefersStatusBarHidden;

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public Thickness SafeAreaPadding { get; private set; }

        /// <inheritdoc/>
        public event EventHandler SafeAreaPaddingChanged;
    }
}
