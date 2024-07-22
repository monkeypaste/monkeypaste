using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;

namespace iosKeyboardTest.Android {
    public class AdKeyboardView : TextureView, TextureView.ISurfaceTextureListener {
        #region Private Variables
        private KeyboardRenderer mThread;
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region View Models
        public KeyboardViewModel DC { get; set; }
        public KeyboardRenderer Renderer { get; private set; }
        #endregion

        #region Appearance

        #endregion

        #region Layout
        #endregion

        #region State
        #endregion

        #region Models
        #endregion

        #endregion

        #region Events
        #endregion

        #region Constructors
        public AdKeyboardView(Context context, IKeyboardInputConnection conn) : base(context) {
            Focusable = false;
            
            if(conn is IOnTouchListener otl) {
                this.SetOnTouchListener(otl);
            }
            var kb_size = KeyboardViewModel.GetTotalSizeByScreenSize(AndroidDisplayInfo.ScaledSize, conn.Flags.HasFlag(KeyboardFlags.Portrait));
            DC = new KeyboardViewModel(conn, kb_size, AndroidDisplayInfo.Scaling);

            int w = (int)(DC.TotalWidth * AndroidDisplayInfo.Scaling);
            int h = (int)(DC.TotalHeight * AndroidDisplayInfo.Scaling);
            this.LayoutParameters = new LinearLayout.LayoutParams(w, h);

            this.SurfaceTextureListener = this;
        }

        public AdKeyboardView(Context context, IAttributeSet attrs) : base(context, attrs) {
            this.SurfaceTextureListener = this;
        }

        public AdKeyboardView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr) {
            this.SurfaceTextureListener = this;
        }

        public AdKeyboardView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes) {
            this.SurfaceTextureListener = this;
        }

        protected AdKeyboardView(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) {
            this.SurfaceTextureListener = this;
        }
        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec) {
            int w = (int)(DC.TotalWidth * AndroidDisplayInfo.Scaling);
            int h = (int)(DC.TotalHeight * AndroidDisplayInfo.Scaling) + 20;
            base.OnMeasure(MeasureSpec.MakeMeasureSpec(w, MeasureSpecMode.Exactly), MeasureSpec.MakeMeasureSpec(h, MeasureSpecMode.Exactly));
        }
        #endregion

        #region Public Methods

        #endregion

        #region Protected Methods


        #endregion

        #region Private Methods

        private Paint SetupPaint() {
            var paint = new Paint();
            paint.AntiAlias = true;
            paint.SetTypeface(Resources.GetFont(Resource.Font.Nunito_Regular));
            return paint;
        }


        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height) {
            Renderer = new KeyboardRenderer(this,SetupPaint(),DC);
            mThread = Renderer;
            mThread.Start();
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface) {
            if (mThread != null) {
                mThread.StopRendering();
            }
            return true;
        }

        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height) {
            //throw new NotImplementedException();
        }

        public void OnSurfaceTextureUpdated(SurfaceTexture surface) {
            //throw new NotImplementedException();
        }
        #endregion

        #region Commands
        #endregion

    }
    public class KeyboardContainer : ViewGroup {
        public KeyboardViewModel DC { get; set; }
        public KeyboardContainer(Context context, IKeyboardInputConnection conn) : base(context) {
            Focusable = false;

            var kb_size = KeyboardViewModel.GetTotalSizeByScreenSize(AndroidDisplayInfo.ScaledSize, conn.Flags.HasFlag(KeyboardFlags.Portrait));
            DC = new KeyboardViewModel(conn, kb_size, AndroidDisplayInfo.Scaling);

            int w = (int)(DC.TotalWidth * AndroidDisplayInfo.Scaling);
            int h = (int)(DC.TotalHeight * AndroidDisplayInfo.Scaling);
            this.LayoutParameters = new LinearLayout.LayoutParams(w, h);

            //this.AddView(new AdKeyboardView(context, DC));
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec) {
            int w = (int)(DC.TotalWidth * AndroidDisplayInfo.Scaling);
            int h = (int)(DC.TotalHeight * AndroidDisplayInfo.Scaling) + 20;
            base.OnMeasure(MeasureSpec.MakeMeasureSpec(w, MeasureSpecMode.Exactly), MeasureSpec.MakeMeasureSpec(h, MeasureSpecMode.Exactly));
        }

        public event EventHandler<iosKeyboardTest.Android.TouchEventArgs> OnMotionEvent;
        public override bool OnTouchEvent(MotionEvent e) {
            double x = e.GetX() / AndroidDisplayInfo.Scaling;
            double y = e.GetY() / AndroidDisplayInfo.Scaling;
            Avalonia.Point p = new Avalonia.Point(x, y);
            var tet =
                e.Action == MotionEventActions.Down ?
                    TouchEventType.Press :
                    e.Action == MotionEventActions.Move ?
                        TouchEventType.Move :
                        e.Action == MotionEventActions.Up ?
                            TouchEventType.Release :
                            TouchEventType.None;
            OnMotionEvent?.Invoke(this, new iosKeyboardTest.Android.TouchEventArgs(new Avalonia.Point(x, y), tet));
            return true;
        }
        protected override void OnLayout(bool changed, int l, int t, int r, int b) {
            //throw new NotImplementedException();
        }
    }
}
