using Android.Content;
using Android.Graphics;
using Android.Widget;

namespace iosKeyboardTest.Android {
    public interface IFrameView {
        RectF Frame { get; }
    }
    public class CustomViewGroup : FrameLayout, IFrameView {
        Paint SharedPaint { get; set; }
        public Color BackgroundColor { get; set; }
        public RectF Frame { get; set; }
        public CustomViewGroup(Context context, Paint paint) : base(context) {
            SharedPaint = paint;
            this.SetWillNotDraw(false);
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec) {
            //base.OnMeasure(MeasureSpec.MakeMeasureSpec((int)Frame.Width(), MeasureSpecMode.Exactly), MeasureSpec.MakeMeasureSpec((int)Frame.Height(), MeasureSpecMode.Exactly));
            SetMeasuredDimension((int)Frame.Width(), (int)Frame.Height());

        }
        protected override void OnLayout(bool changed, int l, int t, int r, int b) {
            //base.OnLayout(changed, Frame.Left, Frame.Top, Frame.Right, Frame.Bottom);
            for (int i = 0; i < ChildCount; i++) {
                var child = GetChildAt(i);
                if (child is IFrameView fv) {
                    var frame = fv.Frame ?? new();

                    child.Layout((int)frame.Left, (int)frame.Top, (int)frame.Right, (int)frame.Bottom);
                }

            }
        }        
        protected override void DispatchDraw(Canvas canvas) {
            base.DispatchDraw(canvas);
            if(SharedPaint == null) {
                return;
            }
            SharedPaint.Color = BackgroundColor;
            //canvas.DrawRect(new RectF(0f,0f,Frame.Width(),Frame.Height()), SharedPaint);
            //for (int i = 0; i < ChildCount; i++) {
            //    var child = GetChildAt(i);
            //    child.Draw(canvas);
            //}
        }
    }
}