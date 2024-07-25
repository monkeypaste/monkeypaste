using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
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

        #region Appearance
        public static float Scaling { get; private set; }
        #endregion

        #region State
        bool HasFullAccess { get; set; }
        #endregion

        #region Views
        CustomView MenuView { get; set; }
        public KeyGridView KeyGridView { get; set; }
        CustomTextView CursorControlTextView { get; set; }
        CustomView CursorControlView { get; set; }
        Window PopupWindow { get; set; }
        #endregion


        #endregion

        #region Events
        #endregion

        #region Constructors
        public KeyboardView(Context context, IKeyboardInputConnection conn) : base(context) {
            SharedPaint = SetupPaint();
            Focusable = false;
            HasFullAccess = false;

            if (conn is IOnTouchListener otl) {
                this.SetOnTouchListener(otl);
            }

            Scaling = (float)AndroidDisplayInfo.Scaling;
            var kbs = KeyboardViewModel.GetTotalSizeByScreenSize(AndroidDisplayInfo.UnscaledSize, AndroidDisplayInfo.IsPortrait);
            DC = new KeyboardViewModel(conn, kbs / Scaling, Scaling, AndroidDisplayInfo.Scaling);
            DC.SetRenderer(this);


            MenuView = new CustomView(context, SharedPaint).SetDefaultProps("Menu");
            this.AddView(MenuView);

            KeyGridView = new KeyGridView(context, SharedPaint, DC).SetDefaultProps("KeyboardGrid");
            this.AddView(KeyGridView);

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

        #region Public Methods
        public void RemapRenderers() {
            // NOTE when flags change keys are recreated and renderer
            KeyGridView.AddOrResetKeys();
            Render(true);
        }

        public void TestPopup() {
            //if(DC.PressedKeys.FirstOrDefault() is { })

        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private GPaint SetupPaint() {
            var paint = new GPaint();
            paint.TextAlign = GPaint.Align.Left;
            //paint.ElegantTextHeight = true;
            paint.AntiAlias = true;
            paint.SetTypeface(Resources.GetFont(Resource.Font.RobotoMono_VariableFont_wght));
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

        public void Measure(bool invalidate) {
            this.Frame = DC.TotalRect.ToRectF();

            MenuView.Frame = DC.MenuRect.ToRectF();

            KeyGridView.Measure(invalidate);

            CursorControlView.Frame = DC.InnerRect.ToRectF();

            CursorControlTextView.TextSize = 24;
            var cct_size = CursorControlTextView.TextSize();
            float cct_l = CursorControlView.Frame.CenterX() - (cct_size.Width / 2);
            float cct_t = CursorControlView.Frame.CenterY() - (cct_size.Height / 2);
            float cct_r = cct_l + cct_size.Width;
            float cct_b = cct_t + cct_size.Height;
            CursorControlTextView.Frame = new RectF(cct_l, cct_t, cct_r, cct_b);
            
            if(invalidate) {
                MenuView.Redraw();
                CursorControlView.Redraw();
                CursorControlTextView.Redraw();
                KeyGridView.Redraw();
                this.Redraw();
            }
        }

        public void Paint(bool invalidate) {
            //this.SetBackgroundColor(Color.Orange);

            MenuView.SetBackgroundColor(KeyboardPalette.MenuBgHex.ToColor());

            KeyGridView.Paint(invalidate);

            CursorControlView.SetBackgroundColor(KeyboardPalette.CursorControlBgHex.ToColor());

            CursorControlTextView.SetTextColor(KeyboardPalette.CursorControlFgHex.ToColor());
            CursorControlTextView.ForegroundColor = KeyboardPalette.CursorControlFgHex.ToColor();

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