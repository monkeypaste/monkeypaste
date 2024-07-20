using Avalonia;
using Avalonia.Controls;
using CoreGraphics;
using CoreImage;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using UIKit;

namespace iosKeyboardTest.iOS {
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
        KeyboardViewModel DC { get; set; }
        #endregion

        #region State
        bool HasFullAccess { get; set; }
        #endregion
        #region Views
        UIView MenuView { get; set; }
        UIView FooterView { get; set; }
        UIView KeyboardGridView { get; set; }
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
            double w = DC.TotalWidth;
            double h = DC.TotalHeight;
            double x = 0;
            double y = UIScreen.MainScreen.Bounds.Height - h;
            this.Frame = new CGRect(x, y, w, h);
            this.BackgroundColor = UIColor.Orange;
            this.Layer.CornerRadius = 0;
            this.Layer.MasksToBounds = true;
            this.TranslatesAutoresizingMaskIntoConstraints = false;
        }

        void AddMenu() {
            double x = 0;
            double y = 0;
            double w = this.Frame.Width;
            double h = DC.MenuHeight;
            MenuView = new UIView(new CGRect(x, y, w, h)) { TranslatesAutoresizingMaskIntoConstraints = false };
            MenuView.BackgroundColor = MenuBgBrush;
            this.AddSubview(MenuView);
        }
        void AddKeyGrid() {
            double x = 0;
            double y = DC.MenuHeight;
            double w = this.Frame.Width;
            double h = DC.KeyboardHeight;
            KeyboardGridView = new UIView(new CGRect(x, y, w, h)) { TranslatesAutoresizingMaskIntoConstraints = false };
            KeyboardGridView.BackgroundColor = BgBrush;
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
        #endregion
        #endregion

        public void Layout(bool invalidate) {
            throw new System.NotImplementedException();
        }

        public void Measure(bool invalidate) {
            throw new System.NotImplementedException();
        }

        public void Paint(bool invalidate) {
            throw new System.NotImplementedException();
        }

        public void Render(bool invalidate) {
            foreach(var kv in KeyViews) {
                kv.Render(invalidate);
            }
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