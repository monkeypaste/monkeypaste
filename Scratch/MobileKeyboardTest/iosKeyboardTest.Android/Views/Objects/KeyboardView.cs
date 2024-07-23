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
using GPaint = Android.Graphics.Paint;
using Rect = Android.Graphics.Rect;

namespace iosKeyboardTest.Android {
    public class KeyboardView : CustomViewGroup, IKeyboardViewRenderer {
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
        Paint SharedPaint { get; set; }
        bool HasFullAccess { get; set; }
        #endregion
        #region Views
        CustomView MenuView { get; set; }
        KeyGridView KeyGridView { get; set; }
        CustomTextView CursorControlTextView { get; set; }
        CustomView CursorControlView { get; set; }
        #endregion


        #endregion

        #region Events
        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        public KeyboardView(IKeyboardInputConnection conn, Context context) : base(context,null) {
            SharedPaint = SetupPaint();

            Focusable = false;
            HasFullAccess = false;

            if (conn is IOnTouchListener otl) {
                this.SetOnTouchListener(otl);
            }

            float s = 1;// (float)AndroidDisplayInfo.Scaling;
            var kbs = KeyboardViewModel.GetTotalSizeByScreenSize(AndroidDisplayInfo.UnscaledSize, AndroidDisplayInfo.IsPortrait);
            DC = new KeyboardViewModel(conn, kbs, s);
            DC.SetRenderer(this);


            int w = (int)(DC.TotalWidth * s);
            int h = (int)(DC.TotalHeight * s);
            this.LayoutParameters = new FrameLayout.LayoutParams(w, h);

            KeyboardPalette.SetTheme(conn.Flags.HasFlag(KeyboardFlags.Dark));
            SharedPaint = SetupPaint();

            MenuView = new CustomView(context, SharedPaint).SetDefaultProps("Menu");
            this.AddView(MenuView);

            KeyGridView = new KeyGridView(context, SharedPaint, DC).SetDefaultProps("KeyboardGrid");
            this.AddView(KeyGridView);
            KeyGridView.SetZ(10);

            CursorControlView = new CustomView(context, SharedPaint).SetDefaultProps();
            this.AddView(CursorControlView);

            CursorControlTextView = new CustomTextView(context, SharedPaint).SetDefaultProps();
            CursorControlTextView.Text = "👆Cursor Control";

            this.AddView(CursorControlTextView);

            HideCursorControl();

            Measure(true);
            DC.Renderer.Render(true);
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private Paint SetupPaint() {
            var paint = new GPaint();
            paint.TextAlign = GPaint.Align.Left;
            paint.AntiAlias = true;
            paint.SetTypeface(Resources.GetFont(Resource.Font.Nunito_Regular));
            return paint;
        }
        void HideCursorControl() {
            if(CursorControlView == null) {
                return;
            }
            CursorControlView.Visibility = ViewStates.Invisible;
            CursorControlTextView.Visibility = ViewStates.Invisible;
            CursorControlView.Redraw();
        }
        void ShowCursorControl() {
            if(CursorControlView == null) {
                return;
            }
            CursorControlView.Visibility = ViewStates.Visible;
            CursorControlTextView.Visibility = ViewStates.Visible;
            CursorControlView.Redraw();
        }
        #endregion

        public void Layout(bool invalidate) {
            if(DC.IsCursorControlEnabled) {
                ShowCursorControl();
            } else {
                HideCursorControl();
            }
            KeyGridView.Layout(invalidate);

            if(invalidate) {
                this.Redraw();
            }
        }
        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec) {
            //base.OnMeasure(MeasureSpec.MakeMeasureSpec((int)Frame.Width(), MeasureSpecMode.Exactly), MeasureSpec.MakeMeasureSpec((int)Frame.Height(), MeasureSpecMode.Exactly));
            SetMeasuredDimension((int)DC.TotalWidth, (int)DC.TotalHeight);
        }

        public void Measure(bool invalidate) {

            float r = (float)DC.TotalWidth;
            float b = (float)DC.TotalHeight;
            float l = 0;
            float t = 0;
            this.Frame = new RectF(l, t, r, b);

            float ml = l;
            float mt = t;
            float mr = r;
            float mb = (float)DC.MenuHeight;
            MenuView.Frame = new RectF(ml, mt, mr, mb);

            KeyGridView.Measure(invalidate);

            CursorControlView.Frame = new RectF(l, t, r, b);

            CursorControlTextView.TextSize = 24;
            var cclvs = CursorControlTextView.TextSize();
            float cx = ((r - l) / 2) - (cclvs.Width / 2);
            float cy = ((b - t) / 2) - (cclvs.Height / 2);
            CursorControlTextView.Frame = new RectF(cx, cy, cclvs.Width, cclvs.Height);

            if(invalidate) {
                MenuView.Redraw();
                CursorControlView.Redraw();
                KeyGridView.Redraw();
                this.Redraw();
            }
        }

        public void Paint(bool invalidate) {
            this.SetBackgroundColor(Color.Orange);
            MenuView.SetBackgroundColor(KeyboardPalette.MenuBgHex.ToColor());
            //MenuView.CustomBgColor = KeyboardPalette.MenuBgHex.ToColor();

            KeyGridView.Paint(invalidate);

            CursorControlView.SetBackgroundColor(KeyboardPalette.CursorControlBgHex.ToColor());
            //CursorControlView.CustomBgColor = KeyboardPalette.CursorControlBgHex.ToColor();

            //CursorControlTextView.SetBackgroundColor(Color.Argb(0, 0, 0, 0));
            CursorControlTextView.SetTextColor(KeyboardPalette.CursorControlFgHex.ToColor());
            CursorControlTextView.CustomTextColor = KeyboardPalette.CursorControlFgHex.ToColor();

            if (invalidate) {
                this.Redraw();
            }
        }

        public void Render(bool invalidate) {
            Layout(false);
            Measure(false);
            Paint(false);
            KeyGridView.Render(invalidate);
            if (invalidate) {
                this.Redraw();
                for (int i = 0; i < ChildCount; i++) {
                    this.GetChildAt(i).Redraw();
                }
            }

        }
    }
}