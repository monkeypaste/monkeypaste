using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GPaint = Android.Graphics.Paint;
using Rect = Android.Graphics.Rect;

namespace iosKeyboardTest.Android {
    public class KeyboardView : CustomViewGroup, IKeyboardViewRenderer {
        #region Private Variables
        int measureCount = 0;
        int layoutCount = 0;
        int paintCount = 0;
        int renderCount = 0;
        int drawCount = 0;
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
            if(MyInputMethodService.IS_MULTI_THREAD_MODE) {
                MyInputMethodService.PendingRenderers.Enqueue((DC, this));
            } else {
                DC.SetRenderer(this);
            }

            MenuView = new CustomView(context, SharedPaint).SetDefaultProps("Menu");
            this.AddView(MenuView);

            KeyGridView = new KeyGridView(context, SharedPaint, DC).SetDefaultProps("KeyboardGrid");
            this.AddView(KeyGridView);

            CursorControlView = new CustomView(context, SharedPaint).SetDefaultProps();
            this.AddView(CursorControlView);

            CursorControlTextView = new CustomTextView(context, SharedPaint) { TextAlignment = TextAlignment.Center }.SetDefaultProps() ;
            CursorControlTextView.Text = DC.CursorControlText;
            this.AddView(CursorControlTextView);

            HideCursorControl();

            Measure(true);
            DC.Renderer.Render(true);
        }
        #endregion

        #region Public Methods
        public void RemapRenderers() {
            // NOTE when flags change keys are recreated and renderers need to be re-assigned
            KeyGridView.AddOrResetKeys();
            Render(true);
        }
        public void ResetStats() {
            measureCount = 0;
            layoutCount = 0;
            paintCount = 0;
            renderCount = 0;
            drawCount = 0;

            foreach (var kv in KeyGridView.KeyViews) {
                kv.ResetStats();
            }

        }
        public void PrintStats() {
            System.Diagnostics.Debug.WriteLine(this.ToString());
            foreach(var kv in KeyGridView.KeyViews) {
                System.Diagnostics.Debug.WriteLine(kv.ToString());
            }
            ResetStats();
        }
        public override string ToString() {
            return $"'KEYBOARD' M:{measureCount} L:{layoutCount} P:{paintCount} R:{renderCount} D:{drawCount}";
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private GPaint SetupPaint() {
            var paint = new GPaint();
            paint.TextAlign = GPaint.Align.Left;
            paint.FakeBoldText = true;
            paint.ElegantTextHeight = true;
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
            MainThread.BeginInvokeOnMainThread(() => {
                if (DC.IsCursorControlEnabled) {
                    ShowCursorControl();
                } else {
                    HideCursorControl();
                }
                KeyGridView.Layout(invalidate);

                if (invalidate) {
                    this.Redraw();
                }
            });            
        }

        public void Measure(bool invalidate) {
            MainThread.BeginInvokeOnMainThread(() => {
                this.Frame = DC.TotalRect.ToRectF();

                MenuView.Frame = DC.MenuRect.ToRectF();

                KeyGridView.Measure(invalidate);

                CursorControlView.Frame = DC.CursorControlRect.ToRectF();

                CursorControlTextView.TextSize = DC.CursorControlFontSize.UnscaledF();
                var cct_size = CursorControlTextView.TextSize();
                float cct_l = CursorControlView.Frame.CenterX() - (cct_size.Width / 2);
                float cct_t = CursorControlView.Frame.CenterY() - (cct_size.Height / 2);
                float cct_r = cct_l + cct_size.Width;
                float cct_b = cct_t + cct_size.Height;
                CursorControlTextView.Frame = new RectF(cct_l, cct_t, cct_r, cct_b);
                //CursorControlTextView.Frame = DC.CursorControlTextRect.ToRectF();

                if (invalidate) {
                    MenuView.Redraw();
                    CursorControlView.Redraw();
                    CursorControlTextView.Redraw();
                    KeyGridView.Redraw();
                    this.Redraw();
                }
            });
            
        }

        public void Paint(bool invalidate) { 
            MainThread.BeginInvokeOnMainThread(() => {
                MenuView.SetBackgroundColor(KeyboardPalette.MenuBgHex.ToColor());

                KeyGridView.Paint(invalidate);

                CursorControlView.SetBackgroundColor(KeyboardPalette.CursorControlBgHex.ToColor());

                CursorControlTextView.SetTextColor(KeyboardPalette.CursorControlFgHex.ToColor());
                CursorControlTextView.ForegroundColor = KeyboardPalette.CursorControlFgHex.ToColor();

                if (invalidate) {
                    this.Redraw();
                }
            });            
        }

        public void Render(bool invalidate) {
            MainThread.BeginInvokeOnMainThread(() => {
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
            });
    

        }
    }
}