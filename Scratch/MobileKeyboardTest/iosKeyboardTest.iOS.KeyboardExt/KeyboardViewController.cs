using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.iOS;
using Avalonia.Media;
using Avalonia.Threading;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UIKit;

using static System.Net.Mime.MediaTypeNames;
using Thickness = Avalonia.Thickness;

namespace iosKeyboardTest.iOS.KeyboardExt {
#pragma warning disable CA1010
    public partial class KeyboardViewController : UIInputViewController, IKeyboardInputConnection_ios, IAvaloniaViewController 
        {
#pragma warning restore CA1010

        #region Private Variables
        UIButton nextKeyboardButton = null;
        UIButton showErrorButton = null;
        UIButton replaceViewButton = null;
        UIStackView outerStackView = null;
        UIStackView innerStackView = null;
        UITextView noFullAccessLabelView = null;

        CGSize keyboardSize = new CGSize(5000, 300);
        KeyboardView kb { get; set; }
        AvaloniaView kbv { get; set; }
        AppDelegate ad { get; set; }

        string _error;
        string error {
            get => string.IsNullOrEmpty(_error) ? "No error":_error;
            set {
                var val = value ?? string.Empty;
                _error = val + Environment.NewLine + _error;
                if(this.HasFullAccess && 
                    kb != null && 
                    kb.DataContext is KeyboardMainViewModel kbmvm &&
                    kbmvm.KeyboardViewModel is { } kbvm) {
                    kbvm.SetError(val);
                    return;
                } 
                if(showErrorButton != null) {
                    showErrorButton.SetTitle(_error, UIControlState.Normal);
                    showErrorButton.SizeToFit();
                }

                //string fn = $"{DateTime.Now.Ticks}.log";
                
                //string errPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), fn);
                //if (ObjCRuntime.Runtime.Arch != Arch.DEVICE) {
                //    errPath = Path.Combine("Users", "tkefauver", "Desktop", fn);
                //}
                //File.WriteAllText(errPath, _error);
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
        #endregion

        #region Public Methods
        public override void LoadView() {
            base.LoadView();
            //InitAvAndKeyboard();
        }
        public override void ViewDidLoad() {
            base.ViewDidLoad();
            
            //DispatchQueue.DefaultGlobalQueue.DispatchAfter(
            //    new DispatchTime((ulong)TimeSpan.FromMilliseconds(500).Nanoseconds), () => {
                    //DispatchQueue.MainQueue.DispatchAsync(() => {
                        //await Task.Delay(500);
                        InitAvAndKeyboard();
                        if (this.HasFullAccess) {
                            SetupKeyboardView();
                        } else {
                            SetupFallbackView();
                        }
                   // });
                //});
            
        }
        public override void ViewDidDisappear(bool animated) {
            base.ViewDidDisappear(animated);
            
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        void SetupFallbackView() {
            try {
                AddOuterContainer();
                AddFullAcccessLabel();
                AddInnerContiner();
                AddButtons();
            } catch (Exception ex) {
                error = ex.ToString();
            }
        }
        void SetupKeyboardView() {
            try {
                AddOuterContainer();
                AddInnerContiner();
                AddButtons();
            }
            catch (Exception ex) {
                error = ex.ToString();
            }
        }

        #region UI Setup
        void RemoveAllSubviews() {
            foreach (var sv in View.Subviews) {
                sv.RemoveFromSuperview();
            }
        }
        private void InitAvAndKeyboard() {
            if (App.IsInitialized) {
                return;
            }

            try {
                App.OnInitialized += (s, e) => {
                    error = "AV INITIALIZED";
                };
                ad = new AppDelegate();
                ad.FinishedLaunching(UIApplication.SharedApplication, null);

                if (this.HasFullAccess) {
                    kb = KeyboardMainViewModel.CreateKeyboardView(this, iosDisplayInfo.ScaledSize, iosDisplayInfo.Scaling, out var unscaledSize);
                    keyboardSize = new CGSize(unscaledSize.Width / iosDisplayInfo.Scaling, unscaledSize.Height / iosDisplayInfo.Scaling);
                    //keyboardSize = new CGSize(unscaledSize.Width * iosDisplayInfo.Scaling, unscaledSize.Height * iosDisplayInfo.Scaling);

                    kbv = new AvaloniaView() {
                        BackgroundColor = UIColor.Blue,
                        Content = kb
                    };
                } else {
                    kbv = new AvaloniaView() {
                        Content = new Avalonia.Controls.Border() {
                            Background = Brushes.Pink,
                            Width = 600,
                            Height = 600
                        }
                    };
                }
                //kbv.InitWithController(this);

            }
            catch (Exception ex) {
                error = ex.ToString();
            }
        }
        void AddKeyboard(bool replaceView = false) {
            //if (outerStackView.Subviews.Contains(kbv)) {
            //    return;
            //}
            double pre_mem = GetMemory();
            if (kbv == null) {
                InitAvAndKeyboard();
            }
            kbv.TranslatesAutoresizingMaskIntoConstraints = false;

            if (replaceView) {
                View.BackgroundColor = UIColor.Orange;
                if(kb.DataContext is KeyboardMainViewModel kbmvm) {
                    kbmvm.ForceSize(new Size(keyboardSize.Width*2,keyboardSize.Height));
                }
                //View = kbv;

                outerStackView.AddArrangedSubview(kbv);
                NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]{
                    kbv.HeightAnchor.ConstraintGreaterThanOrEqualTo(keyboardSize.Height),
                    kbv.WidthAnchor.ConstraintGreaterThanOrEqualTo(keyboardSize.Width),
                    });

                foreach (var sv in outerStackView.Subviews) {
                    if (sv == kbv) {
                        continue;
                    }
                    sv.RemoveFromSuperview();
                    //sv.Dispose();
                }

            } else {
                outerStackView.InsertArrangedSubview(kbv, 0);
                NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]{
                    kbv.HeightAnchor.ConstraintGreaterThanOrEqualTo(keyboardSize.Height),
                    kbv.WidthAnchor.ConstraintGreaterThanOrEqualTo(keyboardSize.Width),
                    });
            }
            
