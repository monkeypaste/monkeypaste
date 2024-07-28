using Android.Content;
using Android.Views;
using static Android.Views.View;
using GPaint = Android.Graphics.Paint;

namespace iosKeyboardTest.Android {
    public class MenuView : CustomViewGroup, IKeyboardViewRenderer, IOnClickListener {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces

        #region IOnClickListener Implementation

        public void OnClick(View v) {
            if(Context is MyInputMethodService mims) {
                mims.StartPrefActivity();
            }
        }
        #endregion

        #region IKeyboardViewRenderer Implementation

        public void Layout(bool invalidate) {
            
        }

        public void Measure(bool invalidate) {
            Frame = DC.MenuRect.ToRectF();

            if (invalidate) {
                this.Redraw();
            }
        }

        public void Paint(bool invalidate) {
            this.SetBackgroundColor(KeyboardPalette.MenuBgHex.ToColor());

            if (invalidate) {
                this.Redraw();
            }
        }

        public void Render(bool invalidate) {
            Layout(false);
            Measure(false);
            Paint(false);
            if(invalidate) {
                this.Redraw();
            }
        }
        #endregion

        #endregion

        #region Properties

        #region View Models
        public KeyboardViewModel DC { get; set; }
        #endregion

        #region Appearance
        #endregion

        #region Layout
        #endregion

        #endregion

        #region Events
        #endregion

        #region Constructors

        public MenuView(Context context) : base(context) {
        }

        public MenuView(Context context, GPaint paint, KeyboardViewModel dc) : base(context, paint) {
            this.Clickable = true;
            DC = dc;
            this.SetOnClickListener(this);
        }


        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}