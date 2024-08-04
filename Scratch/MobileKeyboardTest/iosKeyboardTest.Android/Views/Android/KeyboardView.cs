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

namespace iosKeyboardTest.Android {
    public class KeyboardView : CustomViewGroup, 
        IKeyboardViewRenderer,
        ITextMeasurer {
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


        #region ITextMeasurer Implementation
        Avalonia.Rect ITextMeasurer.MeasureText(string text, double scaledFontSize, Avalonia.Media.TextAlignment alignment, out double ascent, out double descent) {
            var tb = new Rect();
            text = text ?? string.Empty;
            SharedPaint.TextAlign = alignment.ToAdAlign();
            SharedPaint.TextSize = scaledFontSize.UnscaledF();
            SharedPaint.GetTextBounds(text, 0, text.Length, tb);
            ascent = SharedPaint.Ascent().ScaledD();
            descent = SharedPaint.Descent().ScaledD();
            return tb.ToAvRect();

        }
        #endregion

        #region IKeyboardRenderer Implementation

        public override void Measure(bool invalidate) {
            this.Frame = DC.TotalRect.ToRectF();
            base.Measure(invalidate);
        }
        #endregion

        #endregion

        #region Properties

        #region View Models
        public KeyboardViewModel DC { get; set; }
        #endregion

        #region Appearance
        public static float Scaling { get; private set; }
        #endregion

        #region State
        #endregion

        #region Views
        MenuView MenuView { get; set; }
        public KeyGridView KeyGridView { get; set; }
        CursorControlView CursorControlView { get; set; }
        Window PopupWindow { get; set; }
        #endregion


        #endregion

        #region Events
        #endregion

        #region Constructors
        public KeyboardView(Context context) : base(context) {            
            SharedPaint = SetupPaint();
            Scaling = (float)AndroidDisplayInfo.Scaling;
        }
        public void Init(IKeyboardInputConnection conn) {
            if (conn is IOnTouchListener otl) {
                this.SetOnTouchListener(otl);
                //KeyGridView.KeyViews.ForEach(x => x.SetOnTouchListener(otl));
            }
            var kbs = KeyboardViewModel.GetTotalSizeByScreenSize(AndroidDisplayInfo.UnscaledSize, AndroidDisplayInfo.IsPortrait);
            DC = new KeyboardViewModel(conn, kbs / Scaling, Scaling, AndroidDisplayInfo.Scaling);
            DC.SetRenderer(this);

            MenuView = new MenuView(Context, SharedPaint, DC.MenuViewModel).SetDefaultProps("Menu");
            this.AddView(MenuView);

            KeyGridView = new KeyGridView(Context, SharedPaint, DC).SetDefaultProps("KeyboardGrid");
            this.AddView(KeyGridView);

            CursorControlView = new CursorControlView(Context, SharedPaint, DC).SetDefaultProps();
            this.AddView(CursorControlView);

            Measure(true);
            DC.Renderer.Render(true);
        }
        #endregion

        #region Public Methods
        public void RemapRenderers() {
            // NOTE when flags change keys are recreated and renderers need to be re-assigned
            KeyGridView.AddOrResetKeys();
            MenuView.ResetRenderer();
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
            //paint.FakeBoldText = true;
            //paint.ElegantTextHeight = true;
            //paint.AntiAlias = true;
            paint.SetTypeface(Resources.GetFont(Resource.Font.Nunito_Regular));
            return paint;
        }
        #endregion


    }
}