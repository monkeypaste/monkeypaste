using Android.Content;
using Android.Graphics;
using Android.Views;
using Microsoft.Maui.Graphics;
using System;
using System.Linq;
using static Android.Views.View;
using Color = Android.Graphics.Color;
using GPaint = Android.Graphics.Paint;
using PointF = Android.Graphics.PointF;
using Rect = Android.Graphics.Rect;
using RectF = Android.Graphics.RectF;

namespace iosKeyboardTest.Android {
    public class MenuView : CustomViewGroup, IKeyboardViewRenderer {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces

        #region IKeyboardViewRenderer Implementation
        public override void Measure(bool invalidate) {
            Frame = DC.MenuRect.ToRectF();

            BackButtonRect = DC.BackButtonRect.ToRectF();
            BackButtonImageRect = DC.BackButtonImageRect.ToRectF();

            OptionsButtonRect = DC.OptionsButtonRect.ToRectF();
            OptionsButtonImageRect = DC.OptionButtonImageRect.ToRectF();

            InnerMenuRect = DC.InnerMenuRect.ToRectF();
            base.Measure(invalidate);
        }

        public override void Paint(bool invalidate) {
            BackgroundColor = DC.MenuBgHexColor.ToColor();
            
            if(BackButtonBmp == null && BackButtonImageRect.Width() > 0 && BackButtonImageRect.Height() > 0) {
                var back_bmp = Context.Resources.GetDrawable(Resource.Drawable.edgearrowleft).ToBitmap();
                BackButtonBmp = back_bmp.Scale((int)BackButtonImageRect.Width(), (int)BackButtonImageRect.Height(),false);

            }

            if(OptionsButtonBmp == null && OptionsButtonImageRect.Width() > 0 && OptionsButtonImageRect.Height() > 0) {
                var opt_bmp = Context.Resources.GetDrawable(Resource.Drawable.dots_1x3).ToBitmap();
                OptionsButtonBmp = opt_bmp.Scale((int)OptionsButtonImageRect.Width(), (int)OptionsButtonImageRect.Height(),false);

            }
            base.Paint(invalidate);
        }
        #endregion

        #endregion

        #region Properties

        #region View Models
        public MenuViewModel DC { get; set; }
        #endregion

        #region Views

        #region Back Button
        Bitmap BackButtonBmp { get; set; }
        RectF BackButtonRect { get; set; } = new();
        RectF BackButtonImageRect { get; set; } = new();
        Color BackButtonBgColor { get; set; }
        #endregion

        #region Options Button
        Bitmap OptionsButtonBmp { get; set; }
        RectF OptionsButtonRect { get; set; } = new();
        RectF OptionsButtonImageRect { get; set; } = new();
        Color OptionsButtonBgColor { get; set; }
        #endregion

        AutoCompleteView AutoCompleteView { get; set; }

        RectF InnerMenuRect { get; set; } = new();
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
            DC = dc;

            AutoCompleteView = new AutoCompleteView(context, paint, DC.AutoCompleteViewModel).SetDefaultProps("AutoComplete View");
            this.AddView(AutoCompleteView);

            ResetRenderer();
            this.SetPadding(0, 0, 0, 0);
            this.SetPaddingRelative(0, 0, 0, 0);

            this.Render(true);
        }


        #endregion

        #region Public Methods
        public void ResetRenderer() {
            DC.SetRenderer(this);
            AutoCompleteView.ResetRenderer();
        }
        #endregion

        #region Protected Methods
        protected override void OnDraw(Canvas canvas) {
            if (this.Visibility == ViewStates.Invisible) {
                return;
            }

            // draw bg
            SharedPaint.Color = DC.MenuBgHexColor.ToColor();
            canvas.DrawRect(Frame.ToBounds(), SharedPaint);

            SharedPaint.SetTint(DC.MenuFgHexColor.ToColor());
            if(BackButtonBmp != null && DC.IsBackButtonVisible) {
                canvas.DrawBitmap(BackButtonBmp, BackButtonImageRect.Left, BackButtonImageRect.Top, SharedPaint);
            }
            if(OptionsButtonBmp != null) {
                canvas.DrawBitmap(OptionsButtonBmp, OptionsButtonImageRect.Left, OptionsButtonImageRect.Top, SharedPaint);
            }
            
            
            SharedPaint.SetTint(null);
        }

        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}