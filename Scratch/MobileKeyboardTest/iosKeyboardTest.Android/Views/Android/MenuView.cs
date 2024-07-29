using Android.Content;
using Android.Graphics;
using Android.Views;
using Microsoft.Maui.Graphics;
using static Android.Views.View;
using Color = Android.Graphics.Color;
using GPaint = Android.Graphics.Paint;

namespace iosKeyboardTest.Android {
    public class MenuView : CustomViewGroup, IOnClickListener {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces

        #region IOnClickListener Implementation

        public void OnClick(View v) {
            if(v == OptionsButtonView &&
                Context is MyInputMethodService mims) {
                mims.StartPrefActivity();
                return;
            }
            if(v == BackButtonView) {
                DC.GoBack();
                return;
            }
            // TODO find text under 
        }
        #endregion

        #region IKeyboardViewRenderer Implementation

        public override void Measure(bool invalidate) {
            Frame = DC.MenuRect.ToRectF();
            BackButtonView.Frame = DC.BackButtonRect.ToRectF();
            OptionsButtonView.Frame = DC.OptionsButtonRect.ToRectF();
            InnerMenuView.Frame = DC.InnerMenuRect.ToRectF();

            base.Measure(invalidate);
        }

        public override void Paint(bool invalidate) {
            this.SetBackgroundColor(KeyboardPalette.MenuBgHex.ToColor());
            BackButtonView.SetBackgroundColor(Color.Orange);
            InnerMenuView.SetBackgroundColor(Color.Purple);
            OptionsButtonView.SetBackgroundColor(Color.Red);

            base.Paint(invalidate);
        }
        #endregion

        #endregion

        #region Properties

        #region View Models
        public MenuViewModel DC { get; set; }
        #endregion

        #region Views
        CustomView BackButtonView { get; set; }
        CustomView InnerMenuView { get; set; }
        CustomView OptionsButtonView { get; set; }
        #endregion

        #region Appearance
        #endregion

        #region Layout
        #endregion

        #endregion

        #region Events
        #endregion

        #region Constructors


        public MenuView(Context context, GPaint paint, MenuViewModel dc) : base(context, paint) {
            this.Clickable = true;
            DC = dc;
            DC.SetRenderer(this);
            this.SetOnClickListener(this);

            BackButtonView = new CustomView(context, paint);
            this.AddView(BackButtonView);

            InnerMenuView = new CustomView(context, paint);
            this.AddView(InnerMenuView);

            OptionsButtonView = new CustomView(context, paint);
            this.AddView(OptionsButtonView);

            this.Render(true);
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