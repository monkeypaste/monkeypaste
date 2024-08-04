using Android.Content;
using Android.Graphics;
using Android.Text;
using Android.Views;
using Android.Widget;
using System.Linq;
using GPaint = Android.Graphics.Paint;

namespace iosKeyboardTest.Android {
    public class CustomTextView : TextView, IFrameView {
        GPaint SharedPaint { get; set; }
        public RectF Frame { get; set; } = new();
        public Color ForegroundColor { get; set; }
        public Color BackgroundColor { get; set; }
        public CustomTextView(Context context, GPaint paint) : base(context) {
            SharedPaint = paint;
            this.SetBackgroundColor(Color.Argb(0, 0, 0, 0));

        }
        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec) {
            SetMeasuredDimension((int)Frame.Width(), (int)Frame.Height());
        }

        protected override void OnDraw(Canvas canvas) {
            if(Visibility == ViewStates.Invisible) {
                return;
            }

            var rect = new RectF(0, 0, Frame.Width(), Frame.Height());
            SharedPaint.Color = BackgroundColor;
            canvas.DrawRect(rect, SharedPaint);

            SharedPaint.Color = ForegroundColor;
            SharedPaint.TextAlign = GPaint.Align.Left;
            SharedPaint.TextSize = this.TextSize;
            canvas.DrawText(this.Text, rect.Left, rect.Bottom, SharedPaint);            
        }
    }
}