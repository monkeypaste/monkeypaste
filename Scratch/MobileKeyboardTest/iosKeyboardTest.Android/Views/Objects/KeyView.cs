using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Avalonia;
using HarfBuzzSharp;
using System;
using System.Linq;
using Rect = Android.Graphics.Rect;

namespace iosKeyboardTest.Android {
    
    public class KeyView : CustomViewGroup, IKeyboardViewRenderer {
        CustomTextView SecondaryTextView { get; set; }
        CustomTextView PrimaryTextView { get; set; }

        public KeyViewModel DC { get; private set; }
        public KeyView(KeyViewModel kvm, Context context) : base(context) {
            DC = kvm;
            kvm.SetRenderer(this);

            PrimaryTextView = new CustomTextView(context).SetDefaultTextProps();      
            this.AddView(PrimaryTextView);

            SecondaryTextView = new CustomTextView(context).SetDefaultTextProps();            
            this.AddView(SecondaryTextView);

            Render(true);
            RoundCorners(DC.CornerRadius);
        }

        public void Render(bool invalidate) {
            Layout(false);
            Measure(false);
            Paint(invalidate);
        }

        public void Layout(bool invalidate) {
            PrimaryTextView.Text = DC.PrimaryValue;
            PrimaryTextView.TextAlignment = TextAlignment.Center;
            //PrimaryTextView.SizeToFit();

            SecondaryTextView.Text = DC.SecondaryValue;
            SecondaryTextView.TextAlignment = TextAlignment.ViewEnd;
            //SecondaryTextView.SizeToFit();

            if(invalidate) {
                PrimaryTextView.Redraw();
                SecondaryTextView.Redraw();
            }
        }
        public void Measure(bool invalidate) {
            float x = (float)(DC.X + (DC.OuterPadX / 2));
            float y = (float)(DC.Y + (DC.OuterPadY / 2));
            float w = (float)DC.InnerWidth;
            float h = (float)DC.InnerHeight;
            
            Frame = new RectF(x,y,w,h);

            PrimaryTextView.TextSize = (float)(DC.PrimaryFontSize * AndroidDisplayInfo.Scaling);
            PrimaryTextView.Frame = this.Frame;
            
            SecondaryTextView.TextSize = (float)DC.SecondaryFontSize;
            var sts = SecondaryTextView.TextSize();
            float sw = sts.Width;
            float sh = sts.Height;
            float sec_right_margin = (float)(5 * AndroidDisplayInfo.Scaling);
            float sx = w - sw - sec_right_margin;
            float sy = 0;
            SecondaryTextView.Frame = new RectF(sx, sy, SecondaryTextView.Width, SecondaryTextView.Height);

            TweakText();

            if (invalidate) {
                PrimaryTextView.Redraw();
                SecondaryTextView.Redraw();
                RoundCorners(DC.CornerRadius);
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
            this.SetBackgroundColor(bg);

            PrimaryTextView.SetTextColor(fg);

            if(DC.IsSecondaryVisible) {
                SecondaryTextView.Visibility = ViewStates.Visible;
                SecondaryTextView.SetTextColor(KeyboardPalette.FgHex2.ToColor());
            } else {
                SecondaryTextView.Visibility = ViewStates.Invisible;
            }
            

            //if(invalidate) {
                PrimaryTextView.Redraw();
                SecondaryTextView.Redraw();
                this.Redraw();
            //}
        }


        void TweakText() {
            
        }
        void RoundCorners(CornerRadius cr) {

        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b) {
        }
    }
    public class CustomViewGroup : ViewGroup {
        public RectF Frame { get; set; }
        public CustomViewGroup(Context context) : base(context) { }

        protected override void OnLayout(bool changed, int l, int t, int r, int b) {
            //base.OnLayout(changed, Frame.Left, Frame.Top, Frame.Right, Frame.Bottom);
        }
    }

    public class CustomTextView : TextView {
        public RectF Frame { get; set; }
        public CustomTextView(Context context) : base(context) { }
    }
}