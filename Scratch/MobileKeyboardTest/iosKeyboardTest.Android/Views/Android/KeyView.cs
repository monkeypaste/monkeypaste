using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Avalonia;
using HarfBuzzSharp;
using System;
using System.Linq;
using GPaint = Android.Graphics.Paint;
using Rect = Android.Graphics.Rect;

namespace iosKeyboardTest.Android {

    public class KeyView : CustomView, IKeyboardViewRenderer {
        Color PrimaryTextColor { get; set; }
        Color SecondaryTextColor { get; set; }
        RectF PrimaryTextRect { get; set; } = new();
        RectF SecondaryTextRect { get; set; } = new();
        public KeyViewModel DC { get; private set; }
        public KeyView(KeyViewModel kvm, Context context, Paint paint) : base(context,paint) {
            this.DC = kvm;
            kvm.SetRenderer(this);
        }

        public void Render(bool invalidate) {
            Layout(false);
            Measure(false);
            Paint(false);
            if(invalidate) {
                this.Redraw();
            }
        }

        public void Layout(bool invalidate) {
            if (invalidate) {
                this.Redraw();
            }
        }
        public void Measure(bool invalidate) {
            //float l = (float)(kvm.X + (kvm.OuterPadX / 2));
            //float t = (float)(kvm.Y + (kvm.OuterPadY / 2));
            //float r = l + (float)kvm.InnerWidth;
            //float b = t + (float)kvm.InnerHeight;            
            //var new_frame = new RectF(l,t,r,b);
            var new_frame = DC.InnerRect.ToRectF();
            if(new_frame.Left != Frame.Left) {

            }
            Frame = new_frame;

            var inner_rect = new RectF(0, 0, Frame.Width(), Frame.Height());

            if(DC.PrimaryValue == null) {
                PrimaryTextRect = new();
            } else {
                var ptb = new Rect();
                SharedPaint.TextSize = (float)DC.PrimaryFontSize;
                SharedPaint.GetTextBounds(DC.PrimaryValue.ToCharArray(), 0, DC.PrimaryValue.Length, ptb);
                float px = inner_rect.CenterX() + DC.PrimaryTranslateOffsetX.UnscaledF() - ((float)ptb.Width() / 2f);
                float py = inner_rect.CenterY() + DC.PrimaryTranslateOffsetY.UnscaledF() + ((float)ptb.Height() / 2f);
                PrimaryTextRect = new RectF(px, py, ptb.Width(), ptb.Height());
            }

            if(DC.SecondaryValue == null) {
                SecondaryTextRect = new();
            } else {
                var stb = new Rect();
                SharedPaint.TextSize = (float)DC.SecondaryFontSize;
                SharedPaint.GetTextBounds(DC.SecondaryValue.ToCharArray(), 0, DC.SecondaryValue.Length, stb);
                float sx = inner_rect.Right + DC.SecondaryTranslateOffsetX.UnscaledF() - ((float)stb.Width() / 1f);
                float sy = inner_rect.Top + DC.SecondaryTranslateOffsetY.UnscaledF() + ((float)stb.Height());
                SecondaryTextRect = new RectF(sx, sy, stb.Width(), stb.Height());
            }

            this.Layout((int)Frame.Left, (int)Frame.Top, (int)Frame.Right, (int)Frame.Bottom);

            if (invalidate) {
                this.Redraw();
            }
        }

        public void Paint(bool invalidate) {
            this.Visibility = DC.IsVisible ? ViewStates.Visible : ViewStates.Invisible;
            if (this.Visibility == ViewStates.Invisible) {
                return;
            }
            Color bg = KeyboardPalette.DefaultKeyBgHex.ToColor();
            Color fg = KeyboardPalette.FgHex.ToColor();
            if (DC.IsSpecial) {
                bg = DC.IsPressed ? KeyboardPalette.SpecialKeyPressedBgHex.ToColor() : KeyboardPalette.SpecialKeyBgHex.ToColor();
                if (DC.SpecialKeyType == SpecialKeyType.Shift) {
                    if (DC.IsShiftOn) {
                        fg = KeyboardPalette.ShiftHex.ToColor();
                    } else if (DC.IsShiftLock) {
                        bg = KeyboardPalette.ShiftHex.ToColor();
                    }
                } else if (DC.IsPrimarySpecial) {
                    bg = DC.IsPressed ? KeyboardPalette.PrimarySpecialKeyPressedBgHex.ToColor() : KeyboardPalette.PrimarySpecialKeyBgHex.ToColor();
                }
            } else if (DC.IsPopupKey) {
                bg = DC.IsActiveKey ? KeyboardPalette.HoldFocusBgHex.ToColor() : KeyboardPalette.HoldBgHex.ToColor();
                fg = KeyboardPalette.HoldFgHex.ToColor();
            } else {
                bg = DC.IsPressed ? KeyboardPalette.PressedBgHex.ToColor() : KeyboardPalette.DefaultKeyBgHex.ToColor();
            }
            BackgroundColor = bg;
            PrimaryTextColor = fg;

            if(DC.IsSecondaryVisible) {
                SecondaryTextColor = KeyboardPalette.FgHex2.ToColor();
            } else {
                SecondaryTextColor = Color.Argb(0, 0, 0, 0);
            }
            
            if(invalidate) {
                this.Redraw();
            }
        }
        protected override void OnDraw(Canvas canvas) {
            if (this.Visibility == ViewStates.Invisible) {
                return;
            }
            if(Frame.Top < 0) {
                Rect newRect = canvas.ClipBounds;
                newRect.Inset(0, (int)Frame.Top);
                canvas.ClipRect(newRect);
            }

            SharedPaint.Color = BackgroundColor;

            // fix precision rounding so popups are seamless
            int pad = DC.IsPopupKey ? 1 : 0;
            var rect = new RectF(0, 0, Frame.Width()+pad, Frame.Height()+pad);

            if (DC.CornerRadius == default) {
                canvas.DrawRect(rect, SharedPaint);
            } else {
                var corners = new double[]{
                    DC.CornerRadius.TopLeft, DC.CornerRadius.TopLeft,        // Top, left in px
                    DC.CornerRadius.TopRight, DC.CornerRadius.TopRight,        // Top, right in px
                    DC.CornerRadius.BottomRight, DC.CornerRadius.BottomRight,          // Bottom, right in px
                    DC.CornerRadius.BottomLeft, DC.CornerRadius.BottomLeft           // Bottom,left in px
                }.Select(x => x.UnscaledF()).ToArray();

                Path path = new Path();
                path.AddRoundRect(rect, corners, Path.Direction.Cw);
                canvas.DrawPath(path, SharedPaint);
            }

            if (!string.IsNullOrEmpty(DC.PrimaryValue)) {
                SharedPaint.TextSize = (float)DC.PrimaryFontSize;
                SharedPaint.Color = PrimaryTextColor;
                
                canvas.DrawText(DC.PrimaryValue, PrimaryTextRect.Left, PrimaryTextRect.Top, SharedPaint);
            }
            if (DC.IsSecondaryVisible) {
                SharedPaint.TextSize = (float)DC.SecondaryFontSize;
                SharedPaint.Color = SecondaryTextColor;
                canvas.DrawText(DC.SecondaryValue, SecondaryTextRect.Left, SecondaryTextRect.Top, SharedPaint);
            }
        }
    }

}