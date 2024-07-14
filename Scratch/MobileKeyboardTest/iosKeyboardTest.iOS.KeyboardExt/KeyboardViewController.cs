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

        UIImageView imgView = null;
        ImgViewWrapper imgWrapper = null;

        NSTimer renderTimer = null;

        Task avTask = null;
        SynchronizationContext avCtx = null;

        CGSize keyboardSize = new CGSize(1000, 300);

        string _error;
        string error {
            get => string.IsNullOrEmpty(_error) ? "No error" : _error;
            set {
                var val = value ?? string.Empty;
                _error = val + Environment.NewLine + _error;
                if (this.HasFullAccess) {
                    //KeyboardRenderer.SetError(val);
                    return;
                }
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

        void SetupRenderHost() {
            //renderTimer = NSTimer.CreateRepeatingScheduledTimer(
            //        //TimeSpan.FromMilliseconds(1000d / fps), delegate {
            //        TimeSpan.FromSeconds(5), delegate {
            //            RenderKeyboard();
            //        });
            imgWrapper.OnTouchEvent += (s, e) => {
                Point? p = e == null ? null : new Point(e.Value.X, e.Value.Y);
                OnPointerChanged?.Invoke(this, p);
                RenderKeyboard();
            };
            has_started_timer = true;
        }
        bool has_added_render_host = false;
        bool has_initd_renderer = false;
        bool has_started_timer = false;

        double fps = 120d;
        KeyboardView kb;
        AvaloniaView kbv { get; set; }
        AppDelegate ad;
        Avalonia.Controls.Image avimg = null;
        byte[] kbImgBytes = default;

        bool is_av_initd = false;
        bool is_kb_created = false;
        bool is_kb_added = false;
        void InitAv() {
            try {
                if (!is_av_initd) {
                    ad = new AppDelegate();
                    ad.FinishedLaunching(UIApplication.SharedApplication, null);
                    is_av_initd = true;
                    return;
                }
                if(!is_kb_created) {
                    //kb = KeyboardViewModel.CreateKeyboardView(this, iosDisplayInfo.ScaledSize, iosDisplayInfo.Scaling, out var unscaledSize);
                    //keyboardSize = new CGSize(unscaledSize.Width / iosDisplayInfo.Scaling, unscaledSize.Height / iosDisplayInfo.Scaling);
                    avimg = new Avalonia.Controls.Image();
                    kbv = new AvaloniaView() { 
                        //Content = new Avalonia.Controls.Image
                        BackgroundColor = UIColor.Orange,
                        Content = new Avalonia.Controls.Border {
                            //Child = avimg, 
                            Background = Brushes.Pink,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
                        },
                        TranslatesAutoresizingMaskIntoConstraints = false
                    };
                    kbv.InitWithController(this);
                    if(kbv.Content is Control c) {
                        c.Loaded += (s, e) => {
                            error = "Loaded";
                        };
                        c.EffectiveViewportChanged += (s, e) => {
                            error = $"Viewport: {c.Bounds}";
                        };
                    }
                    is_kb_created = true;
                    return;
                }
                if(!is_kb_added) {
                    imgWrapper = new ImgViewWrapper() {
                        BackgroundColor = UIColor.Blue,
                        TranslatesAutoresizingMaskIntoConstraints = false
                    };

                    outerStackView.InsertArrangedSubview(imgWrapper, 0);
                    NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]{
                    imgWrapper.WidthAnchor.ConstraintGreaterThanOrEqualTo(keyboardSize.Width),

                    imgWrapper.HeightAnchor.ConstraintGreaterThanOrEqualTo(keyboardSize.Height),
                    });

                    imgWrapper.AddSubview(kbv);
                    NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]{
                    //kbv.WidthAnchor.ConstraintGreaterThanOrEqualTo(keyboardSize.Width),

                    //kbv.HeightAnchor.ConstraintGreaterThanOrEqualTo(keyboardSize.Height),
                    
                    kbv.WidthAnchor.ConstraintEqualTo(imgWrapper.WidthAnchor),
                    kbv.HeightAnchor.ConstraintEqualTo(imgWrapper.HeightAnchor),
                    });

                    is_kb_added = true;
                }
                
                
            }
            catch (Exception ex) {
                error = string.IsNullOrWhiteSpace(ex.Message) ? "EMPTY ERROR" : ex.Message;
            }
        }
        void InitRenderer() {
            if (!this.HasFullAccess) {
                return;

            }
            //avTask = Task.Run(async() => {

                KeyboardRenderer.Init(
                    this,
                    KeyboardViewModel.GetTotalSizeByScreenSize(iosDisplayInfo.ScaledSize),
                    iosDisplayInfo.Scaling,
                    out var unscaledSize);
            imgView.Image = new UIImage(NSData.FromArray(KeyboardRenderer.GetKeyboardImageBytes(0.1)));

            //    while(true) {
            //        await Task.Delay(TimeSpan.FromMilliseconds( 1000d / fps));
            //        var kbBytes = KeyboardRenderer.GetKeyboardImageBytes(0.1, 100);
            //        DispatchQueue.MainQueue.DispatchAsync(() => {
            //            kbImgBytes = kbBytes;
            //        });

            //    }
            //});
            has_initd_renderer = true;
        }

        void AddRenderHost() {
            try {
                Size unscaledSize = new Size(1000, 300);

                keyboardSize = new CGSize(
                    unscaledSize.Width / iosDisplayInfo.Scaling,
                    unscaledSize.Height / iosDisplayInfo.Scaling);

                imgWrapper = new ImgViewWrapper() {
                    BackgroundColor = UIColor.Blue,
                    TranslatesAutoresizingMaskIntoConstraints = false
                };

                outerStackView.InsertArrangedSubview(imgWrapper, 0);
                NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]{
                    imgWrapper.WidthAnchor.ConstraintGreaterThanOrEqualTo(keyboardSize.Width),

                    imgWrapper.HeightAnchor.ConstraintGreaterThanOrEqualTo(keyboardSize.Height),
                    });

                imgView = new UIImageView();
                imgView.TranslatesAutoresizingMaskIntoConstraints = false;
                imgWrapper.AddSubview(imgView);
                NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]{
                    imgView.WidthAnchor.ConstraintEqualTo(imgWrapper.WidthAnchor),
                    imgView.HeightAnchor.ConstraintEqualTo(imgWrapper.HeightAnchor),
                    });
                has_added_render_host = true;
            }
            catch (Exception ex) {
                error = ex.ToString();
            }
        }
        int rcount = 0;
        void RenderKeyboard() {
            if(avimg != null ) {
                if(this.HasFullAccess && 
                    KeyboardRenderer.GetKeyboardImageBytes(0.1/*iosDisplayInfo.Scaling*/) is { } avbytes) {
                    avimg.Source = RenderHelpers.RenderToBitmap(avbytes);
                    rcount++;
                    replaceViewButton.SetTitle($"Rendered: {rcount}", UIControlState.Normal);
                }
                return;
            }
            if (imgView == null ||
                !this.HasFullAccess) {
                return;
            }
            var imgBytes = KeyboardRenderer.GetKeyboardImageBytes(0.1/*iosDisplayInfo.Scaling*/) ?? Convert.FromBase64String("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");

            rcount++;
            replaceViewButton.SetTitle($"Rendered: {rcount}", UIControlState.Normal);
            
            imgView.Image = new UIImage(NSData.FromArray(imgBytes));
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
                //AddRenderHost();
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
                if (!has_added_render_host) {
                    AddRenderHost();
                    return;
                }
                if (!has_initd_renderer) {
                    InitRenderer();
                    return;
                }
                //if(!is_kb_added) {
                //    InitAv();
                //    return;
                //}
                if (!has_started_timer) {
                    SetupRenderHost();
                    return;
                }
                if(renderTimer != null) {

                    renderTimer.Fire();
                }
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
                if (renderTimer == null) {

                    SetupRenderHost();
                } else {
                    renderTimer.Fire();
                }
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

        public void OnVibrateRequest() {

        }

        event EventHandler<TouchEventArgs> IHeadlessRender.OnPointerChanged {
            add {
                throw new NotImplementedException();
            }

            remove {
                throw new NotImplementedException();
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