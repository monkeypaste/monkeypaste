using CoreFoundation;
using CoreGraphics;
using Foundation;
using System;
using System.Drawing;
using System.Reflection;
using UIKit;

namespace iosKeyboardTest.iOS.KeyboardExt {
    public static class ViewHelpers {
        public static byte[] ImageDataFromResource(string r) {
            // Ensure "this" is an object that is part of your implementation within your Xamarin forms project
            var assembly = typeof(ViewHelpers).GetTypeInfo().Assembly;
            byte[] buffer = null;

            using (System.IO.Stream s = assembly.GetManifestResourceStream(r)) {
                if (s != null) {
                    long length = s.Length;
                    buffer = new byte[length];
                    s.Read(buffer, 0, (int)length);
                }
            }

            return buffer;
        }
        public static UIColor ToUIColor(this string hex) {
            Color color = ColorTranslator.FromHtml(hex);
            //return UIColor.FromRGBA(color.A, color.R, color.G, color.B);
            return UIColor.FromRGBA(color.R, color.G, color.B, color.A);
        }
        public static void Translate(this UIView v,nfloat tx,nfloat ty) {
            v.Transform = CGAffineTransform.Translate(v.Transform, tx, ty);
        }
        public static CGSize TextSize(this UITextView tv) {
            var attr = new NSAttributedString(tv.Text, tv.Font);
            return attr.Size;
        }
        public static void Redraw(this UIView v) {
            v.Layer.SetNeedsDisplay();
            v.Layer.DisplayIfNeeded();
        }
        public static T SetDefaultProps<T>(this T uiv) where T: UIView {
            uiv.TranslatesAutoresizingMaskIntoConstraints = false;
            uiv.UserInteractionEnabled = false;
            uiv.ClipsToBounds = false;
            return uiv;
        }
        public static T SetDefaultTextProps<T>(this T uitv) where T: UITextView{
            uitv = uitv.SetDefaultProps();
            uitv.Selectable = false;
            uitv.BackgroundColor = UIColor.FromRGBA(0, 0, 0, 0);
            uitv.TextContainer.LineBreakMode = UILineBreakMode.TailTruncation;
            uitv.TextContainer.MaximumNumberOfLines = 1;
            return uitv;
        }
    }
}