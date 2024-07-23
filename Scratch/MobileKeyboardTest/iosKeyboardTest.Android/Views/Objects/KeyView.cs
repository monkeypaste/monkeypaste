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
        public KeyViewModel kvm { get; private set; }
        public KeyView(KeyViewModel kvm, Context context, Paint paint) : base(context,paint) {
            this.kvm = kvm;
            kvm.SetRenderer(this);
        }

        public void Render(bool invalidate) {
            Layout(false);
            Measure(false);
            Paint(invalidate);
        }

        public void Layout(bool invalidate) {
            if (invalidate) {
                this.Redraw();
            }
        }
        public void Measure(bool invalidate) {
            float l = (float)(kvm.X + (kvm.OuterPadX / 2));
            float t = (float)(kvm.Y + (kvm.OuterPadY / 2));
            float r = l + (float)kvm.InnerWidth;
            float b = t + (float)kvm.InnerHeight;            
            var new_frame = new RectF(l,t,r,b);

            bool needs_layout = new_frame.Left != Frame.Left || new_frame.Top != Frame.Top;
            Frame = new_frame;

            var inner_rect = new RectF(0, 0, Frame.Width(), Frame.Height());

            if(kvm.PrimaryValue == null) {
                PrimaryTextRect = new();
            } else {
                var ptb = new Rect();
                SharedPaint.TextSize = (float)kvm.PrimaryFontSize;
                SharedPaint.GetTextBounds(kvm.PrimaryValue.ToCharArray(), 0, kvm.PrimaryValue.Length, ptb);
                float px = inner_rect.CenterX() + kvm.PrimaryTranslateOffsetX.UnscaledF() - ((float)ptb.Width() / 2f);
                float py = inner_rect.CenterY() + kvm.PrimaryTranslateOffsetY.UnscaledF() + ((float)ptb.Height() / 2f);
                PrimaryTextRect = new RectF(px, py, ptb.Width(), ptb.Height());
            }

            if(kvm.SecondaryValue == null) {
                SecondaryTextRect = new();
            } else {
                var stb = new Rect();
                SharedPaint.TextSize = (float)kvm.SecondaryFontSize;
                SharedPaint.GetTextBounds(kvm.SecondaryValue.ToCharArray(), 0, kvm.SecondaryValue.Length, stb);
                float sx = inner_rect.Right + kvm.SecondaryTranslateOffsetX.UnscaledF() - ((float)stb.Width() / 1f);
                float sy = inner_rect.Top + kvm.SecondaryTranslateOffsetY.UnscaledF() + ((float)stb.Height());
                SecondaryTextRect = new RectF(sx, sy, stb.Width(), stb.Height());
            }

            if (needs_layout) {
                this.Layout((int)Frame.Left, (int)Frame.Top, (int)Frame.Right, (int)Frame.Bottom);
            }

            if (invalidate) {
                this.Redraw();
            }
        }

        public void Paint(bool invalidate) {
            this.Visibility = kvm.IsVisible ? ViewStates.Visible : ViewStates.Invisible;
            if (this.Visibility == ViewStates.Invisible) {
                return;
            }
            Color bg = KeyboardPalette.DefaultKeyBgHex.ToColor();
            Color fg = KeyboardPalette.FgHex.ToColor();
            if (kvm.IsSpecial) {
                bg = kvm.IsPressed ? KeyboardPalette.SpecialKeyPressedBgHex.ToColor() : KeyboardPalette.SpecialKeyBgHex.ToColor();
                if (kvm.SpecialKeyType == SpecialKeyType.Shift) {
                    if (kvm.IsShiftOn) {
                        fg = KeyboardPalette.ShiftHex.ToColor();
                    } else if (kvm.IsShiftLock) {
                        bg = KeyboardPalette.ShiftHex.ToColor();
                    }
                } else if (kvm.IsPrimarySpecial) {
                    bg = kvm.IsPressed ? KeyboardPalette.PrimarySpecialKeyPressedBgHex.ToColor() : KeyboardPalette.PrimarySpecialKeyBgHex.ToColor();
                }
            } else if (kvm.IsPopupKey) {
                bg = kvm.IsActiveKey ? KeyboardPalette.HoldFocusBgHex.ToColor() : KeyboardPalette.HoldBgHex.ToColor();
                fg = KeyboardPalette.HoldFgHex.ToColor();
            } else {
                bg = kvm.IsPressed ? KeyboardPalette.PressedBgHex.ToColor() : KeyboardPalette.DefaultKeyBgHex.ToColor();
            }
            BackgroundColor = bg;
            PrimaryTextColor = fg;

            if(kvm.IsSecondaryVisible) {
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

            SharedPaint.Color = BackgroundColor;

            // fix precision rounding so popups are seamless
            int pad = kvm.IsPopupKey ? 1 : 0;
            var rect = new RectF(0, 0, Frame.Width()+pad, Frame.Height()+pad);

            if (kvm.CornerRadius == default) {
                canvas.DrawRect(rect, SharedPaint);
            } else {
                var corners = new double[]{
                    kvm.CornerRadius.TopLeft, kvm.CornerRadius.TopLeft,        // Top, left in px
                    kvm.CornerRadius.TopRight, kvm.CornerRadius.TopRight,        // Top, right in px
                    kvm.CornerRadius.BottomRight, kvm.CornerRadius.BottomRight,          // Bottom, right in px
                    kvm.CornerRadius.BottomLeft, kvm.CornerRadius.BottomLeft           // Bottom,left in px
                }.Select(x => x.UnscaledF()).ToArray();

                Path path = new Path();
                path.AddRoundRect(rect, corners, Path.Direction.Cw);
                canvas.DrawPath(path, SharedPaint);
            }

            if (!string.IsNullOrEmpty(kvm.PrimaryValue)) {
                SharedPaint.TextSize = (float)kvm.PrimaryFontSize;
                SharedPaint.Color = PrimaryTextColor;
                canvas.DrawText(kvm.PrimaryValue, PrimaryTextRect.Left, PrimaryTextRect.Top, SharedPaint);
            }
            if (kvm.IsSecondaryVisible) {
                SharedPaint.TextSize = (float)kvm.SecondaryFontSize;
                SharedPaint.Color = SecondaryTextColor;
                canvas.DrawText(kvm.SecondaryValue, SecondaryTextRect.Left, SecondaryTextRect.Top, SharedPaint);
            }
        }
    }

}