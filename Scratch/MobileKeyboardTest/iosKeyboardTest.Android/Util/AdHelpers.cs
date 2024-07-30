using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Android.Widget;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Java.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using static Android.Icu.Text.CaseMap;
using static System.Net.Mime.MediaTypeNames;
using Bitmap = Android.Graphics.Bitmap;
using Color = Android.Graphics.Color;
using PointF = Android.Graphics.PointF;
using Size = Android.Util.Size;

namespace iosKeyboardTest.Android {
    public static class AdHelpers {
        static float s =>
            (float)AndroidDisplayInfo.Scaling;

        #region Geometery
        public static float[] ToCornerArray(this Avalonia.CornerRadius cr) {
            return new double[]{
                        cr.TopLeft, cr.TopLeft,        // Top, left in px
                        cr.TopRight, cr.TopRight,        // Top, right in px
                        cr.BottomRight, cr.BottomRight,          // Bottom, right in px
                        cr.BottomLeft, cr.BottomLeft           // Bottom,left in px
                    }.Select(x => x.UnscaledF()).ToArray();
        }
        public static RectF ToRectF(this Avalonia.Rect av_rect) {
            return new RectF((float)av_rect.Left * s, (float)av_rect.Top * s, (float)av_rect.Right * s, (float)av_rect.Bottom * s);
        }
        public static Rect ToRect(this Avalonia.Rect av_rect) {
            return new Rect((int)av_rect.Left * (int)s, (int)av_rect.Top * (int)s, (int)av_rect.Right * (int)s, (int)av_rect.Bottom * (int)s);
        }
        public static Rect ToRect(this RectF rectf) {
            return new Rect((int)rectf.Left, (int)rectf.Top, (int)rectf.Right, (int)rectf.Bottom);
        }
        public static PointF Location(this RectF rectf) {
            return new PointF(rectf.Left, rectf.Top);
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
        public static Rect Move(this Rect rect, int ox, int oy) {
            int w = rect.Width();
            int h = rect.Height();
            int l = ox;
            int t = oy;
            int r = l + w;
            int b = t + h;
            return new Rect(l, t, r, b);
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
        public static RectF ToBounds(this RectF rect, RectF outer_rect) {
            float w = rect.Width();
            float h = rect.Height();
            float l = rect.Left - outer_rect.Left;
            float t = rect.Top - outer_rect.Top;
            float r = l + w;
            float b = t + h;
            return new RectF(l, t, r, b);
        }
        public static Rect ToBounds(this Rect rect) {
            return rect.Move(0, 0);
        }
        public static Size GetSize(this Rect rect) {
            return new Size(rect.Width(), rect.Height());
        }

        public static float UnscaledF(this double d) {
            return (float)(d * KeyboardView.Scaling);
        }
        public static int UnscaledI(this double d) {
            return (int)(d * KeyboardView.Scaling);
        }
        
        public static double ScaledD(this float f) {
            return (double)((double)f / (double)KeyboardView.Scaling);
        }
        public static double ScaledD(this int i) {
            return (double)((double)i / (double)KeyboardView.Scaling);
        }
        public static int ScaledI(this int i) {
            return (int)(i / KeyboardView.Scaling);
        }

        #endregion

        #region Motion Event
        public static int GetTouchIdx(this MotionEventActions met) {
            // returns -1 if not release
            switch (met) {
                case MotionEventActions.Down:
                case MotionEventActions.Up:
                case MotionEventActions.Pointer1Down:
                case MotionEventActions.Pointer1Up:
                    return 0;
                case MotionEventActions.Pointer2Down:
                case MotionEventActions.Pointer2Up:
                    return 1;
                case MotionEventActions.Pointer3Down:
                case MotionEventActions.Pointer3Up:
                    return 2;
                default:
                    return -1;
            }
        }
        public static IEnumerable<(int id,Avalonia.Point loc,TouchEventType eventType)> GetMotions(this MotionEvent e, Dictionary<int, Avalonia.Point> touches) {
            Debug.WriteLine("");
            Debug.WriteLine($"{e.ActionMasked}");
            List<(int, Avalonia.Point, TouchEventType)> changed_ids = [];
            for (int i = 0; i < e.PointerCount; i++) {
                int tid = e.GetPointerId(i);
                var tid_p = new Avalonia.Point(e.GetX(i), e.GetY(i));
                if(e.ActionMasked == MotionEventActions.Move) {
                    if (touches[tid] == tid_p) {
                        continue;
                    }

                    touches[tid] = tid_p;
                    changed_ids.Add((tid, tid_p, TouchEventType.Move));
                } else {
                    if(i == e.ActionIndex) {
                        if(e.ActionMasked.IsDown()) {
                            // new touch
                            touches.Add(tid, tid_p);
                            changed_ids.Add((tid, tid_p, TouchEventType.Press));
                        } else if(e.ActionMasked.IsUp()) {
                            // old touch
                            changed_ids.Add((tid, tid_p, TouchEventType.Release));
                            touches.Remove(tid);
                        }
                    }
                }
            }
            foreach(var ch in changed_ids) {
                Debug.WriteLine($"Type: {ch.Item3} Id: {ch.Item1} Loc: {ch.Item2}");
            }
            return changed_ids;
        }

        public static bool IsDown(this MotionEventActions met) {
            return
                met == MotionEventActions.Down ||
                met == MotionEventActions.PointerDown ||
                met == MotionEventActions.Pointer1Down ||
                met == MotionEventActions.Pointer2Down ||
                met == MotionEventActions.Pointer3Down ||
                met == MotionEventActions.ButtonPress;
        }
        public static bool IsMove(this MotionEventActions met) {
            return
                met == MotionEventActions.Move;
        }
        public static bool IsUp(this MotionEventActions met) {
            return
                met == MotionEventActions.Up ||
                met == MotionEventActions.PointerUp ||
                met == MotionEventActions.Pointer1Up ||
                met == MotionEventActions.Pointer2Up ||
                met == MotionEventActions.Pointer3Up ||
                met == MotionEventActions.ButtonRelease;
        }
        #endregion

        #region Images

        public static Bitmap Scale(this Bitmap bmp, int newWidth,int newHeight, bool recycle = true) {
            //from https://stackoverflow.com/a/10703256/105028
            int w = Math.Max(1, bmp.Width);
            int h = Math.Max(1, bmp.Height);
            float scaled_w = ((float)Math.Max(1,newWidth)) / w;
            float scaled_h = ((float)Math.Max(1,newHeight)) / h;
            Matrix matrix = new Matrix();
            matrix.PostScale(scaled_w, scaled_h);
            var scaled_bmp = Bitmap.CreateBitmap(bmp, 0, 0, w, h, matrix, false);
            if(recycle) {
                bmp.Recycle();
            }
            return scaled_bmp;
        }
        public static Bitmap ToBitmap(this Drawable d) {
            // from https://stackoverflow.com/a/10600736/105028
            if(d is BitmapDrawable bd && bd.Bitmap is { } bdbmp) {
                return bdbmp;
            }

            Bitmap bmp = null;
            if(d.IntrinsicWidth <= 0 || d.IntrinsicHeight <= 0) {
                bmp = Bitmap.CreateBitmap(1, 1, Bitmap.Config.Argb8888);
            } else {
                bmp = Bitmap.CreateBitmap(d.IntrinsicWidth, d.IntrinsicHeight, Bitmap.Config.Argb8888);
            }
            Canvas canvas = new Canvas(bmp);
            d.SetBounds(0, 0, canvas.Width, canvas.Height);
            d.Draw(canvas);
            return bmp;
        }
        public static Bitmap TintBitmap(this Bitmap bmp, int color) {
            // from https://stackoverflow.com/a/4856229/105028

            var result = Bitmap.CreateBitmap(bmp.Width, bmp.Height, bmp.GetConfig());
            var canvas = new Canvas(result);
            var paint = new Paint();
            paint.SetColorFilter(new LightingColorFilter(color, 0));
            canvas.DrawBitmap(bmp, 0, 0, paint);
            return result;
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
        #endregion

        #region Views

        static event EventHandler<DialogClickEventArgs> alertHandler;
        public static void Alert(this string msg, Context context,string title = "") {
            void OnAlertClick(object sender, DialogClickEventArgs e) {
                alertHandler -= OnAlertClick;
                if(sender is AlertDialog ad) {
                    ad.Dismiss();
                }
            }
            alertHandler += OnAlertClick;
            new AlertDialog.Builder(context)
                .SetTitle(title)
                .SetMessage(msg)
                .SetPositiveButton("OK", alertHandler)
                .Show();
        }

        public static Size TextSize(this TextView tv) {
            // from https://stackoverflow.com/a/24359594/105028
            Rect bounds = new Rect();
            Paint textPaint = tv.Paint;
            textPaint.GetTextBounds(tv.Text, 0, tv.Text.Length, bounds);
            return new Size(bounds.Width(), bounds.Height());
        }
        public static void Translate(this View v, float tx, float ty) {
            //v.Transform = CGAffineTransform.Translate(v.Transform, tx, ty);
        }
        public static void Redraw(this View v) {
            v.Invalidate();
        }
        public static T SetDefaultProps<T>(this T uiv, string name = default) where T : View {
            uiv.Focusable = false;
            uiv.ClipToOutline = false;

            if (uiv is CustomView cv && !string.IsNullOrEmpty(name)) {
                cv.Name = name;
            }
            if (uiv is ViewGroup vg) {
                vg.SetClipChildren(false);
            }
            return uiv;
        }
        public static T SetDefaultTextProps<T>(this T uitv) where T : TextView {
            uitv = uitv.SetDefaultProps();
            uitv.SetMaxLines(1);
            //uitv.Selectable = false;
            //uitv.BackgroundColor = Color.FromRGBA(0, 0, 0, 0);
            //uitv.TextContainer.LineBreakMode = UILineBreakMode.TailTruncation;
            //uitv.TextContainer.MaximumNumberOfLines = 1;
            return uitv;
        }
        #endregion

        #region Color
        public static void SetTint(this Paint paint, Color? color) {
            if(color is { } c) {
                paint.Color = c;
                paint.SetColorFilter(new PorterDuffColorFilter(c, PorterDuff.Mode.SrcIn));
            } else {
                paint.SetColorFilter(null);
            }
        }
        public static int ToInt(this Color c) {
            return (int)c;
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
        public static Color ToAdColor(this SolidColorBrush scb) {
            return new(scb.Color.R, scb.Color.G, scb.Color.B, scb.Color.A);
        }
        #endregion
    }

}