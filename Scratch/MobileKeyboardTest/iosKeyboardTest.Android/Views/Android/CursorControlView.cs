using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;

namespace iosKeyboardTest.Android {
    public class CursorControlView : CustomViewGroup, IKeyboardViewRenderer {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces

        #region IKeyboardViewRenderer Implementation

        public override void Layout(bool invalidate) {
            if (DC.IsCursorControlEnabled) {
                ShowCursorControl();
            } else {
                HideCursorControl();
            }

            base.Layout(invalidate);
        }

        public override void Measure(bool invalidate) {
            Frame = DC.CursorControlRect.ToRectF();

            CursorControlTextView.TextSize = DC.CursorControlFontSize.UnscaledF();
            var cct_size = CursorControlTextView.TextSize();
            float cct_l = Frame.CenterX() - (cct_size.Width / 2);
            float cct_t = Frame.CenterY() - (cct_size.Height / 2);
            float cct_r = cct_l + cct_size.Width;
            float cct_b = cct_t + cct_size.Height;
            CursorControlTextView.Frame = new RectF(cct_l, cct_t, cct_r, cct_b);

            base.Measure(invalidate);
        }

        public override void Paint(bool invalidate) {
            SetBackgroundColor(KeyboardPalette.CursorControlBgHex.ToColor());

            CursorControlTextView.SetTextColor(KeyboardPalette.CursorControlFgHex.ToColor());
            CursorControlTextView.ForegroundColor = KeyboardPalette.CursorControlFgHex.ToColor();

            base.Paint(invalidate);
        }

        public override void Render(bool invalidate) {
            Layout(false);
            Measure(false);
            Paint(false);

            base.Render(invalidate);
        }
        #endregion
        #endregion

        #region Properties

        #region View Models
        public KeyboardViewModel DC { get; set; }
        #endregion

        #region Views
        CustomTextView CursorControlTextView { get; set; }
        #endregion

        #region Appearance
        #endregion

        #region Layout
        #endregion

        #region State
        #endregion

        #region Models
        #endregion

        #endregion

        #region Events
        #endregion

        #region Constructors

        public CursorControlView(Context context, Paint paint, KeyboardViewModel dc) : base(context, paint) {
            DC = dc;

            CursorControlTextView = new CustomTextView(context, SharedPaint) { TextAlignment = TextAlignment.Center }.SetDefaultProps();
            CursorControlTextView.Text = DC.CursorControlText;
            this.AddView(CursorControlTextView);

            HideCursorControl();
        }
        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        void HideCursorControl() {
            this.Visibility = ViewStates.Invisible;
            CursorControlTextView.Visibility = ViewStates.Invisible;
        }
        void ShowCursorControl() {
            if (this == null) {
                return;
            }
            this.Visibility = ViewStates.Visible;
            CursorControlTextView.Visibility = ViewStates.Visible;
        }
        #endregion

        #region Commands
        #endregion

    }
}