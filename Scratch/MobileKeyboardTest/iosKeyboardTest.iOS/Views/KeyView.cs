using Avalonia;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using System;
using System.Linq;
using UIKit;

namespace iosKeyboardTest.iOS {
    public class KeyView : UIView, IKeyboardViewRenderer {
        UITextView SecondaryTextView { get; set; }
        UITextView PrimaryTextView { get; set; }
        public KeyViewModel DC { get; private set; }
        public KeyView(KeyViewModel kvm) : base() {
            DC = kvm;
            kvm.SetRenderer(this);

            PrimaryTextView = new UITextView().SetDefaultTextProps();            
            this.AddSubview(PrimaryTextView);

            SecondaryTextView = new UITextView().SetDefaultTextProps();            
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
                PrimaryTextView.Redraw();
                SecondaryTextView.Redraw();
            }
        }
        public void Measure(bool invalidate) {
            double x = DC.X + (DC.OuterPadX / 2);
            double y = DC.Y + (DC.OuterPadY / 2);
            double w = DC.InnerWidth;
            double h = DC.InnerHeight;
            Frame = new CGRect(x,y,w,h);

            double pfs = DC.PrimaryFontSize * UIScreen.MainScreen.Scale;
            PrimaryTextView.TranslatesAutoresizingMaskIntoConstraints = true;
            if (PrimaryTextView.Font == null || PrimaryTextView.Font.PointSize != pfs) {
                PrimaryTextView.Font = UIFont.SystemFontOfSize((nfloat)pfs, UIFontWeight.Regular);
                //PrimaryTextView.SizeToFit();
            }
            PrimaryTextView.TextAlignment = UITextAlignment.Center;
            var pts = PrimaryTextView.TextSize();
            double pw = pts.Width;
            double ph = pts.Height;
            double px = (w / 2) - (pw / 2);
            double py = (h / 2) - (ph / 2);// - (pfs/2);
            PrimaryTextView.Frame = new CGRect(px, py, PrimaryTextView.Bounds.Width,PrimaryTextView.Bounds.Height);

            double sfs = DC.SecondaryFontSize;
            if (SecondaryTextView.Font == null || SecondaryTextView.Font.PointSize != sfs) {
                SecondaryTextView.Font = UIFont.SystemFontOfSize((nfloat)sfs, UIFontWeight.Regular);
                SecondaryTextView.SizeToFit();
            }
            var sts = SecondaryTextView.TextSize();
            double sw = sts.Width;
            double sh = sts.Height;
            double sec_right_margin = 5 * UIScreen.MainScreen.Scale;
            double sx = w - sw - sec_right_margin;
            double sy = 0;
            SecondaryTextView.Frame = new CGRect(sx, sy, SecondaryTextView.Bounds.Width, SecondaryTextView.Bounds.Height);
            SecondaryTextView.SizeToFit();


            if (invalidate) {
                PrimaryTextView.Redraw();
                SecondaryTextView.Redraw();
                RoundCorners(DC.CornerRadius);
                this.Redraw();
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
                if (DC.SpecialKeyType == SpecialKeyType.Shift) {
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
            

            //if(invalidate) {
                PrimaryTextView.Redraw();
                SecondaryTextView.Redraw();
                this.Redraw();
            //}
        }


        void RoundCorners(CornerRadius cr) {
            var scale = UIScreen.MainScreen.Scale;
            if(UIDevice.CurrentDevice.CheckSystemVersion(11,0)) {
                // from https://stackoverflow.com/a/71329483/105028
                UIRectCorner corner_mask = (UIRectCorner)0;
                double radius = 0;
                if (cr.TopLeft > 0) {
                    corner_mask |= UIRectCorner.TopLeft;
                    radius = cr.TopLeft * scale;
                }
                if (cr.TopRight > 0) {
                    corner_mask |= UIRectCorner.TopRight;
                    radius = cr.TopRight * scale;
                }
                if (cr.BottomRight > 0) {
                    corner_mask |= UIRectCorner.BottomRight;
                    radius = cr.BottomRight * scale;
                }
                if (cr.BottomLeft > 0) {
                    corner_mask |= UIRectCorner.BottomLeft;
                    radius = cr.BottomLeft * scale;
                }
                this.ClipsToBounds = true;
                this.Layer.CornerRadius = (nfloat)radius;
                this.Layer.MaskedCorners = (CACornerMask)corner_mask;
            } else {
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
    }
}