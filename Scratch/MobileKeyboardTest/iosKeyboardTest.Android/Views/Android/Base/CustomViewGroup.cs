using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;

namespace iosKeyboardTest.Android {
    public interface IFrameView {
        RectF Frame { get; }
    }
    public class CustomViewGroup : FrameLayout, IKeyboardViewRenderer, IFrameView {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        public Paint SharedPaint { get; set; } = new();
        #endregion

        #region Interfaces
        #region IKeyboardViewRenderer Implementation

        public virtual void Layout(bool invalidate) {
            for (int i = 0; i < ChildCount; i++) {
                if(this.GetChildAt(i) is IKeyboardViewRenderer ckbvr) {
                    ckbvr.Layout(invalidate);
                }
            }
            if(invalidate) {
                this.Redraw();
            }
        }

        public virtual void Measure(bool invalidate) {
            for (int i = 0; i < ChildCount; i++) {
                if (this.GetChildAt(i) is IKeyboardViewRenderer ckbvr) {
                    ckbvr.Measure(invalidate);
                }
            }
            if (invalidate) {
                this.Redraw();
            }
        }

        public virtual void Paint(bool invalidate) {
            for (int i = 0; i < ChildCount; i++) {
                if (this.GetChildAt(i) is IKeyboardViewRenderer ckbvr) {
                    ckbvr.Paint(invalidate);
                }
            }
            if (invalidate) {
                this.Redraw();
            }
        }

        public virtual void Render(bool invalidate) {
            Layout(false);
            Measure(false);
            Paint(false);

            for (int i = 0; i < ChildCount; i++) {
                if (this.GetChildAt(i) is IKeyboardViewRenderer ckbvr) {
                    ckbvr.Render(invalidate);
                } else if(invalidate) {
                    this.GetChildAt(i).Redraw();
                }
            }
            if (invalidate) {
                this.Redraw();
            }
        }
        #endregion
        #endregion

        #region Properties

        public Color BackgroundColor { get; set; }
        public virtual RectF Frame { get; set; }
        #endregion
        #region Constructors

        public CustomViewGroup(Context context) : this(context, null) { }
        public CustomViewGroup(Context context, Paint paint) : base(context) {
            SharedPaint = paint;
            this.SetWillNotDraw(false);
        }
        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec) {
            SetMeasuredDimension((int)Frame.Width(), (int)Frame.Height());

        }
        protected override void OnLayout(bool changed, int l, int t, int r, int b) {
            for (int i = 0; i < ChildCount; i++) {
                var child = GetChildAt(i);
                if (child is IFrameView fv) {
                    var frame = fv.Frame ?? new();
                    child.Layout((int)frame.Left, (int)frame.Top, (int)frame.Right, (int)frame.Bottom);
                }

            }
        }

        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}