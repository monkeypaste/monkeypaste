using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;
using Avalonia.Media;
using System;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;
using Color = Android.Graphics.Color;
using Size = Android.Util.Size;

namespace iosKeyboardTest.Android {
    public static class ViewHelpers {
        public static RectF ToRectF(this Avalonia.Rect av_rect) {
            return new RectF((float)av_rect.Left, (float)av_rect.Top, (float)av_rect.Right, (float)av_rect.Bottom);
        }
        public static Rect ToRect(this Avalonia.Rect av_rect) {
            return new Rect((int)av_rect.Left, (int)av_rect.Top, (int)av_rect.Right, (int)av_rect.Bottom);
        }
        public static Rect ToRect(this RectF rectf) {
            return new Rect((int)rectf.Left, (int)rectf.Top, (int)rectf.Right, (int)rectf.Bottom);
        }
        public static RectF ToRectF(this Rect rectf) {
            return new RectF((float)rectf.Left, (float)rectf.Top, (float)rectf.Right, (float)rectf.Bottom);
        }
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
        public static Color ToColor(this int c) {
            string hex = $"#{c.ToString("x8", CultureInfo.InvariantCulture).ToUpper()}";
            return hex.ToColor();
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
            // from https://stackoverflow.com/a/24359594/105028
            Rect bounds = new Rect();
            Paint textPaint = tv.Paint;
            textPaint.GetTextBounds(tv.Text, 0, tv.Text.Length, bounds);
            return new Size(bounds.Width(), bounds.Height());
        }
        public static void Redraw(this View v) {
            v.Invalidate();
        }
        public static T SetDefaultProps<T>(this T uiv, string name = default) where T: View {
            uiv.Focusable = false;
            uiv.ClipToOutline = false;
            
            if(uiv is CustomView cv && !string.IsNullOrEmpty(name)) {
                cv.Name = name;
            }
            if(uiv is ViewGroup vg) {
                vg.SetClipChildren(false);
            }
            return uiv;
        }
        public static T SetDefaultTextProps<T>(this T uitv) where T: TextView{
            uitv = uitv.SetDefaultProps();
            uitv.SetMaxLines(1);
            //uitv.Selectable = false;
            //uitv.BackgroundColor = Color.FromRGBA(0, 0, 0, 0);
            //uitv.TextContainer.LineBreakMode = UILineBreakMode.TailTruncation;
            //uitv.TextContainer.MaximumNumberOfLines = 1;
            return uitv;
        }

        public static Color ToAdColor(this SolidColorBrush scb) {
            return new(scb.Color.R, scb.Color.G, scb.Color.B, scb.Color.A);
        }
        public static float UnscaledF(this double d) {
            return (float)(d * KeyboardView.Scaling);
        }
        public static int UnscaledI(this double d) {
            return (int)(d * KeyboardView.Scaling);
        }
    }
}