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

        #region Pallette
        public static UIColor FgBrush { get; set; }
        public static UIColor FgBrush2 { get; set; }
        public static UIColor BgBrush { get; set; }
        public static UIColor HoldBgBrush { get; set; }
        public static UIColor HoldFocusBgBrush { get; set; }
        public static UIColor HoldFgBrush { get; set; }
        public static UIColor PressedBgBrush { get; set; }
        public static UIColor SpecialPressedBgBrush { get; set; }
        public static UIColor PrimarySpecialPressedBgBrush { get; set; }
        public static UIColor ShiftBrush { get; set; }
        public static UIColor MenuBgBrush { get; set; }
        public static UIColor CursorControlBgBrush { get; set; }
        public static UIColor CursorControlFgBrush { get; set; }
        public static UIColor DefaultBgBrush { get; set; }
        public static UIColor SpecialBgBrush { get; set; }
        public static UIColor PrimarySpecialBgBrush { get; set; }
        #endregion
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
        UITextView CursorControlLabelView { get; set; }
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
            var kbs = KeyboardViewModel.GetTotalSizeByScreenSize(new Avalonia.Size(UIScreen.MainScreen.Bounds.Width / s, UIScreen.MainScreen.Bounds.Height / s));
            DC = new KeyboardViewModel(conn,kbs,s);
            DC.SetRenderer(this);

            InitPalette(false);

            SetBackground();
            AddMenu();
            AddKeyGrid();
            AddFooter();
            AddCursorControl();

            DC.Renderer.Render(true);
        }


        void InitPalette(bool isDark) {
            nfloat bg_alpha = 0.7f;
            if (isDark) {
                BgBrush = UIColor.SystemGray6.ColorWithAlpha(bg_alpha);
                FgBrush = UIColor.White;
                FgBrush2 = UIColor.SystemGray5;
                HoldBgBrush = UIColor.Yellow;
                HoldFocusBgBrush = UIColor.Orange;
                HoldFgBrush = UIColor.Black;
                PressedBgBrush = UIColor.Gray;
                SpecialPressedBgBrush = UIColor.DarkGray;
                PrimarySpecialPressedBgBrush = UIColor.Blue;
                ShiftBrush = UIColor.Cyan;
                MenuBgBrush = UIColor.FromRGB(51, 51, 51);
                CursorControlBgBrush = UIColor.FromRGBA(20, 20, 20, 150);
                CursorControlFgBrush = UIColor.White;

                DefaultBgBrush = UIColor.SystemGray3;
                SpecialBgBrush = UIColor.SystemGray5;
                PrimarySpecialBgBrush = UIColor.SystemIndigo;
                return;
            }
            BgBrush = UIColor.White.ColorWithAlpha(bg_alpha);
            FgBrush = UIColor.Black;
            FgBrush2 = UIColor.DarkGray;
            HoldBgBrush = UIColor.Yellow;
            HoldFocusBgBrush = UIColor.Orange;
            HoldFgBrush = UIColor.Black;
            PressedBgBrush = UIColor.Gray;
            SpecialPressedBgBrush = UIColor.Brown;
            PrimarySpecialPressedBgBrush = UIColor.Blue;
            ShiftBrush = UIColor.Green;
            MenuBgBrush = UIColor.SystemGray;
            CursorControlBgBrush = UIColor.FromRGBA(255, 255, 255,150);
            CursorControlFgBrush = UIColor.Black;

            DefaultBgBrush = UIColor.SystemGray3;
            SpecialBgBrush = UIColor.SystemMint;
            PrimarySpecialBgBrush = UIColor.SystemIndigo;
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
            FooterView.UserInteractionEnabled = true;
            this.Add(FooterView);
            NSLayoutConstraint.ActivateConstraints([
                FooterView.HeightAnchor.ConstraintEqualTo(50),// (nfloat)DC.FooterHeight),
                FooterView.LeftAnchor.ConstraintEqualTo(this.LeftAnchor),
                FooterView.BottomAnchor.ConstraintEqualTo(this.BottomAnchor)
                ]);

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
            CursorControlView = new UIView(this.Frame).SetDefaultProps();
            this.AddSubview(KeyboardGridView);

            CursorControlLabelView = new UITextView().SetDefaultProps();
            CursorControlLabelView.Text = "👆Cursor Control";

            CursorControlView.AddSubview(CursorControlLabelView);

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

            //double fx = 0;
            //double fy = this.Bounds.Bottom - DC.FooterHeight;
            //double fw = this.Frame.Width;
            //double fh = DC.FooterHeight;
            //FooterView.Frame = new CGRect(fx, fy, fw, fh);

            CursorControlLabelView.Font = UIFont.SystemFontOfSize(24);
            CursorControlLabelView.SizeToFit();
            var cclvs = CursorControlLabelView.TextSize();
            double cx = (this.Bounds.Width / 2) - (cclvs.Width / 2);
            double cy = (this.Bounds.Height / 2) - (cclvs.Height / 2);
            CursorControlLabelView.Frame = new CGRect(cx, cy, cclvs.Width, cclvs.Height);

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

            MenuView.BackgroundColor = MenuBgBrush;

            KeyboardGridView.BackgroundColor = BgBrush;

            FooterView.BackgroundColor = UIColor.Purple;

            CursorControlView.BackgroundColor = CursorControlBgBrush;

            CursorControlLabelView.BackgroundColor = UIColor.FromRGBA(0, 0, 0, 0);
            CursorControlLabelView.TextColor = CursorControlFgBrush;
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