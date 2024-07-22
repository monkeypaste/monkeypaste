using Android.Graphics;
using Android.Views;
using System.Linq;

namespace iosKeyboardTest.Android {
    public class KeyRenderer : IKeyboardViewRenderer {
        //TextureView mSurface { get; set; }
        //Paint paint { get; set; }
        KeyViewModel kvm { get; set; }
        public KeyRenderer(KeyViewModel kvm/*, Paint paint, TextureView surface*/) {
            //mSurface = surface;
            //this.paint = paint;
            this.kvm = kvm;
            this.kvm.SetRenderer(this);
        }
        public void DrawKey(Canvas canvas, Paint paint) {
            if (!kvm.IsVisible) {
                return;
            }
            float x = kvm.X.UnscaledF() + (kvm.OuterPadX.UnscaledF() / 2);
            float y = kvm.Parent.MenuHeight.UnscaledF() + kvm.Y.UnscaledF() + (kvm.OuterPadY.UnscaledF() / 2);
            float w = kvm.InnerWidth.UnscaledF();
            float h = kvm.InnerHeight.UnscaledF();
            string pv = kvm.PrimaryValue;
            string sv = kvm.SecondaryValue;

            object bg = default;
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

            var cr = kvm.CornerRadius;

            var corners = new double[]{
                cr.TopLeft, cr.TopLeft,        // Top, left in px
                cr.TopRight, cr.TopRight,        // Top, right in px
                cr.BottomRight, cr.BottomRight,          // Bottom, right in px
                cr.BottomLeft, cr.BottomLeft           // Bottom,left in px
            }.Select(x => x.UnscaledF()).ToArray();


            Path path = new Path();
            float l = x;
            float t = y;
            float r = x + w;
            float b = y + h;
            var rect = new RectF(l, t, r, b);
            if (bg is Color bgColor) {
                paint.Color = bgColor;
            } else if (bg is Shader shader) {
                paint.SetShader(shader);
            }

            path.AddRoundRect(rect, corners, Path.Direction.Cw);
            canvas.DrawPath(path, paint);
            paint.SetShader(null);

            if (!string.IsNullOrEmpty(kvm.PrimaryValue)) {
                paint.TextAlign = Paint.Align.Left;
                paint.TextSize = kvm.PrimaryFontSize.UnscaledF();
                var ptb = new Rect();
                paint.GetTextBounds(kvm.PrimaryValue.ToCharArray(), 0, kvm.PrimaryValue.Length, ptb);
                float px = rect.CenterX() + kvm.PrimaryTranslateOffsetX.UnscaledF() - ((float)ptb.Width() / 2f);
                float py = rect.CenterY() + kvm.PrimaryTranslateOffsetY.UnscaledF() + ((float)ptb.Height() / 2f);
                paint.Color = fg;
                canvas.DrawText(kvm.PrimaryValue, px, py, paint);
                canvas.DrawText(kvm.PrimaryValue, px, py, paint);
            }
            if (kvm.IsSecondaryVisible) {
                paint.TextAlign = Paint.Align.Left;
                paint.TextSize = kvm.SecondaryFontSize.UnscaledF();
                paint.Color = KeyboardPalette.FgHex.ToColor();

                var stb = new Rect();
                paint.GetTextBounds(kvm.SecondaryValue.ToCharArray(), 0, kvm.SecondaryValue.Length, stb);
                float sx = r + kvm.SecondaryTranslateOffsetX.UnscaledF() - ((float)stb.Width() / 2f);
                float sy = t + kvm.SecondaryTranslateOffsetY.UnscaledF() + ((float)stb.Height());
                canvas.DrawText(kvm.SecondaryValue, sx, sy, paint);
            }

        }
        public void Layout(bool invalidate) {
            if (invalidate) {
                RenderInternal();
            }
        }

        public void Measure(bool invalidate) {
            if (invalidate) {
                RenderInternal();
            }
        }

        void IKeyboardViewRenderer.Paint(bool invalidate) {
            if (invalidate) {
                RenderInternal();
            }
        }

        public void Render(bool invalidate) {
            if (invalidate) {
                RenderInternal();
            }
        }

        void RenderInternal() {
            //bool is_locked = KeyboardRenderer.LockedCanvas != null;
            //if(is_locked) {
            //    DrawKey(KeyboardRenderer.LockedCanvas);
            //    return;
            //}
            //Canvas canvas = mSurface.LockCanvas(null);
            //try {
            //    DrawKey(canvas);
            //}
            //finally {
            //    mSurface.UnlockCanvasAndPost(canvas);
            //}
            //KeyboardRenderer.Instance.RenderInternal();

        }
    }
}
