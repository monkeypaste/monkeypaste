using Avalonia;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using HarfBuzzSharp;
using System;
using System.Linq;
using UIKit;

namespace iosKeyboardTest.iOS.KeyboardExt {
    
    public class KeyView : UIView, IKeyboardViewRenderer {
        UITextView SecondaryTextView { get; set; }
        UITextView PrimaryTextView { get; set; }
        UIImageView SpecialImageView { get; set; }
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
            PrimaryTextView.TextAlignment = UITextAlignment.Center;
            PrimaryTextView.SizeToFit();

            SecondaryTextView.Text = DC.SecondaryValue;
            SecondaryTextView.TextAlignment = UITextAlignment.Right;
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

            double pfs = DC.PrimaryFontSize;// * UIScreen.MainScreen.Scale;
            if (PrimaryTextView.Font == null || PrimaryTextView.Font.PointSize != pfs) {
                PrimaryTextView.Font = UIFont.SystemFontOfSize((nfloat)pfs, UIFontWeight.Regular);
            }
            PrimaryTextView.Frame = this.Bounds;

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
            
            TweakText();

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
            UIColor bg = KeyboardPalette.DefaultKeyBgHex.ToUIColor();
            UIColor fg = KeyboardPalette.FgHex.ToUIColor();
            if (DC.IsSpecial) {
                bg = DC.IsPressed ? KeyboardPalette.SpecialKeyPressedBgHex.ToUIColor() : KeyboardPalette.SpecialKeyBgHex.ToUIColor();
                if (DC.SpecialKeyType == SpecialKeyType.Shift) {
                    if (DC.IsShiftOn) {
                        fg = KeyboardPalette.ShiftHex.ToUIColor();
                    } else if (DC.IsShiftLock) {
                        bg = KeyboardPalette.ShiftHex.ToUIColor();
                    }
                } else if (DC.IsPrimarySpecial) {
                    bg = DC.IsPressed ? KeyboardPalette.PrimarySpecialKeyPressedBgHex.ToUIColor() : KeyboardPalette.PrimarySpecialKeyBgHex.ToUIColor();
                }
            } else if (DC.IsPopupKey) {
                bg = DC.IsActiveKey ? KeyboardPalette.HoldFocusBgHex.ToUIColor() : KeyboardPalette.HoldBgHex.ToUIColor();
                fg = KeyboardPalette.HoldFgHex.ToUIColor();
            } else {
                bg = DC.IsPressed ? KeyboardPalette.PressedBgHex.ToUIColor() : KeyboardPalette.DefaultKeyBgHex.ToUIColor();
            }
            BackgroundColor = bg;

            PrimaryTextView.TextColor = fg;

            if(DC.IsSecondaryVisible) {
                SecondaryTextView.Hidden = false;
                SecondaryTextView.TextColor = KeyboardPalette.FgHex2.ToUIColor();
            } else {
                SecondaryTextView.Hidden = true;
            }
            

            //if(invalidate) {
                PrimaryTextView.Redraw();
                SecondaryTextView.Redraw();
                this.Redraw();
            //}
        }


        void TweakText() {
            if (DC.IsNumber && !DC.IsPopupKey && !DC.Parent.IsNumbers) {
                // BUG numbers don't center naturally, trying use existing values no magic numbers
                // they need a nudge UP
                nfloat pty = -PrimaryTextView.Frame.Height / 8;
                PrimaryTextView.Translate(0, pty);

                // make numbers slightly smaller
                nfloat nfs = PrimaryTextView.Font.PointSize * 0.85f;
                PrimaryTextView.Font = UIFont.SystemFontOfSize(nfs);
            }
            if (DC.IsSpecial) {
                // BUG special text is too high, nudge DOWN

                nfloat pty = PrimaryTextView.Frame.Height / 8;
                PrimaryTextView.Translate(0, pty);
            }
            if(DC.IsPopupKey) {
                // BUG popup text isn't centering
                PrimaryTextView.Translate(this.Bounds.Width / 16, 0);
            }

            nfloat sty = -SecondaryTextView.Frame.Height / 4;
            SecondaryTextView.Translate(0, sty);
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