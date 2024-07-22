using Avalonia;
using Avalonia.Controls;
using CoreGraphics;
using CoreImage;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using UIKit;

namespace iosKeyboardTest.iOS.KeyboardExt {
    public class KeyboardView : UIView, IKeyboardViewRenderer {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region View Models
        public KeyboardViewModel DC { get; set; }
        #endregion

        #region State
        bool HasFullAccess { get; set; }
        #endregion
        #region Views
        UIView MenuView { get; set; }
        UIView KeyboardGridView { get; set; }
        UIView FooterView { get; set; }
        UIButton NextKeyboardButton { get; set; }
        UITextView CursorControlTextViewView { get; set; }
        UIView CursorControlView { get; set; }
        List<KeyView> KeyViews { get; set; } = [];

        #endregion


        #endregion

        #region Events
        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        public KeyboardView(IKeyboardInputConnection_ios conn) {
            Init(conn);
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        private void Init(IKeyboardInputConnection_ios conn) {
            HasFullAccess = false;

            Subviews.ToList().ForEach(x => x.RemoveFromSuperview());
            double s = 1;// UIScreen.MainScreen.Scale;
            var kbs = KeyboardViewModel.GetTotalSizeByScreenSize(new Size(UIScreen.MainScreen.Bounds.Width / s, UIScreen.MainScreen.Bounds.Height / s), conn.Flags.HasFlag(KeyboardFlags.Portrait));
            DC = new KeyboardViewModel(conn,kbs,s);
            DC.SetRenderer(this);

            KeyboardPalette.SetTheme(conn.Flags.HasFlag(KeyboardFlags.Dark));

            SetBackground();
            AddMenu();
            AddKeyGrid();
            AddFooter();
            AddCursorControl();

            DC.Renderer.Render(true);
        }

        #region Add Views
        void SetBackground() {
            this.Layer.CornerRadius = 0;
            this.Layer.MasksToBounds = true;
            this.TranslatesAutoresizingMaskIntoConstraints = true;
        }

        void AddMenu() {
            MenuView = new UIView().SetDefaultProps();
            this.AddSubview(MenuView);
        }
        void AddKeyGrid() {
            KeyboardGridView = new UIView().SetDefaultProps();
            this.AddSubview(KeyboardGridView);
            AddKeys();
        }

        void AddKeys() {
            foreach(var kvm in DC.Keys) {
                AddKey(kvm);
            }
        }
        void AddKey(KeyViewModel kvm) {
            var kv = new KeyView(kvm).SetDefaultProps();
            KeyViews.Add(kv);
            KeyboardGridView.AddSubview(kv);
        }

        void AddFooter() {
            FooterView = new UIView().SetDefaultProps();
            FooterView.BackgroundColor = UIColor.Purple;
            this.AddSubview(FooterView);

            NextKeyboardButton = new UIButton(UIButtonType.System);
            NextKeyboardButton.SetTitle("🌐", UIControlState.Normal);
            NextKeyboardButton.SizeToFit();
            NextKeyboardButton.TranslatesAutoresizingMaskIntoConstraints = false;
            NextKeyboardButton.AddTarget(this, new ObjCRuntime.Selector("advanceToNextInputMode"), UIControlEvent.TouchUpInside);
            FooterView.AddSubview(NextKeyboardButton);

            NSLayoutConstraint.ActivateConstraints([
                NextKeyboardButton.LeftAnchor.ConstraintEqualTo(FooterView.LeftAnchor),
                NextKeyboardButton.BottomAnchor.ConstraintEqualTo(FooterView.BottomAnchor)
                ]);
        }

        void AddCursorControl() {
            CursorControlView = new UIView().SetDefaultProps();
            this.AddSubview(KeyboardGridView);

            CursorControlTextViewView = new UITextView().SetDefaultProps();
            CursorControlTextViewView.Text = "👆Cursor Control";

            CursorControlView.AddSubview(CursorControlTextViewView);

            HideCursorControl();
        }
        #endregion

        void HideCursorControl() {
            if(CursorControlView == null) {
                return;
            }
            CursorControlView.Hidden = true;
            CursorControlView.Redraw();
        }
        void ShowCursorControl() {
            if(CursorControlView == null) {
                return;
            }
            CursorControlView.Hidden = false;
            CursorControlView.Redraw();
        }
        #endregion

        public void Layout(bool invalidate) {
            if(DC.IsCursorControlEnabled) {
                ShowCursorControl();
            } else {
                HideCursorControl();
            }

        }

        public void Measure(bool invalidate) {
            double w = DC.TotalWidth;
            double h = DC.TotalHeight;
            double x = 0;
            double y = 0;
            this.Frame = new CGRect(x, y, w, h);

            double mx = 0;
            double my = 0;
            double mw = this.Frame.Width;
            double mh = DC.MenuHeight;
            MenuView.Frame = new CGRect(mx, my, mw, mh);

            double kgx = 0;
            double kgy = DC.MenuHeight;
            double kgw = this.Frame.Width;
            double kgh = DC.KeyboardHeight;
            KeyboardGridView.Frame = new CGRect(kgx, kgy, kgw, kgh);

            foreach(var kv in KeyViews) {
                kv.Measure(invalidate);
            }

            double fx = 0;
            double fy = this.Frame.Bottom - DC.FooterHeight;
            double fw = this.Frame.Width;
            double fh = DC.FooterHeight;
            FooterView.Frame = new CGRect(fx, fy, fw, fh);
            
            CursorControlTextViewView.Font = UIFont.SystemFontOfSize(24);
            CursorControlTextViewView.SizeToFit();
            var cclvs = CursorControlTextViewView.TextSize();
            double cx = (this.Bounds.Width / 2) - (cclvs.Width / 2);
            double cy = (this.Bounds.Height / 2) - (cclvs.Height / 2);
            CursorControlTextViewView.Frame = new CGRect(cx, cy, cclvs.Width, cclvs.Height);

            if(invalidate) {
                MenuView.Redraw();
                KeyboardGridView.Redraw();
                FooterView.Redraw();
                CursorControlView.Redraw();
                this.Redraw();
            }
        }

        public void Paint(bool invalidate) {
            this.BackgroundColor = UIColor.Orange;

            MenuView.BackgroundColor = KeyboardPalette.MenuBgHex.ToUIColor();

            KeyboardGridView.BackgroundColor = KeyboardPalette.BgHex.ToUIColor();

            FooterView.BackgroundColor = UIColor.Purple;

            CursorControlView.BackgroundColor = KeyboardPalette.CursorControlBgHex.ToUIColor();

            CursorControlTextViewView.BackgroundColor = UIColor.FromRGBA(0, 0, 0, 0);
            CursorControlTextViewView.TextColor = KeyboardPalette.CursorControlFgHex.ToUIColor();
        }

        public void Render(bool invalidate) {
            Layout(invalidate);
            Measure(invalidate);
            Paint(invalidate);
            foreach(var kv in KeyViews) {
                kv.Render(invalidate);
            }
            this.Redraw();
        }

        #region Input Handlers
        public event EventHandler<TouchEventArgs> OnTouchEvent;
        public override void TouchesBegan(NSSet touches, UIEvent evt) {
            if (touches.FirstOrDefault() is not UITouch t) {
                return;
            }
            var p = t.LocationInView(this);
            OnTouchEvent?.Invoke(this, new TouchEventArgs(new Point(p.X,p.Y),TouchEventType.Press));
        }
        public override void TouchesMoved(NSSet touches, UIEvent evt) {
            if (touches.FirstOrDefault() is not UITouch t) {
                return;
            }
            var p = t.LocationInView(this);
            OnTouchEvent?.Invoke(this, new TouchEventArgs(new Point(p.X, p.Y),TouchEventType.Move));

        }
        public override void TouchesEnded(NSSet touches, UIEvent evt) {
            if (touches.FirstOrDefault() is not UITouch t) {
                return;
            }
            var p = t.LocationInView(this);
            OnTouchEvent?.Invoke(this, new TouchEventArgs(new Point(p.X, p.Y),TouchEventType.Release));
        }
        #endregion
    }
}