using Avalonia;
using CoreAnimation;
using CoreGraphics;
using System;
using System.Linq;
using UIKit;

namespace iosKeyboardTest.iOS {
    public class KeyView : UIView, IViewHelper {
        UITextView SecondaryTextView { get; set; }
        UITextView PrimaryTextView { get; set; }
        public KeyViewModel_fallback DC { get; private set; }
        public KeyView(KeyViewModel_fallback kvm) : base() {
            this.TranslatesAutoresizingMaskIntoConstraints = false;
            DC = kvm;
            kvm.VH = this;

            PrimaryTextView = new UITextView() { 
                BackgroundColor = UIColor.FromRGBA(0, 0, 0, 0),
                TranslatesAutoresizingMaskIntoConstraints = false,
                Selectable = false,
                
            };
            PrimaryTextView.TextContainer.LineBreakMode = UILineBreakMode.TailTruncation;
            PrimaryTextView.TextContainer.MaximumNumberOfLines = 1;
            PrimaryTextView.Font = UIFont.SystemFontOfSize((nfloat)DC.PrimaryFontSize, UIFontWeight.Regular);
            this.AddSubview(PrimaryTextView);

            SecondaryTextView = new UITextView() { 
                BackgroundColor = UIColor.FromRGBA(0, 0, 0, 0), 
                TranslatesAutoresizingMaskIntoConstraints = false,
                Selectable = false
            };
            PrimaryTextView.TextContainer.LineBreakMode = UILineBreakMode.TailTruncation;
            PrimaryTextView.TextContainer.MaximumNumberOfLines = 1;
            SecondaryTextView.Font = UIFont.SystemFontOfSize((nfloat)DC.SecondaryFontSize, UIFontWeight.Regular);
            this.AddSubview(SecondaryTextView);

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
            PrimaryTextView.SizeToFit();

            SecondaryTextView.Text = DC.SecondaryValue;
            SecondaryTextView.SizeToFit();

            if(invalidate) {
                Redraw(PrimaryTextView);
                Redraw(SecondaryTextView);
            }
        }
        public void Measure(bool invalidate) {
            double x = DC.X + (DC.OuterPadX / 2);
            double y = DC.Y + (DC.OuterPadY / 2);
            double w = DC.InnerWidth;
            double h = DC.InnerHeight;
            Frame = new CGRect(x,y,w,h);


            double px = (w / 2) - (PrimaryTextView.Frame.Width / 2);
            double py = (h / 2) - (PrimaryTextView.Frame.Height / 2);
            PrimaryTextView.Frame = new CGRect(px, py, PrimaryTextView.Frame.Width, PrimaryTextView.Frame.Height);

            double pad = 5;
            double sx = w - SecondaryTextView.Frame.Width - pad;
            double sy = 0;
            SecondaryTextView.Frame = new CGRect(sx, sy, SecondaryTextView.Frame.Width, SecondaryTextView.Frame.Height);

            if(invalidate) {
                Redraw(PrimaryTextView);
                Redraw(SecondaryTextView);
                RoundCorners(DC.CornerRadius);
                Redraw(this);
            }
        }

        public void Paint(bool invalidate) {
            this.Hidden = !DC.IsVisible;
            if (this.Hidden) {
                return;
            }
            UIColor bg = KeyboardView.DefaultBgBrush;
            UIColor fg = KeyboardView.FgBrush;
            if (DC.IsSpecial) {
                bg = DC.IsPressed ? KeyboardView.SpecialPressedBgBrush : KeyboardView.SpecialBgBrush;
                if (DC.SpecialKeyType == SpecialKeyType_fallback.Shift) {
                    if (DC.IsShiftOn) {
                        fg = KeyboardView.ShiftBrush;
                    } else if (DC.IsShiftLock) {
                        bg = KeyboardView.ShiftBrush;
                    }
                } else if (DC.IsPrimarySpecial) {
                    bg = DC.IsPressed ? KeyboardView.PrimarySpecialPressedBgBrush : KeyboardView.PrimarySpecialBgBrush;
                }
            } else if (DC.IsPopupKey) {
                bg = DC.IsActiveKey ? KeyboardView.HoldFocusBgBrush : KeyboardView.HoldBgBrush;
                fg = KeyboardView.HoldFgBrush;
            } else {
                bg = DC.IsPressed ? KeyboardView.PressedBgBrush : KeyboardView.DefaultBgBrush;
            }
            BackgroundColor = bg;

            PrimaryTextView.TextColor = fg;

            if(DC.IsSecondaryVisible) {
                SecondaryTextView.Hidden = false;
                SecondaryTextView.TextColor = KeyboardView.FgBrush2;
            } else {
                SecondaryTextView.Hidden = true;
            }
            

            if(invalidate) {
                Redraw(PrimaryTextView);
                Redraw(SecondaryTextView);
                Redraw(this);
            }
        }

        void Redraw(UIView v) {
            v.Layer.SetNeedsDisplay();
            v.Layer.DisplayIfNeeded();
        }

        void RoundCorners(CornerRadius cr) {
            var tlr = new CGSize(cr.TopLeft, cr.TopLeft);
            var trr = new CGSize(cr.TopRight, cr.TopRight);
            var brr = new CGSize(cr.BottomRight, cr.BottomRight);
            var blr = new CGSize(cr.BottomLeft, cr.BottomLeft);
            var maskPath = new UIBezierPathExt(Bounds, tlr, trr, brr, blr);
            var shape = new CAShapeLayer() {
                Path = maskPath.CGPath
            };
            this.Layer.Mask = shape;
        }
    }

    public static class ViewHelpers {

    }
}