using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;
using Color = Android.Graphics.Color;
using Size = Android.Util.Size;

namespace iosKeyboardTest.Android {
    public static class AdHelpers {
        static float s =>
            (float)AndroidDisplayInfo.Scaling;
        public static RectF ToRectF(this Avalonia.Rect av_rect) {
            return new RectF((float)av_rect.Left*s, (float)av_rect.Top * s, (float)av_rect.Right * s, (float)av_rect.Bottom * s);
        }
        public static Rect ToRect(this Avalonia.Rect av_rect) {
            return new Rect((int)av_rect.Left * (int)s, (int)av_rect.Top * (int)s, (int)av_rect.Right * (int)s, (int)av_rect.Bottom * (int)s);
        }
        public static Rect ToRect(this RectF rectf) {
            return new Rect((int)rectf.Left, (int)rectf.Top, (int)rectf.Right, (int)rectf.Bottom);
        }
        public static RectF ToRectF(this Rect rectf) {
            return new RectF((float)rectf.Left, (float)rectf.Top, (float)rectf.Right, (float)rectf.Bottom);
        }
        public static RectF Move(this RectF rect, float ox, float oy) {
            float w = rect.Width();
            float h = rect.Height();
            float l = ox;
            float t = oy;
            float r = l + w;
            float b = t + h;
            return new RectF(l, t, r, b);
        }
        public static RectF Push(this RectF rect, float dx, float dy) {
            float w = rect.Width();
            float h = rect.Height();
            float l = rect.Left + dx;
            float t = rect.Top + dy;
            float r = l + w;
            float b = t + h;
            return new RectF(l, t, r, b);
        }
        public static RectF ToBounds(this RectF rect) {
            return rect.Move(0, 0);
        }
        public static Size GetSize(this Rect rect) {
            return new Size(rect.Width(), rect.Height());
        }
        public static Avalonia.Point GetMotionPoint(this MotionEvent e, Dictionary<int,Avalonia.Point> touches) {
            Avalonia.Point motion_p = default;
            var to_remove_tidl = touches.Select(x => x.Key).ToList();
            for (int i = 0; i < e.PointerCount; i++) {
                int tid = e.GetPointerId(i);
                var idx_p = new Avalonia.Point(e.GetX(i),e.GetY(i));
                if(touches.TryGetValue(tid, out var tp)) {
                    if (touches[tid] != idx_p) {
                        if(motion_p != default) {
                            //Debugger.Break();
                        }
                        motion_p = idx_p;
                    }
                    touches[tid] = idx_p;
                    to_remove_tidl.Remove(tid);
                } else {
                    if (motion_p != default) {
                        //Debugger.Break();
                    }
                    motion_p = idx_p;
                    touches.Add(tid, idx_p);
                }
            }
            foreach(var to_remove_tid in to_remove_tidl) {

                if (motion_p != default) {
                    //Debugger.Break();
                }
                motion_p = touches[to_remove_tid];
                touches.Remove(to_remove_tid);
            }
            return motion_p;
        }
        public static bool IsDown(this MotionEventActions met) {
            return
                met == MotionEventActions.Down ||
                met == MotionEventActions.Pointer1Down ||
                met == MotionEventActions.Pointer2Down ||
                met == MotionEventActions.ButtonPress;
        }
        public static bool IsMove(this MotionEventActions met) {
            return
                met == MotionEventActions.Move;
        }
        public static bool IsUp(this MotionEventActions met) {
            return
                met == MotionEventActions.Up ||
                met == MotionEventActions.Pointer1Up ||
                met == MotionEventActions.Pointer2Up ||
                met == MotionEventActions.ButtonRelease;
        }
        public static void FitTextToSize(this Paint paint, Size constraintSize, char[] text, float max = 96) {
            if(text == null || text.Length == 0) {
                return;
            }

            paint.TextSize = 8f;
            float ts_step = 0.5f;
            Rect tr = new Rect();
            while(true) {
                paint.TextSize += ts_step;
                if(paint.TextSize >= max) {
                    return;
                }
                paint.GetTextBounds(text, 0, text.Length, tr);
                int w = tr.Width();
                if (w > constraintSize.Width) {
                    // outside constraint
                    paint.TextSize -= ts_step;
                    return;
                }
            }
        }
        public static byte[] ImageDataFromResource(string r) {
            // Ensure "this" is an object that is part of your implementation within your Xamarin forms project
            var assembly = typeof(AdHelpers).GetTypeInfo().Assembly;
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