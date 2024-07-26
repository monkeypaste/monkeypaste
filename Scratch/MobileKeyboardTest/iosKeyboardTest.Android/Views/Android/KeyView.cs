using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using Avalonia;
using HarfBuzzSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using GPaint = Android.Graphics.Paint;
using Point = Android.Graphics.Point;
using Rect = Android.Graphics.Rect;

namespace iosKeyboardTest.Android {

    public class KeyView : CustomView, IKeyboardViewRenderer {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        static Dictionary<string, Drawable> ImageLookup { get; set; } = [];

        #endregion

        #region Interfaces

        #region IkeyboardViewRenderer Implementation
        public void Render(bool invalidate) {
            Layout(false);
            Measure(false);
            Paint(false);
            if (invalidate) {
                this.Redraw();
            }
        }

        public void Layout(bool invalidate) {
            if (invalidate) {
                this.Redraw();
            }
        }
        public void Measure(bool invalidate) {
            var new_frame = DC.InnerRect.ToRectF();
            if (new_frame.Left != Frame.Left) {

            }
            Frame = new_frame;

            Cr = DC.CornerRadius;

            var inner_rect = Frame.ToBounds();

            float cx = inner_rect.CenterX();
            float cy = inner_rect.CenterY();
            float px = 0;
            float py = 0;

            float pfs = DC.PrimaryFontSize.UnscaledF();

            if (DC.PrimaryValue == null || DC.IsPrimaryImage) {
                PrimaryTextLoc = new();
            } else {
                var ptb = new Rect();
                SharedPaint.TextAlign = GPaint.Align.Center;
                SharedPaint.TextSize = pfs;
                SharedPaint.GetTextBounds(DC.PrimaryValue.ToCharArray(), 0, DC.PrimaryValue.Length, ptb);
                px = cx;
                py = cy - ((SharedPaint.Ascent() + SharedPaint.Descent()) / 2);
                PrimaryTextLoc = new PointF(px, py);
            }

            if (DC.SecondaryValue == null) {
                SecondaryTextLoc = new();
            } else {
                var stb = new Rect();
                SharedPaint.TextAlign = GPaint.Align.Center;
                SharedPaint.TextSize = (float)DC.SecondaryFontSize;
                SharedPaint.GetTextBounds(DC.SecondaryValue.ToCharArray(), 0, DC.SecondaryValue.Length, stb);
                float sx = cx;
                float sy = cy - ((SharedPaint.Ascent() + SharedPaint.Descent()) / 2);
                sx = sx + (sx * 0.5f);
                sy = sy - (sy * 0.5f);
                SecondaryTextLoc = new PointF(sx, sy);
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
            if (invalidate) {
                this.Redraw();
            }
        }

        public void Paint(bool invalidate) {
            this.Visibility = DC.IsVisible ? ViewStates.Visible : ViewStates.Invisible;
            if (this.Visibility == ViewStates.Invisible) {
                return;
            }
            DC.SetBrushes();
            BackgroundColor = DC.BgHex.ToColor();
            PrimaryTextColor = DC.PrimaryHex.ToColor();
            PrimaryColorFilter = DC.IsPrimaryImage ? new LightingColorFilter(PrimaryTextColor.ToInt(), 0) : null;
            SecondaryTextColor = DC.SecondaryHex.ToColor();

            if (invalidate) {
                this.Redraw();
            }
        }
        #endregion

        #endregion

        #region Properties

        #region View Models
        public KeyViewModel DC { get; private set; }
        #endregion

        #region Appearance
        Color PrimaryTextColor { get; set; }
        ColorFilter? PrimaryColorFilter { get; set; }
        Color SecondaryTextColor { get; set; }
        PointF PrimaryTextLoc { get; set; } = new();
        PointF SecondaryTextLoc { get; set; } = new();

        float[] corners;

        CornerRadius _cr;
        CornerRadius Cr {
            get => _cr;
            set {
                if (Cr == default || Cr.TopLeft != value.TopLeft || Cr.TopRight != value.TopRight || Cr.BottomRight != value.BottomRight || Cr.BottomLeft != value.BottomLeft) {
                    _cr = value;

                    corners = new double[]{
                        value.TopLeft, value.TopLeft,        // Top, left in px
                        value.TopRight, value.TopRight,        // Top, right in px
                        value.BottomRight, value.BottomRight,          // Bottom, right in px
                        value.BottomLeft, value.BottomLeft           // Bottom,left in px
                    }.Select(x => x.UnscaledF()).ToArray();
                }
            }
        }

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
        }
        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        protected override void OnDraw(Canvas canvas) {
            if (this.Visibility == ViewStates.Invisible) {
                return;
            }
            SharedPaint.Color = BackgroundColor;

            // fix precision rounding so popups are seamless
            int pad = DC.IsPopupKey ? 1 : 0;
            var rect = new RectF(0, 0, Frame.Width() + pad, Frame.Height() + pad);

            if (DC.CornerRadius == default) {
                canvas.DrawRect(rect, SharedPaint);
            } else {
                Path path = new Path();
                path.AddRoundRect(rect, corners, Path.Direction.Cw);
                canvas.DrawPath(path, SharedPaint);
            }


            if (!string.IsNullOrEmpty(DC.PrimaryValue)) {
                if (DC.IsPrimaryImage) {
                    var r = DC.PrimaryImageRect.ToRect();
                    //SharedPaint.Color = Color.Orange;
                    //canvas.DrawRect(r, SharedPaint);
                    SharedPaint.SetColorFilter(PrimaryColorFilter);
                    SharedPaint.Color = PrimaryTextColor;

                    if(ImageLookup.TryGetValue(DC.CurrentChar, out var bmp)) {
                        bmp.SetBounds(r.Left,r.Top,r.Right,r.Bottom);
                        bmp.Draw(canvas);
                    }
                    
                    SharedPaint.SetColorFilter(null);
                } else {
                    SharedPaint.TextAlign = GPaint.Align.Center;
                    SharedPaint.TextSize = DC.PrimaryFontSize.UnscaledF();
                    SharedPaint.Color = PrimaryTextColor;

                    canvas.DrawText(DC.PrimaryValue, PrimaryTextLoc.X, PrimaryTextLoc.Y, SharedPaint);
                }
            }
            if (DC.IsSecondaryVisible) {
                SharedPaint.TextAlign = GPaint.Align.Center;
                SharedPaint.TextSize = DC.SecondaryFontSize.UnscaledF();
                SharedPaint.Color = SecondaryTextColor;
                canvas.DrawText(DC.SecondaryValue, SecondaryTextLoc.X, SecondaryTextLoc.Y, SharedPaint);
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
            //ImageLookup = new Dictionary<string, Bitmap>() {
            //    { KeyViewModel.SHIFT_IMG_FILE_NAME, CreateBitmap(Resource.Drawable.shift) },
            //    { KeyViewModel.SHIFT_ON_IMG_FILE_NAME, CreateBitmap(Resource.Drawable.shift_on) },
            //    { KeyViewModel.SHIFT_LOCK_IMG_FILE_NAME, CreateBitmap(Resource.Drawable.shift_lock) },

            //    { KeyViewModel.SEARCH_IMG_FILE_NAME, CreateBitmap(Resource.Drawable.search) },
            //    { KeyViewModel.ENTER_IMG_FILE_NAME, CreateBitmap(Resource.Drawable.enter) },
            //    { KeyViewModel.BACKSPACE_IMG_FILE_NAME, CreateBitmap(Resource.Drawable.backspace) },
            //    { KeyViewModel.EMOJI_SELECT_BTN_IMG_FILE_NAME, CreateBitmap(Resource.Drawable.emoji) },
            //    { KeyViewModel.NEXT_KEYBOARD_IMG_FILE_NAME, CreateBitmap(Resource.Drawable.globe) }
            //};
            ImageLookup = new Dictionary<string, Drawable>() {
                { KeyViewModel.SHIFT_IMG_FILE_NAME, Context.Resources.GetDrawable(Resource.Drawable.shift,null) },
                { KeyViewModel.SHIFT_ON_IMG_FILE_NAME, Context.Resources.GetDrawable(Resource.Drawable.shift_on, null) },
                { KeyViewModel.SHIFT_LOCK_IMG_FILE_NAME, Context.Resources.GetDrawable(Resource.Drawable.shift_lock, null) },

                { KeyViewModel.SEARCH_IMG_FILE_NAME, Context.Resources.GetDrawable(Resource.Drawable.search, null) },
                { KeyViewModel.ENTER_IMG_FILE_NAME, Context.Resources.GetDrawable(Resource.Drawable.enter, null) },
                { KeyViewModel.BACKSPACE_IMG_FILE_NAME, Context.Resources.GetDrawable(Resource.Drawable.backspace, null) },
                { KeyViewModel.EMOJI_SELECT_BTN_IMG_FILE_NAME, Context.Resources.GetDrawable(Resource.Drawable.emoji, null) },
                { KeyViewModel.NEXT_KEYBOARD_IMG_FILE_NAME, Context.Resources.GetDrawable(Resource.Drawable.globe, null) }
            };
        }

        Bitmap CreateBitmap(int res_id, string hex = default) {
            return Context.Resources.GetDrawable(res_id, null).ToBitmap();
        }
        #endregion

    }
}