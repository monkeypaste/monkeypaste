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
    public class MenuView : CustomView, IKeyboardViewRenderer {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces

        #region IKeyboardViewRenderer Implementation
        public override void Layout(bool invalidate) {
            CompletionTexts = DC.CompletionDisplayValues.ToArray();
            base.Layout(invalidate);
        }
        public override void Measure(bool invalidate) {
            Frame = DC.MenuRect.ToRectF();

            BackButtonRect = DC.BackButtonRect.ToRectF();
            BackButtonImageRect = DC.BackButtonImageRect.ToRectF();

            OptionsButtonRect = DC.OptionsButtonRect.ToRectF();
            OptionsButtonImageRect = DC.OptionButtonImageRect.ToRectF();

            InnerMenuRect = DC.InnerMenuRect.ToRectF();

            CompletionRects = DC.CompletionItemRects.Select(x => x.ToRectF()).ToArray();
            //CompletionTextLocs = DC.CompletionItemTextLocs.Select(x => x.ToPointF()).ToArray();
            CompletionTextLocs = new PointF[CompletionTexts.Length];
            int avail_count = Math.Min(CompletionRects.Length, CompletionTexts.Length);
            for (int i = 0; i < avail_count; i++) {
                var comp_item_text = CompletionTexts[i];
                var comp_item_rect = CompletionRects[i];
                var citb = new Rect();
                SharedPaint.TextAlign = GPaint.Align.Center;
                SharedPaint.TextSize = DC.CompletionItemFontSize.UnscaledF();
                SharedPaint.GetTextBounds(comp_item_text, 0, comp_item_text.Length, citb);
                float cix = comp_item_rect.CenterX();
                float ciy = comp_item_rect.CenterY() - ((SharedPaint.Ascent() + SharedPaint.Descent()) / 2);
                CompletionTextLocs[i] = new PointF(cix, ciy);
            }
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
            CompletionBgColors = DC.CompletionItemBgHexColors.Select(x => x.ToColor()).ToArray();
            
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

        #region Completions
        RectF[] CompletionRects { get; set; } = [];
        string[] CompletionTexts { get; set; } = [];
        Color[] CompletionBgColors { get; set; } = [];
        PointF[] CompletionTextLocs { get; set; } = [];
        #endregion
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
            //this.Clickable = true;
            DC = dc;
            ResetRenderer();
            this.SetPadding(0, 0, 0, 0);
            this.SetPaddingRelative(0, 0, 0, 0);

            this.Render(true);
        }


        #endregion

        #region Public Methods
        public void ResetRenderer() {
            DC.SetRenderer(this);
        }
        #endregion

        #region Protected Methods
        protected override void OnDraw(Canvas canvas) {
            if (this.Visibility == ViewStates.Invisible) {
                return;
            }
            //var test = Frame.Width() - canvas.Width;
            //var test2 = Frame.Height() - canvas.Height;
            // draw bg
            SharedPaint.Color = DC.MenuBgHexColor.ToColor();

            //var test3 = Frame.Width() - canvas.Cl;
            //var test4 = Frame.Height() - canvas.Height;
            canvas.DrawRect(Frame.ToBounds(), SharedPaint);
            
            if (DC.MenuPageType == MenuPageType.Completions) {
                // draw completions

                // clip completions to inner frame
                canvas.Save();
                canvas.ClipRect(InnerMenuRect.Left, InnerMenuRect.Top, InnerMenuRect.Right, InnerMenuRect.Bottom);

                int avail_count = GetAvailableComplCount();
                for (int i = 0; i < avail_count; i++) {
                    var comp_item_rect = CompletionRects[i];
                    if(comp_item_rect.Right < InnerMenuRect.Left ||
                        comp_item_rect.Left > InnerMenuRect.Right) {
                        // clipped
                        continue;
                    }

                    var comp_item_text = CompletionTexts[i];
                    var comp_item_bg = CompletionBgColors[i];
                    var comp_item_loc = CompletionTextLocs[i];
                    // draw item bg
                    SharedPaint.SetStyle(GPaint.Style.Fill);
                    SharedPaint.Color = comp_item_bg;
                    canvas.DrawRect(comp_item_rect, SharedPaint);

                    // draw item outline
                    SharedPaint.SetStyle(GPaint.Style.Stroke);
                    SharedPaint.Color = DC.MenuFgHexColor.ToColor();
                    canvas.DrawRect(comp_item_rect, SharedPaint);

                    // draw item text
                    SharedPaint.SetStyle(GPaint.Style.Fill);
                    SharedPaint.TextAlign = DC.CompletionTextAlignment.ToAdAlign();
                    SharedPaint.TextSize = DC.CompletionItemFontSize.UnscaledF();
                    SharedPaint.Color = DC.MenuFgHexColor.ToColor();                    
                    canvas.DrawText(comp_item_text, comp_item_loc.X, comp_item_loc.Y, SharedPaint);
                }
                canvas.Restore();
            }

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
        int GetAvailableComplCount() {
            int avail_count = Math.Min(CompletionTexts.Length, Math.Min(CompletionRects.Length, Math.Min(CompletionBgColors.Length, CompletionTextLocs.Length)));
            int max_count = Math.Max(CompletionTexts.Length, Math.Max(CompletionRects.Length, Math.Max(CompletionBgColors.Length, CompletionTextLocs.Length)));
            if (avail_count != max_count) {
                // mismatch!
            }
            return avail_count;
        }
        #endregion

        #region Commands
        #endregion
    }
}