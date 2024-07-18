using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Shapes;
using Avalonia.iOS;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Rendering;
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
    public partial class KeyboardViewController : UIInputViewController, IKeyboardInputConnection_ios, IAvaloniaViewController {
#pragma warning restore CA1010

        #region Private Variables
        UIButton nextKeyboardButton = null;
        UIButton showErrorButton = null;
        UIButton replaceViewButton = null;

        UITextView noFullAccessLabelView = null;

        UIStackView outerStackView = null;
        UIStackView innerStackView = null;

        CGSize keyboardSize = new CGSize(1000, 300);

        string _error;
        string error {
            get => string.IsNullOrEmpty(_error) ? "No error" : _error;
            set {
                var val = value ?? string.Empty;
                _error = val + Environment.NewLine + _error;
                if (showErrorButton != null) {
                    showErrorButton.SetTitle(_error, UIControlState.Normal);
                    showErrorButton.SizeToFit();
                }

                //string fn = $"{DateTime.Now.Ticks}.log";

                //string errPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), fn);
                //if (true/*ObjCRuntime.Runtime.Arch != Arch.DEVICE*/) {
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

        public event EventHandler<Point?> OnPointerChanged;

        
        KeyboardView kb;
        AvaloniaView kbv { get; set; }
        AppDelegate ad;

        bool is_av_initd = false;
        bool is_kb_created = false;
        bool is_kbv_added = false;
        bool is_kb_added = false;
        void InitAv() {
            try {

                if (!is_av_initd) {
                    ad = new AppDelegate();
                    ad.FinishedLaunching(UIApplication.SharedApplication, null);
                    is_av_initd = true;
                    error = $"Loaded. Mem {GetMemoryMb(true)}mb";
                    return;
                }
                if(!is_kb_created) {
                    var kbs = KeyboardViewModel.GetTotalSizeByScreenSize(iosDisplayInfo.ScaledSize);
                    kb = KeyboardViewModel.CreateKeyboardView(this, kbs, iosDisplayInfo.Scaling, out var unscaledSize);
                    keyboardSize = new CGSize(unscaledSize.Width / iosDisplayInfo.Scaling, unscaledSize.Height / iosDisplayInfo.Scaling);
                    
                    kbv = new AvaloniaView() { 
                        //Content = kb,
                        BackgroundColor = UIColor.Orange,
                        TranslatesAutoresizingMaskIntoConstraints = false
                    };
                    kbv.InitWithController(this);
                    //if(kbv.Content is Control c) {
                    //    c.Loaded += (s, e) => {
                    //        error = "Loaded";
                    //    };
                    //    c.EffectiveViewportChanged += (s, e) => {
                    //        error = $"Viewport: {c.Bounds}";
                    //    };
                    //}
                    is_kb_created = true;
                    error = $"Created. Mem {GetMemoryMb(true)}mb";
                    return;
                }
                if(!is_kbv_added) {
                    outerStackView.InsertArrangedSubview(kbv, 0);
                    NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]{
                    kbv.HeightAnchor.ConstraintEqualTo(keyboardSize.Height),
                    kbv.WidthAnchor.ConstraintEqualTo(keyboardSize.Width),
                    //kbv.LeftAnchor.ConstraintEqualTo(View.LeftAnchor),
                    //kbv.RightAnchor.ConstraintEqualTo(View.RightAnchor),
                    });
                    is_kbv_added = true;
                    error = $"Added1. Mem {GetMemoryMb()}mb";
                    return;
                }
                if(!is_kb_added) {
                    kbv.Content = kb;
                    is_kb_added = true;
                    error = $"Added2. Mem {GetMemoryMb()}mb";
                }
                
                
            }
            catch (Exception ex) {
                error = string.IsNullOrWhiteSpace(ex.Message) ? "EMPTY ERROR" : ex.Message;
            }
        }
        #endregion

        #region iosIKeyboardInputConnection Implementation
        bool IKeyboardInputConnection_ios.NeedsInputModeSwitchKey =>
            this.NeedsInputModeSwitchKey;

        public void OnInputModeSwitched() {
            //AdvanceToNextInputMode();
        }
        public void OnText(string text) {
            //TextDocumentProxy.InsertText(text);
        }

        public void OnBackspace() {
            //TextDocumentProxy.DeleteBackward();
        }

        public void OnDone() {
            //View.EndEditing(true);
        }

        public void OnNavigate(int deltaX, int deltaY) {
            //string all_text =
            //    TextDocumentProxy.DocumentContextBeforeInput +
            //    TextDocumentProxy.SelectedText +
            //    TextDocumentProxy.DocumentContextAfterInput;
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
            //InitAv();
            //InitAvAndKeyboard();
        }
        public override void ViewDidLoad() {
            base.ViewDidLoad();
            //InitAvAndKeyboard();
            if (this.HasFullAccess) {
                SetupKeyboardView();
            } else {
                SetupFallbackView();
            }
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
            }
            catch (Exception ex) {
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
        void AddKeyboard() {
            try {
                var ad2 = new AppDelegate();
                ad2.FinishedLaunching(UIApplication.SharedApplication, null);

                var outerStackView2 = new UIStackView() {
                    BackgroundColor = UIColor.Magenta,
                    Axis = UILayoutConstraintAxis.Vertical,
                    TranslatesAutoresizingMaskIntoConstraints = false,
                    Spacing = 5
                };
                View.AddSubview(outerStackView2);
                NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]{
                    outerStackView2.HeightAnchor.ConstraintGreaterThanOrEqualTo(300),
                    outerStackView2.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                    outerStackView2.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                    outerStackView2.LeftAnchor.ConstraintEqualTo(View.LeftAnchor),
                    outerStackView2.RightAnchor.ConstraintEqualTo(View.RightAnchor) });

                var kbs = KeyboardViewModel.GetTotalSizeByScreenSize(iosDisplayInfo.ScaledSize);
                var kb2 = KeyboardViewModel.CreateKeyboardView(this, kbs, iosDisplayInfo.Scaling, out var unscaledSize);
                var keyboardSize2 = new CGSize(unscaledSize.Width / iosDisplayInfo.Scaling, unscaledSize.Height / iosDisplayInfo.Scaling);

                var kbv2 = new AvaloniaView() {
                    Content = kb2,
                    BackgroundColor = UIColor.Orange,
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
                kbv2.InitWithController(this);

            }
            catch(Exception ex) {
                error = ex.ToString();
            }

        }
        void RemoveAllSubviews() {
            foreach (var sv in View.Subviews) {
                sv.RemoveFromSuperview();
            }
        }
        void AddFullAcccessLabel() {
            //if (outerStackView.Subviews.Contains(noFullAccessLabelView)) {
            //    return;
            //}
            noFullAccessLabelView = new UITextView();
            noFullAccessLabelView.Selectable = true;
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
            //string title = kbv == null ? "NO KB" : "YUUUUUUP";
            nextKeyboardButton.SetTitle("Next Keyboard", UIControlState.Normal);
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
                //if (!has_added_render_host) {
                //    AddRenderHost();
                //    return;
                //}
                //if (!has_initd_renderer) {
                //    InitRenderer();
                //    return;
                //}
                if (!is_kb_added) {
                    InitAv();
                    return;
                }
                //if (!has_started_timer) {
                //    SetupRenderHost();
                //    return;
                //}
                //if(renderTimer != null) {

                //    renderTimer.Fire();
                //}
                //AddKeyboard();
                //AddRenderHost();
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
                //AddKeyboard(true);
                //RenderKeyboard();
                //if (renderTimer == null) {

                //    SetupRenderHost();
                //} else {
                //    renderTimer.Fire();
                //}
                //if (KeyboardRenderer.GetKeyboardImageBytes(iosDisplayInfo.Scaling) is { } imgBytes) {
                //    noFullAccessLabelView.Text = Convert.ToBase64String(imgBytes);
                //} else {
                //    noFullAccessLabelView.Text = "No result";
                //}

            };
            innerStackView.AddArrangedSubview(replaceViewButton);
            //NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]{
            //    showErrorButton.LeftAnchor.ConstraintEqualTo(innerStackView.LeftAnchor),
            //    showErrorButton.TopAnchor.ConstraintEqualTo(innerStackView.TopAnchor),
            //    showErrorButton.BottomAnchor.ConstraintEqualTo(innerStackView.BottomAnchor),
            //    });
        }

        #endregion

        int GetMemoryMb(bool forceCollect = false) {
            try {
                int mb = (int)(GC.GetTotalMemory(false) / (1024 * 1024));
                return mb;
            } catch(Exception ex) {
                error = ex.ToString();
            }
            return 0;
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
        void ShowMemory() {
            
            error = $"MEMORY WARNING {GetMemoryMb()}mb";
        }
        public override void DidReceiveMemoryWarning() {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning();
            ShowMemory();
            if(kbv != null) {
                kbv.RemoveFromSuperview();
                kbv.Dispose();
                kbv = null;
            }
            GC.Collect();
            //RemoveAllSubviews();
            //SetupFallbackView();
            // Release any cached data, images, etc that aren't in use.
        }
        public override void UpdateViewConstraints() {
            base.UpdateViewConstraints();

            // Add custom view sizing constraints here
        }
        public KeyboardViewController(IntPtr handle) : base(handle) {
            // don't do stuff here
            //InitAvAndKeyboard();
        }

        public void OnFeedback() {

        }

        public event EventHandler OnCursorChanged;
        public event EventHandler OnFlagsChanged;
        public event EventHandler OnDismissed;

        public string GetLeadingText(int offset, int len) {
            return string.Empty;
        }

        public void OnBackspace(int count) {
            //throw new NotImplementedException();
        }

        public void OnFeedback(KeyboardFeedbackFlags flags) {
            //throw new NotImplementedException();
        }

        public KeyboardFlags Flags { 
            get {
                var kbf = KeyboardFlags.None;
                kbf |= KeyboardFlags.Portrait | KeyboardFlags.Mobile;
                return kbf;
            }
        }
    }

#pragma warning disable CA1010 // Generic interface should also be implemented
    public class ImgViewWrapper : UIView {
#pragma warning restore CA1010 // Generic interface should also be implemented
        public event EventHandler<CGPoint?> OnTouchEvent;
        public override void TouchesBegan(NSSet touches, UIEvent evt) {
            if (touches.FirstOrDefault() is not UITouch t) {
                return;
            }
            OnTouchEvent?.Invoke(this, t.LocationInView(this));
        }
        public override void TouchesMoved(NSSet touches, UIEvent evt) {
            if (touches.FirstOrDefault() is not UITouch t) {
                return;
            }
            OnTouchEvent?.Invoke(this, t.LocationInView(this));

        }
        public override void TouchesEnded(NSSet touches, UIEvent evt) {
            if (touches.FirstOrDefault() is not UITouch t) {
                return;
            }
            OnTouchEvent?.Invoke(this, null);
        }

    }
#pragma warning disable CA1010 // Generic interface should also be implemented
    public class FastImageView : UIImageView {
#pragma warning restore CA1010 // Generic interface should also be implemented
        void Test() {
            // Renderer
        }
    }
}