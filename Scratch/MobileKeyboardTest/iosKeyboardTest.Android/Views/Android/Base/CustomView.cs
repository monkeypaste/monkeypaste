using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Avalonia;
using System.Collections.Generic;
using System.Linq;

namespace iosKeyboardTest.Android {
    public class CustomView : View, IFrameView, IKeyboardViewRenderer {
        public Color BackgroundColor { get; set; }
        public Paint SharedPaint { get; set; }
        private RectF _frame = new();
        public RectF Frame {
            get => _frame;
            set {
                _frame = value;
                Bounds = _frame.ToBounds();
            }
        }
        public RectF Bounds { get; private set; } = new();
        public string Name { get; set; } = "Unnamed";

        #region ctors

        public CustomView(Context context, Paint paint) : base(context) {
            SharedPaint = paint;
            this.SetBackgroundColor(Color.Argb(0, 0, 0, 0));
        }

        public CustomView(Context context, IAttributeSet attrs) : base(context, attrs) {
        }

        public CustomView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr) {
        }

        public CustomView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes) {
        }

        protected CustomView(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) {
        }
        #endregion

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec) {
            SetMeasuredDimension((int)Frame.Width(), (int)Frame.Height());
        }

        public override string ToString() {
            return $"{Name} {Frame}";
        }

        public virtual void Layout(bool invalidate) {
            if (invalidate) {
                this.Redraw();
            }
        }

        public virtual void Measure(bool invalidate) {
            if (invalidate) {
                this.Redraw();
            }
        }

        public virtual void Paint(bool invalidate) {
            if (invalidate) {
                this.Redraw();
            }
        }

        public virtual void Render(bool invalidate) {
            Layout(false);
            Measure(false);
            Paint(false);

            if (invalidate) {
                this.Redraw();
            }
        }
    }
}