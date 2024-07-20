using Avalonia;
using CoreAnimation;
using CoreGraphics;
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

            double text_pad = 5;
            double pfs = DC.PrimaryFontSize * UIScreen.MainScreen.Scale;
            double pw = pfs * DC.PrimaryValue.Length + text_pad;
            double ph = pfs + text_pad;

            double px = (w / 2) - (PrimaryTextView.Frame.Width / 2);
            double py = (h / 2) - (PrimaryTextView.Frame.Height / 2);
            PrimaryTextView.Frame = new CGRect(px, py, PrimaryTextView.Frame.Width, PrimaryTextView.Frame.Height);
            if (PrimaryTextView.Font == null || PrimaryTextView.Font.PointSize != pfs) {
                PrimaryTextView.Font = UIFont.SystemFontOfSize((nfloat)pfs, UIFontWeight.Regular);
            }

            double sec_right_margin = 5;
            double sx = w - SecondaryTextView.Frame.Width - sec_right_margin;
            double sy = 0;
            SecondaryTextView.Frame = new CGRect(sx, sy, SecondaryTextView.Frame.Width, SecondaryTextView.Frame.Height);
            double sfs = DC.SecondaryFontSize * UIScreen.MainScreen.Scale;
            if (SecondaryTextView.Font == null || SecondaryTextView.Font.PointSize != sfs) {
                SecondaryTextView.Font = UIFont.SystemFontOfSize((nfloat)sfs, UIFontWeight.Regular);
            }

            if (invalidate) {
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
            

            /*
            func roundCorners(_ corners: UIRectCorner, radius: CGFloat) {
        if #available(iOS 11.0, *) {
            clipsToBounds = true
            layer.cornerRadius = radius
            layer.maskedCorners = CACornerMask(rawValue: corners.rawValue)
        } else {
            let path = UIBezierPath(
                roundedRect: bounds, 
                byRoundingCorners: corners, 
                cornerRadii: CGSize(width: radius, height: radius)
            )
            let mask = CAShapeLayer()
            mask.path = path.cgPath
            layer.mask = mask
        }
    }
            */
            if(UIDevice.CurrentDevice.CheckSystemVersion(11,0)) {
                // from https://stackoverflow.com/a/71329483/105028
                UIRectCorner corner_mask = (UIRectCorner)0;
                double radius = 0;
                if (cr.TopLeft > 0) {
                    corner_mask |= UIRectCorner.TopLeft;
                    radius = cr.TopLeft;
                }
                if (cr.TopRight > 0) {
                    corner_mask |= UIRectCorner.TopRight;
                    radius = cr.TopRight;
                }
                if (cr.BottomRight > 0) {
                    corner_mask |= UIRectCorner.BottomRight;
                    radius = cr.BottomRight;
                }
                if (cr.BottomLeft > 0) {
                    corner_mask |= UIRectCorner.BottomLeft;
                    radius = cr.BottomLeft;
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