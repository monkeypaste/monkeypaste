using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;

namespace iosKeyboardTest.Android {
    public class CursorControlView : CustomView, IKeyboardViewRenderer {
        #region ctors
        public CursorControlView(Context context, Paint paint) : base(context, paint) {
        }

        public CursorControlView(Context context, IAttributeSet attrs) : base(context, attrs) {
        }

        public CursorControlView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr) {
        }

        public CursorControlView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes) {
        }

        protected CursorControlView(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) {
        }

        public void Layout(bool invalidate) {
            throw new System.NotImplementedException();
        }

        public void Measure(bool invalidate) {
            throw new System.NotImplementedException();
        }

        public void Paint(bool invalidate) {
            throw new System.NotImplementedException();
        }

        public void Render(bool invalidate) {
            throw new System.NotImplementedException();
        }
        #endregion

    }
}