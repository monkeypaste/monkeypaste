using AuthenticationServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.iOS;
using Avalonia.Media;
using Avalonia.Threading;
using CoreAnimation;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UIKit;
using Thickness = Avalonia.Thickness;

namespace iosKeyboardTest.iOS.KeyboardExt {
#pragma warning disable CA1010
    public partial class KeyboardViewController : UIInputViewController, iosIKeyboardInputConnection, IAvaloniaViewController {    
#pragma warning restore CA1010

        #region Private Variables
        UIButton nextKeyboardButton;
        UIButton showErrorButton;
        UIStackView outerStackView;
        UIStackView innerStackView;

        CGSize keyboardSize = new CGSize(1000, 300);
        MyKeyboardView kb;
        AvaloniaView kbv;
        AppDelegate ad;

        string error = "NO ERROR";

        //UIView av;
        //DateTime ctorDt = default, loadDt = default;
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
        public override void LoadView() {
            base.LoadView();
            InitAv();
        }
        public override void UpdateViewConstraints() {
            base.UpdateViewConstraints();

            // Add custom view sizing constraints here
        }
        
        public override void ViewDidLoad() {
            base.ViewDidLoad();
            SetupKeyboardView();
            //InputView.AddSubview(new AvaloniaView());
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
        private void InitAv() {
            try {
                if (ad == null) {
                    ad = new AppDelegate();
                    ad.FinishedLaunching(UIApplication.SharedApplication, null);
                }
                kb = KeyboardViewModel.CreateKeyboardView(this, iosDisplayInfo.ScaledSize, iosDisplayInfo.Scaling, out var unscaledSize);
                keyboardSize = new CGSize(unscaledSize.Width/ iosDisplayInfo.Scaling, unscaledSize.Height/iosDisplayInfo.Scaling);
                kbv = new AvaloniaView() {
                    Content = kb
                };
                kbv.InitWithController(this);
            }
            catch (Exception ex) {
                error = string.IsNullOrWhiteSpace(ex.Message) ? "EMPTY ERROR" : ex.Message;
            }
        }
        void SetupKeyboardView() {
            outerStackView = new UIStackView() {
                BackgroundColor = UIColor.Magenta,
                Axis = UILayoutConstraintAxis.Vertical,
                TranslatesAutoresizingMaskIntoConstraints = false,
                Spacing = 5
            };
            View.AddSubview(outerStackView);

            NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]{
                    outerStackView.HeightAnchor.ConstraintGreaterThanOrEqualTo(300),
                    outerStackView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                    outerStackView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                    outerStackView.LeftAnchor.ConstraintEqualTo(View.LeftAnchor),
                    outerStackView.RightAnchor.ConstraintEqualTo(View.RightAnchor),
                    });


            try {
                if (kbv == null) {
                    InitAv();
                }
                outerStackView.AddArrangedSubview(kbv);

                NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]{
                    kbv.HeightAnchor.ConstraintGreaterThanOrEqualTo(keyboardSize.Height),
                    });
            }
            catch (Exception ex) {
                error = string.IsNullOrWhiteSpace(ex.Message) ? "EMPTY ERROR" : ex.Message;
            }

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
            //nextKeyboardButton.TranslatesAutoresizingMaskIntoConstraints = false;
            nextKeyboardButton.AddTarget(this, new Selector("advanceToNextInputMode"), UIControlEvent.TouchUpInside);
            innerStackView.AddArrangedSubview(nextKeyboardButton);
            //NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[] {
            //    nextKeyboardButton.LeftAnchor.ConstraintEqualTo(View.LeftAnchor),
            //    nextKeyboardButton.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
            //});


            showErrorButton = new UIButton(UIButtonType.System);
            showErrorButton.SetTitle(error, UIControlState.Normal);
            showErrorButton.SizeToFit();
            //showErrorButton.TranslatesAutoresizingMaskIntoConstraints = false;
            showErrorButton.TouchUpInside += (s, e) => {
                //DispatchQueue.MainQueue.DispatchAsync(() => {
                var alert = UIAlertController.Create(error, error, UIAlertControllerStyle.Alert);
                var ok = UIAlertAction.Create("OK", UIAlertActionStyle.Default, (x) => {
                    Debug.WriteLine("whatever");
                });
                alert.AddAction(ok);
                this.PresentViewController(alert, true, null);
                //});
            };
            innerStackView.AddArrangedSubview(showErrorButton);
            //NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[] {
            //    showErrorButton.RightAnchor.ConstraintEqualTo(View.RightAnchor),
            //    showErrorButton.TopAnchor.ConstraintEqualTo(nextKeyboardButton.TopAnchor),
            //});




            DispatchQueue.GetGlobalQueue(DispatchQueuePriority.Default).DispatchAsync(async () => {
                //var ui_kbv = iosExtAvaloniaViewLoader.AvViewObj as AvaloniaView;
                await Task.Delay(300);

                //var ui_kbv = Dispatcher.UIThread.Invoke(() => {
                //TestAvaloniaView.SharedLayer = View.Layer;
                //var ui_kbv = new AvaloniaView() {
                //    Content = new Avalonia.Controls.Border() {
                //        Width = 600,
                //        Height = 600,
                //        Background = Brushes.Brown
                //    }
                //};
                //    return ui_kbv;
                //});
                DispatchQueue.MainQueue.DispatchAsync(() => {


                    //if(iosExtAvaloniaViewLoader.AvViewObj is IMauiContext ctx) {

                    //    var cv = new Border() {
                    //        WidthRequest = 600,
                    //        HeightRequest = 600,
                    //        BackgroundColor = Colors.Cyan
                    //    }.ToPlatform(ctx);
                    //    InputView.AddSubview(cv);
                    //}

                    //InitAv();

                    //var ui_kbv = new AvaloniaView() {
                    //    Content = new Avalonia.Controls.Border() {
                    //        Width = 600,
                    //        Height = 600,
                    //        Background = Brushes.Brown
                    //    }
                    //};

                    //View.AddSubview(ui_kbv);
                    //NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[] {
                    //    ui_kbv.WidthAnchor.ConstraintGreaterThanOrEqualTo(600),
                    //    ui_kbv.HeightAnchor.ConstraintGreaterThanOrEqualTo(600),
                    //});
                });
            });
        }
        #endregion

        #region Commands
        #endregion
    }
}