            double post_mem = GetMemory();
            error = $"W: {keyboardSize.Width} H: {keyboardSize.Height}";
            //error = $"Pre: {pre_mem}mb Post: {post_mem}mb Diff: {post_mem - pre_mem}mb";
        }
        void AddFullAcccessLabel() {
            //if (outerStackView.Subviews.Contains(noFullAccessLabelView)) {
            //    return;
            //}
            noFullAccessLabelView = new UITextView();
            noFullAccessLabelView.Text = "Full access is required. Enable in Settings->General->Keyboard->iosKeyboardTest";
            noFullAccessLabelView.TextContainer.LineBreakMode = UILineBreakMode.WordWrap;
            noFullAccessLabelView.Font = UIFont.PreferredHeadline;
            noFullAccessLabelView.TranslatesAutoresizingMaskIntoConstraints = false;
            outerStackView.AddArrangedSubview(noFullAccessLabelView);
            NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]{
                    noFullAccessLabelView.HeightAnchor.ConstraintEqualTo(100),
                    noFullAccessLabelView.LeftAnchor.ConstraintEqualTo(View.LeftAnchor),
                    noFullAccessLabelView.RightAnchor.ConstraintEqualTo(View.RightAnchor),
                    });
        }
        void AddOuterContainer() {
            //if(View.Subviews.Contains(outerStackView)) {
            //    return;
            //}
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
        }
        void AddInnerContiner() {
            //if (outerStackView.Subviews.Contains(innerStackView)) {
            //    return;
            //}
            innerStackView = new UIStackView() {
                BackgroundColor = UIColor.Purple,
                Axis = UILayoutConstraintAxis.Horizontal,
                TranslatesAutoresizingMaskIntoConstraints = false,
                Spacing = 5
            };
            outerStackView.AddArrangedSubview(innerStackView);
            NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]{
                innerStackView.HeightAnchor.ConstraintEqualTo(75),
                innerStackView.LeftAnchor.ConstraintEqualTo(outerStackView.LeftAnchor),
                innerStackView.RightAnchor.ConstraintEqualTo(outerStackView.RightAnchor),
                });
        }
        void AddButtons() {
            AddNextKeyboardButton();
            AddDebugButton();
            AddReplaceViewButton();
        }
        void AddNextKeyboardButton() {
            //if (innerStackView.Subviews.Contains(nextKeyboardButton)) {
            //    return;
            //}
            nextKeyboardButton = new UIButton(UIButtonType.System);
            string title = kbv == null ? "NO KB" : "YUUUUUUP";
            nextKeyboardButton.SetTitle(title, UIControlState.Normal);
            nextKeyboardButton.SizeToFit();
            nextKeyboardButton.TranslatesAutoresizingMaskIntoConstraints = false;
            nextKeyboardButton.AddTarget(this, new Selector("advanceToNextInputMode"), UIControlEvent.TouchUpInside);
            innerStackView.AddArrangedSubview(nextKeyboardButton);
            //NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]{
            //    nextKeyboardButton.LeftAnchor.ConstraintEqualTo(innerStackView.LeftAnchor),
            //    nextKeyboardButton.TopAnchor.ConstraintEqualTo(innerStackView.TopAnchor),
            //    nextKeyboardButton.WidthAnchor.ConstraintEqualTo(150),
            //    nextKeyboardButton.HeightAnchor.ConstraintEqualTo(150),
            //    });

            
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
                AddKeyboard();
            };
            innerStackView.AddArrangedSubview(showErrorButton);
            //NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]{
            //    showErrorButton.LeftAnchor.ConstraintEqualTo(innerStackView.LeftAnchor),
            //    showErrorButton.TopAnchor.ConstraintEqualTo(innerStackView.TopAnchor),
            //    showErrorButton.BottomAnchor.ConstraintEqualTo(innerStackView.BottomAnchor),
            //    });
        }
        void AddReplaceViewButton() {
            //if (innerStackView.Subviews.Contains(showErrorButton)) {
            //    return;
            //}
            replaceViewButton = new UIButton(UIButtonType.System);
            replaceViewButton.SetTitle(error, UIControlState.Normal);
            replaceViewButton.SizeToFit();
            replaceViewButton.TranslatesAutoresizingMaskIntoConstraints = false;
            replaceViewButton.TouchUpInside += (s, e) => {
                AddKeyboard(true);
            };
            innerStackView.AddArrangedSubview(replaceViewButton);
            //NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]{
            //    showErrorButton.LeftAnchor.ConstraintEqualTo(innerStackView.LeftAnchor),
            //    showErrorButton.TopAnchor.ConstraintEqualTo(innerStackView.TopAnchor),
            //    showErrorButton.BottomAnchor.ConstraintEqualTo(innerStackView.BottomAnchor),
            //    });
        }

        #endregion

        int GetMemory() {
            /*
            System.Diagnostics.Process proc = ...; // assign your process here :-)

int memsize = 0; // memsize in KB
PerformanceCounter PC = new PerformanceCounter();
PC.CategoryName = "Process";
PC.CounterName = "Working Set - Private";
PC.InstanceName = proc.ProcessName;
memsize = Convert.ToInt32(PC.NextValue()) / (int)(1024);
PC.Close();
PC.Dispose();
}
}
            */
            //var result = NSProcessInfo.ProcessInfo.PhysicalMemory;
            //double mb = (double)result / (double)1_048_576;
            //return (int)mb;
            return 0;
            //System.Diagnostics.Process proc = Process.GetCurrentProcess();
            //double mb = (double)proc.WorkingSet64 / (double)1_048_576;
            //return (int)mb;

            //return (int)result;
            //int memsize = 0; // memsize in KB
            //PerformanceCounter PC = new PerformanceCounter();
            //PC.CategoryName = "Process";
            //PC.CounterName = "Working Set - Private";
            //PC.InstanceName = proc.ProcessName;
            //memsize = Convert.ToInt32(PC.NextValue()) / (int)(1024);
            //PC.Close();
            //PC.Dispose();

        }

        #endregion

        #region Commands
        #endregion

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
        public override void DidReceiveMemoryWarning() {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning();
            error = "MEMORY WARNING";
            RemoveAllSubviews();
            SetupFallbackView();
            // Release any cached data, images, etc that aren't in use.
        }
        public override void UpdateViewConstraints() {
            base.UpdateViewConstraints();

            // Add custom view sizing constraints here
        }
        //public KeyboardViewController() : base() {
        //    InitAvAndKeyboard();
        //}
        //public KeyboardViewController(NSCoder coder) :base(coder) {
        //    InitAvAndKeyboard();
        //}
        //public KeyboardViewController(string? nibName, NSBundle? bundle) : base(nibName,bundle) {

        //    InitAvAndKeyboard();
        //}

        //protected KeyboardViewController(NSObjectFlag t) : base(t) {

        //    InitAvAndKeyboard();
        //}

        public KeyboardViewController(IntPtr handle) : base(handle) {
            // don't do stuff here
            //InitAvAndKeyboard();
        }
    }

}
