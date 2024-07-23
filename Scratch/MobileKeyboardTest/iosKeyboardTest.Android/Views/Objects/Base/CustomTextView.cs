using Android.Content;
using Android.Graphics;
using Android.Text;
using Android.Views;
using Android.Widget;
using System.Linq;
using GPaint = Android.Graphics.Paint;

namespace iosKeyboardTest.Android {
    public class CustomTextView : TextView, IFrameView {
        StaticLayout CustomStaticLayout { get; set; }
        GPaint SharedPaint { get; set; }
        IFrameView ParentFrame { get; set; }
        public RectF Frame { get; set; } = new();
        public Color CustomTextColor { get; set; }

        //public new string Text {
        //    get => base.Text;
        //    set {
        //        if(Text != value) {
        //            TextPaint textPaint = new TextPaint();
        //            textPaint.AntiAlias = true;
        //            textPaint.TextSize = this.TextSize;
        //            textPaint.Color = CustomTextColor;

        //            int width = (int)textPaint.MeasureText(this.Text);
        //            StaticLayout.Builder builder = StaticLayout.Builder.Obtain(this.Text, 0, Text.Length, textPaint, width)
        //               .SetAlignment(Layout.Alignment.AlignNormal)
        //               .SetLineSpacing(1, 0) // multiplier, add
        //               .SetIncludePad(false);
        //            CustomStaticLayout = builder.Build();
        //        }
        //    }
        //}
        public CustomTextView(Context context, GPaint paint) : base(context) {
            SharedPaint = paint;
            this.SetBackgroundColor(Color.Argb(0, 0, 0, 0));

        }
        public void SetParent(IFrameView parent) {
            ParentFrame = parent;
        }
        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec) {
            //base.OnMeasure(MeasureSpec.MakeMeasureSpec((int)Frame.Width(), MeasureSpecMode.Exactly), MeasureSpec.MakeMeasureSpec((int)Frame.Height(), MeasureSpecMode.Exactly));
            SetMeasuredDimension((int)Frame.Width(), (int)Frame.Height());
        }

        protected override void OnDraw(Canvas canvas) {
            if(Visibility == ViewStates.Invisible) {
                return;
            }
            SetBackgroundColor(Color.Cyan);
            canvas.Save();

            SharedPaint.Color = Color.Cyan;
            canvas.DrawRect(Frame, SharedPaint);
            canvas.Restore();

            canvas.Save();
            SharedPaint.Color = CustomTextColor;
            SharedPaint.TextAlign = GPaint.Align.Left;
            SharedPaint.TextSize = 24;// this.TextSize;
            canvas.DrawText(this.Text, Frame.Left,Frame.Top, SharedPaint);
            canvas.Restore();

            if(CustomStaticLayout != null) {
            //    CustomStaticLayout.Draw(canvas);
            }
            
        }
    }
}