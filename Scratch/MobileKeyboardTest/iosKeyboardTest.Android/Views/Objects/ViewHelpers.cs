using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.Drawing;
using System.Reflection;
using Color = Android.Graphics.Color;
using Size = Android.Util.Size;

namespace iosKeyboardTest.Android {
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
        public static Color ToColor(this string hex) {
            System.Drawing.Color color = ColorTranslator.FromHtml(hex);
            //return Color.FromRGBA(color.A, color.R, color.G, color.B);
            return Color.Argb(color.A, color.R, color.G, color.B);
        }
        public static void Translate(this View v,float tx,float ty) {
            //v.Transform = CGAffineTransform.Translate(v.Transform, tx, ty);
        }
        public static Size TextSize(this TextView tv) {
            //var attr = new NSAttributedString(tv.Text, tv.Font);
            //return attr.Size;
            return default;
        }
        public static void Redraw(this View v) {
            //v.Layer.SetNeedsDisplay();
            //v.Layer.DisplayIfNeeded();
        }
        public static T SetDefaultProps<T>(this T uiv) where T: View {
            //uiv.TranslatesAutoresizingMaskIntoConstraints = false;
            //uiv.UserInteractionEnabled = false;
            //uiv.ClipsToBounds = false;
            return uiv;
        }
        public static T SetDefaultTextProps<T>(this T uitv) where T: TextView{
            uitv = uitv.SetDefaultProps();
            //uitv.Selectable = false;
            //uitv.BackgroundColor = Color.FromRGBA(0, 0, 0, 0);
            //uitv.TextContainer.LineBreakMode = UILineBreakMode.TailTruncation;
            //uitv.TextContainer.MaximumNumberOfLines = 1;
            return uitv;
        }
    }
}