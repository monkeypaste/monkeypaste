using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using Avalonia;
using HarfBuzzSharp;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using static Android.Views.View;
using GPaint = Android.Graphics.Paint;
using Point = Android.Graphics.Point;
using Rect = Android.Graphics.Rect;

namespace iosKeyboardTest.Android {

    public class KeyView : 
        CustomView, 
        IKeyboardViewRenderer,
        IOnClickListener {
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
        public static Dictionary<string, Bitmap> ImageLookup { get; set; } = [];

        #endregion

        #region Interfaces

        #region IOnClickListener Implementation

        public void OnClick(View v) {
            DC.PerformTapActionCommand.Execute(null);
        }
        #endregion

        #region IkeyboardViewRenderer Implementation
        public override void Render(bool invalidate) {
            if (!DC.IsVisible) {
                return;
            }
            renderCount++;

            Layout(false);
            Measure(false);
            Paint(false);

            base.Render(invalidate);
        }
        public override void Layout(bool invalidate) {
            if (!DC.IsVisible) {
                return;
            }
            layoutCount++;
            base.Layout(invalidate);
        }
        public override void Measure(bool invalidate) {
            if (!DC.IsVisible) {
                return;
            }
            measureCount++;

            var new_frame = DC.InnerRect.ToRectF();//DC.KeyboardRect.ToRectF();
            Frame = new_frame;


            var inner_rect = Frame.ToBounds();

            // fix precision rounding so popups are seamless
            int pad = DC.IsPopupKey ? 1 : 0; 
            var key_rect = new RectF(0, 0, Frame.Width() + pad, Frame.Height() + pad);
            KeyPath = new Path();
            KeyPath.AddRoundRect(key_rect, DC.CornerRadius.ToCornerArray(), Path.Direction.Cw);

            float cx = inner_rect.CenterX();
            float cy = inner_rect.CenterY();
            float px = 0;
            float py = 0;

            float pfs = DC.PrimaryFontSize.UnscaledF();

            if (DC.PrimaryValue == null || DC.IsPrimaryImage) {
                var img_rect = DC.PrimaryImageRect.ToRectF();
                PrimaryLoc = img_rect.Location();
                if (ImageLookup.TryGetValue(DC.CurrentChar, out var bmp) &&
                    bmp.Width != img_rect.Width() || bmp.Height != img_rect.Height()) {
                    // first use, scale bmp
                    ImageLookup[DC.CurrentChar] = bmp.Scale((int)img_rect.Width(), (int)img_rect.Height());
                }
            } else {
                var ptb = new Rect();
                SharedPaint.TextAlign = GPaint.Align.Center;
                SharedPaint.TextSize = pfs;
                SharedPaint.GetTextBounds(DC.PrimaryValue.ToCharArray(), 0, DC.PrimaryValue.Length, PrimaryTextBounds);

                px = cx;
                py = cy - ((SharedPaint.Ascent() + SharedPaint.Descent()) / 2);
                PrimaryLoc = new PointF(px, py);
                PrimaryTextBounds = PrimaryTextBounds.Move((int)px, (int)py);
            }

            if (DC.SecondaryValue == null) {
                SecondaryLoc = new();
            } else {
                var stb = new Rect();
                SharedPaint.TextAlign = GPaint.Align.Center;
                SharedPaint.TextSize = (float)DC.SecondaryFontSize;
                SharedPaint.GetTextBounds(DC.SecondaryValue.ToCharArray(), 0, DC.SecondaryValue.Length, stb);
                float sx = cx;
                float sy = cy - ((SharedPaint.Ascent() + SharedPaint.Descent()) / 2);
                sx += sx * (float)DC.SecondaryOffsetRatio.X;
                sy -= sy * (float)DC.SecondaryOffsetRatio.Y;
                SecondaryLoc = new PointF(sx, sy);
            }

            this.Layout((int)Frame.Left, (int)Frame.Top, (int)Frame.Right, (int)Frame.Bottom);

            if (!DC.IsPopupKey && DC.PopupKeys.Any()) {
                if (!IsPopupVisible && Parent is KeyGridView kgv) {
                    //kgv.ShowPopup(this);
                    IsPopupVisible = true;
                }
            } else if (IsPopupVisible && Parent is KeyGridView kgv) {
                //kgv.HidePopup(this);
                IsPopupVisible = false;
            }
            base.Layout(invalidate);
        }
        public override void Paint(bool invalidate) {
            this.Visibility = DC.IsVisible ? ViewStates.Visible : ViewStates.Invisible;
            if (this.Visibility == ViewStates.Invisible) {
                return;
            }
            paintCount++;
            DC.SetBrushes();
            BackgroundColor = DC.BgHex.ToColor();
            PrimaryTextColor = DC.PrimaryHex.ToColor();
            SecondaryTextColor = DC.SecondaryHex.ToColor();

            base.Paint(invalidate);
        }
        #endregion

        #endregion

        #region Properties

        #region Members

        #endregion

        #region View Models
        public KeyViewModel DC { get; private set; }
        #endregion

        #region Appearance
        Color PrimaryTextColor { get; set; }
        Color SecondaryTextColor { get; set; }
        PointF PrimaryLoc { get; set; } = new();
        PointF SecondaryLoc { get; set; } = new();
        Rect PrimaryTextBounds { get; set; } = new();

        Path KeyPath { get; set; } = new();

        #endregion

        #region Layout
        #endregion

        #region State
        bool IsPopupVisible { get; set; }
        #endregion

        #region Models
        #endregion

        #endregion

        #region Events
        #endregion

        #region Constructors
        public KeyView(KeyViewModel kvm, Context context, Paint paint) : base(context, paint) {
            this.DC = kvm;
            kvm.SetRenderer(this);

            InitImages();
            Measure(false);
        }
        #endregion

        #region Public Methods

        public void ResetStats() {
            measureCount = 0;
            layoutCount = 0;
            paintCount = 0;
            renderCount = 0;
            drawCount = 0;
        }
        public override string ToString() {
            return $"'{DC.PrimaryValue}' M:{measureCount} L:{layoutCount} P:{paintCount} R:{renderCount} D:{drawCount}";
        }
        #endregion

        #region Protected Methods
        protected override void OnDraw(Canvas canvas) {
            if (this.Visibility == ViewStates.Invisible) {
                return;
            }
            drawCount++;
            SharedPaint.Color = BackgroundColor;
            
            canvas.DrawPath(KeyPath, SharedPaint);

            if (!string.IsNullOrEmpty(DC.PrimaryValue)) {
                if (DC.IsPrimaryImage) {
                    var r = DC.PrimaryImageRect.ToRect();
                    SharedPaint.SetTint(PrimaryTextColor);

                    if(ImageLookup.TryGetValue(DC.CurrentChar, out var bmp)) {
                        canvas.DrawBitmap(bmp, PrimaryLoc.X, PrimaryLoc.Y, SharedPaint);
                    }
                    
                    SharedPaint.SetColorFilter(null);
                } else {
                    //SharedPaint.Color = Color.Orange;
                    //canvas.DrawRect(PrimaryTextBounds, SharedPaint);

                    SharedPaint.TextAlign = GPaint.Align.Center;
                    SharedPaint.TextSize = DC.PrimaryFontSize.UnscaledF();
                    SharedPaint.Color = PrimaryTextColor;

                    canvas.DrawText(DC.PrimaryValue, PrimaryLoc.X, PrimaryLoc.Y, SharedPaint);
                }
            }
            if (DC.IsSecondaryVisible) {
                SharedPaint.TextAlign = GPaint.Align.Center;
                SharedPaint.TextSize = DC.SecondaryFontSize.UnscaledF();
                SharedPaint.Color = SecondaryTextColor;
                canvas.DrawText(DC.SecondaryValue, SecondaryLoc.X, SecondaryLoc.Y, SharedPaint);
            }
        }
        #endregion

        #region Private Methods
        void InitImages() {
            if (ImageLookup.Any()) {
                // already loaded
                return;
            }

            // from https://stackoverflow.com/a/12486221/105028

            ImageLookup = new Dictionary<string, Bitmap>() {
                { KeyViewModel.SHIFT_IMG_FILE_NAME, CreateBitmap(Resource.Drawable.shift) },
                { KeyViewModel.SHIFT_ON_IMG_FILE_NAME, CreateBitmap(Resource.Drawable.shift_on) },
                { KeyViewModel.SHIFT_LOCK_IMG_FILE_NAME, CreateBitmap(Resource.Drawable.shift_lock) },

                { KeyViewModel.SEARCH_IMG_FILE_NAME, CreateBitmap(Resource.Drawable.search) },
                { KeyViewModel.ENTER_IMG_FILE_NAME, CreateBitmap(Resource.Drawable.enter) },
                { KeyViewModel.BACKSPACE_IMG_FILE_NAME, CreateBitmap(Resource.Drawable.backspace) },
                { KeyViewModel.EMOJI_SELECT_BTN_IMG_FILE_NAME, CreateBitmap(Resource.Drawable.emoji) },
                { KeyViewModel.NEXT_KEYBOARD_IMG_FILE_NAME, CreateBitmap(Resource.Drawable.globe) }
            };
            //ImageLookup = new Dictionary<string, Drawable>() {
            //    { KeyViewModel.SHIFT_IMG_FILE_NAME, Context.Resources.GetDrawable(Resource.Drawable.shift,null) },
            //    { KeyViewModel.SHIFT_ON_IMG_FILE_NAME, Context.Resources.GetDrawable(Resource.Drawable.shift_on, null) },
            //    { KeyViewModel.SHIFT_LOCK_IMG_FILE_NAME, Context.Resources.GetDrawable(Resource.Drawable.shift_lock, null) },

            //    { KeyViewModel.SEARCH_IMG_FILE_NAME, Context.Resources.GetDrawable(Resource.Drawable.search, null) },
            //    { KeyViewModel.ENTER_IMG_FILE_NAME, Context.Resources.GetDrawable(Resource.Drawable.enter, null) },
            //    { KeyViewModel.BACKSPACE_IMG_FILE_NAME, Context.Resources.GetDrawable(Resource.Drawable.backspace, null) },
            //    { KeyViewModel.EMOJI_SELECT_BTN_IMG_FILE_NAME, Context.Resources.GetDrawable(Resource.Drawable.emoji, null) },
            //    { KeyViewModel.NEXT_KEYBOARD_IMG_FILE_NAME, Context.Resources.GetDrawable(Resource.Drawable.globe, null) }
            //};
        }

        Bitmap CreateBitmap(int res_id, string hex = default) {
            return Context.Resources.GetDrawable(res_id, null).ToBitmap();
        }


        #endregion

    }

}