using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Avalonia;
using Avalonia.Controls;
using iosKeyboardTest.Android;
using System;
using System.Collections.Generic;
using System.Linq;
using Rect = Android.Graphics.Rect;

namespace iosKeyboardTest.Android {
    public class KeyboardView :  CustomViewGroup, IKeyboardViewRenderer {
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
        CustomViewGroup MenuView { get; set; }
        CustomViewGroup KeyboardGridView { get; set; }
        CustomViewGroup FooterView { get; set; }
        TextView CursorControlTextViewView { get; set; }
        CustomViewGroup CursorControlView { get; set; }
        List<KeyView> KeyViews { get; set; } = [];

        #endregion


        #endregion

        #region Events
        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        public KeyboardView(IKeyboardInputConnection_ios conn, Context context) : base(context) {
            Init(conn);
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        private void Init(IKeyboardInputConnection_ios conn) {
            HasFullAccess = false;

            float s = 1;// UIScreen.MainScreen.Scale;
            var kbs = KeyboardViewModel.GetTotalSizeByScreenSize(AndroidDisplayInfo.ScaledSize, conn.Flags.HasFlag(KeyboardFlags.Portrait));
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

        }

        void AddMenu() {
            //MenuView = new ViewGroup(this.Context).SetDefaultProps();
            this.AddView(MenuView);
        }
        void AddKeyGrid() {
            //KeyboardGridView = new ViewGroup(this.Context).SetDefaultProps();
            this.AddView(KeyboardGridView);
            AddKeys();
        }

        void AddKeys() {
            foreach(var kvm in DC.Keys) {
                AddKey(kvm);
            }
        }
        void AddKey(KeyViewModel kvm) {
            var kv = new KeyView(kvm,this.Context).SetDefaultProps();
            KeyViews.Add(kv);
            KeyboardGridView.AddView(kv);
        }

        void AddFooter() {
            FooterView = new CustomViewGroup(this.Context).SetDefaultProps();
            //FooterView.BackgroundColor = UIColor.Purple;
            //this.AddView(FooterView);

            //NextKeyboardButton = new UIButton(UIButtonType.System);
            //NextKeyboardButton.SetTitle("🌐", UIControlState.Normal);
            //NextKeyboardButton.SizeToFit();
            //NextKeyboardButton.TranslatesAutoresizingMaskIntoConstraints = false;
            //NextKeyboardButton.AddTarget(this, new ObjCRuntime.Selector("advanceToNextInputMode"), UIControlEvent.TouchUpInside);
            //FooterView.AddView(NextKeyboardButton);

            //NSLayoutConstraint.ActivateConstraints([
            //    NextKeyboardButton.LeftAnchor.ConstraintEqualTo(FooterView.LeftAnchor),
            //    NextKeyboardButton.BottomAnchor.ConstraintEqualTo(FooterView.BottomAnchor)
            //    ]);
        }

        void AddCursorControl() {
            CursorControlView = new CustomViewGroup(this.Context).SetDefaultProps();
            this.AddView(KeyboardGridView);

            CursorControlTextViewView = new TextView(this.Context).SetDefaultProps();
            CursorControlTextViewView.Text = "👆Cursor Control";

            CursorControlView.AddView(CursorControlTextViewView);

            HideCursorControl();
        }
        #endregion

        void HideCursorControl() {
            if(CursorControlView == null) {
                return;
            }
            CursorControlView.Visibility = ViewStates.Visible;
            CursorControlView.Redraw();
        }
        void ShowCursorControl() {
            if(CursorControlView == null) {
                return;
            }
            CursorControlView.Visibility = ViewStates.Invisible;
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
            float w = (float)DC.TotalWidth;
            float h = (float)DC.TotalHeight;
            float x = 0;
            float y = 0;
            this.Frame = new RectF(x, y, w, h);

            float mx = 0;
            float my = 0;
            float mw = this.Frame.Width();
            float mh = (float)DC.MenuHeight;
            MenuView.Frame = new RectF(mx, my, mw, mh);

            float kgx = 0;
            float kgy = (float)DC.MenuHeight;
            float kgw = this.Frame.Width();
            float kgh = (float)DC.KeyboardHeight;
            KeyboardGridView.Frame = new RectF(kgx, kgy, kgw, kgh);

            foreach(var kv in KeyViews) {
                kv.Measure(invalidate);
            }

            float fx = 0;
            float fy = this.Frame.Bottom - (float)DC.FooterHeight;
            float fw = this.Frame.Width();
            float fh = (float)DC.FooterHeight;
            FooterView.Frame = new RectF(fx, fy, fw, fh);
            
            //CursorControlTextViewView.Font = UIFont.SystemFontOfSize(24);

            var cclvs = CursorControlTextViewView.TextSize();
            float cx = (this.Frame.Width() / 2) - (cclvs.Width / 2);
            float cy = (this.Frame.Height() / 2) - (cclvs.Height / 2);
            //CursorControlTextViewView.Frame = new RectF(cx, cy, cclvs.Width, cclvs.Height);

            if(invalidate) {
                MenuView.Redraw();
                KeyboardGridView.Redraw();
                FooterView.Redraw();
                CursorControlView.Redraw();
                this.Redraw();
            }
        }

        public void Paint(bool invalidate) {
            this.SetBackgroundColor(Color.Orange);
            MenuView.SetBackgroundColor(KeyboardPalette.MenuBgHex.ToColor());

            KeyboardGridView.SetBackgroundColor(KeyboardPalette.BgHex.ToColor());

            FooterView.SetBackgroundColor(Color.Purple);

            CursorControlView.SetBackgroundColor(KeyboardPalette.CursorControlBgHex.ToColor());

            CursorControlTextViewView.SetBackgroundColor(Color.Argb(0, 0, 0, 0));
            CursorControlTextViewView.SetTextColor(KeyboardPalette.CursorControlFgHex.ToColor());
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
        //public override void TouchesBegan(NSSet touches, UIEvent evt) {
        //    if (touches.FirstOrDefault() is not UITouch t) {
        //        return;
        //    }
        //    var p = t.LocationInView(this);
        //    OnTouchEvent?.Invoke(this, new TouchEventArgs(new Point(p.X,p.Y),TouchEventType.Press));
        //}
        //public override void TouchesMoved(NSSet touches, UIEvent evt) {
        //    if (touches.FirstOrDefault() is not UITouch t) {
        //        return;
        //    }
        //    var p = t.LocationInView(this);
        //    OnTouchEvent?.Invoke(this, new TouchEventArgs(new Point(p.X, p.Y),TouchEventType.Move));

        //}
        //public override void TouchesEnded(NSSet touches, UIEvent evt) {
        //    if (touches.FirstOrDefault() is not UITouch t) {
        //        return;
        //    }
        //    var p = t.LocationInView(this);
        //    OnTouchEvent?.Invoke(this, new TouchEventArgs(new Point(p.X, p.Y),TouchEventType.Release));
        //}

        protected override void OnLayout(bool changed, int l, int t, int r, int b) {
            throw new NotImplementedException();
        }
        #endregion
    }
}