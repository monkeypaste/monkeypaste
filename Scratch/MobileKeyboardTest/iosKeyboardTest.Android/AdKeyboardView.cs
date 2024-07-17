using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using System;
using System.Linq;

namespace iosKeyboardTest.Android {
    public class RenderingThread : Thread {
        private Canvas canvas;
        private Paint paint;
        private TextureView mSurface;
        private KeyboardViewModel DC;
        private volatile bool mRunning = true;

        public RenderingThread(TextureView surface, Paint paint, KeyboardViewModel kbvm) {
            DC = kbvm;
            this.paint = paint;
            mSurface = surface;
            InitPalette(DC.IsThemeDark);
        }
        Color FgBrush { get; set; }
        Color FgBrush2 { get; set; }
        Color BgBrush { get; set; }
        Color HoldBgBrush { get; set; }
        Color HoldFocusBgBrush { get; set; }
        Color HoldFgBrush { get; set; }
        Color PressedBgBrush { get; set; }
        Color SpecialPressedBgBrush { get; set; }
        Color PrimarySpecialPressedBgBrush { get; set; }
        Color ShiftBrush { get; set; }
        Color MenuBgBrush { get; set; }
        Color CursorControlBgBrush { get; set; }
        Color CursorControlFgBrush { get; set; }
        Shader DefaultBgGradient { get; set; }
        Shader SpecialBgGradient { get; set; }
        Shader PrimarySpecialBgGradient { get; set; }

        void InitPalette(bool isDark) {
            if (isDark) {
                FgBrush = Color.White;
                FgBrush2 = Color.Silver;
                BgBrush = Color.Black;
                HoldBgBrush = Color.Gold;
                HoldFocusBgBrush = Color.Orange;
                HoldFgBrush = Color.Black;
                PressedBgBrush = Color.Gray;
                SpecialPressedBgBrush = Color.DimGray;
                PrimarySpecialPressedBgBrush = Color.MediumBlue;
                ShiftBrush = Color.Cyan;
                MenuBgBrush = Color.Rgb(51, 51, 51);
                CursorControlBgBrush = Color.Argb(150, 20, 20, 20);
                CursorControlFgBrush = Color.White;

                DefaultBgGradient = new LinearGradient(
                    DC.SpecialKeyWidth.UnscaledF() / 2,
                    0,
                    0,
                    DC.KeyHeight.UnscaledF(),
                    new int[] {
                        Color.Silver.ToArgb(),
                        Color.DimGray.ToArgb(),
                        Color.Rgb(68,68,68),
                        Color.Rgb(68,68,68)},
                    new float[] {
                        0f,
                        0.09f,
                        0.8f,
                        1f},
                        Shader.TileMode.Clamp);
                SpecialBgGradient = new LinearGradient(
                    DC.SpecialKeyWidth.UnscaledF() / 2,
                    0,
                    0,
                    DC.KeyHeight.UnscaledF(),
                    new int[] {
                        Color.DimGray,
                        Color.Rgb(51,51,51),
                        Color.Rgb(51,51,51),
                        Color.Rgb(34,34,34)},
                    new float[] {
                        0f,
                        0.08f,
                        0.8f,
                        1f},
                        Shader.TileMode.Clamp);
                PrimarySpecialBgGradient = new LinearGradient(
                    DC.SpecialKeyWidth.UnscaledF() / 2,
                    0,
                    0,
                    DC.KeyHeight.UnscaledF(),
                    new int[] {
                        Color.MediumBlue,
                        Color.Navy,
                        Color.MidnightBlue,
                        Color.DarkBlue},
                    new float[] {
                        0f,
                        0.08f,
                        0.8f,
                        1f},
                        Shader.TileMode.Clamp);
                return;
            }
            FgBrush = Color.Black;
            FgBrush2 = Color.DimGray;
            BgBrush = Color.White;
            HoldBgBrush = Color.LightGoldenrodYellow;
            HoldFocusBgBrush = Color.Khaki;
            HoldFgBrush = Color.Black;
            PressedBgBrush = Color.Gainsboro;
            SpecialPressedBgBrush = Color.MintCream;
            PrimarySpecialPressedBgBrush = Color.LightSkyBlue;
            ShiftBrush = Color.CornflowerBlue;
            MenuBgBrush = Color.Rgb(204, 204, 204);
            CursorControlBgBrush = Color.Argb(150, 255, 255, 255);
            CursorControlFgBrush = Color.Black;

            DefaultBgGradient = new LinearGradient(
                DC.SpecialKeyWidth.UnscaledF() / 2,
                0,
                0,
                DC.KeyHeight.UnscaledF(),
                new int[] {
                        Color.Rgb(85,85,85),
                        Color.Rgb(238,238,238),
                        Color.Rgb(220,220,220),
                        Color.Rgb(204,204,204),
                },
                new float[] {
                        0f,
                        0.09f,
                        0.8f,
                        1f},
                    Shader.TileMode.Clamp);
            SpecialBgGradient = new LinearGradient(
                DC.SpecialKeyWidth.UnscaledF() / 2,
                0,
                0,
                DC.KeyHeight.UnscaledF(),
                new int[] {
                        Color.Rgb(204,204,204),
                        Color.MintCream,
                        Color.MintCream,
                        Color.Rgb(238,238,238) },
                new float[] {
                        0f,
                        0.08f,
                        0.8f,
                        1f},
                    Shader.TileMode.Clamp);
            PrimarySpecialBgGradient = new LinearGradient(
                DC.SpecialKeyWidth.UnscaledF() / 2,
                0,
                0,
                DC.KeyHeight.UnscaledF(),
                new int[] {
                        Color.MediumBlue,
                        Color.LightSkyBlue,
                        Color.LightSkyBlue,
                        Color.DeepSkyBlue},
                new float[] {
                        0f,
                        0.08f,
                        0.8f,
                        1f},
                    Shader.TileMode.Clamp);
        }
        public override void Run() {
            //paint.setColor(0xff00ff00);

            while (mRunning && !Thread.Interrupted()) {
                Canvas canvas = mSurface.LockCanvas(null);
                try {
                    DrawKeyboard(canvas);
                }
                finally {
                    mSurface.UnlockCanvasAndPost(canvas);
                }

                try {
                    Thread.Sleep(15);
                }
                catch (InterruptedException e) {
                    // Interrupted
                }
            }
        }

        public void StopRendering() {
                Interrupt();
                mRunning = false;
        }
        void DrawKeyboard(Canvas canvas) {

            DrawBackground(canvas);
            DrawMenu(canvas);
            DrawKeys(canvas);
            DrawCursorControl(canvas);
        }
        void DrawBackground(Canvas canvas) {
            paint.Color = BgBrush;
            canvas.DrawRect(new Rect(0, 0, canvas.Width, canvas.Height), paint);
        }

        void DrawMenu(Canvas canvas) {
            int l = 0;
            int t = 0;
            int r = canvas.Width;
            int b = DC.MenuHeight.UnscaledI();
            Rect rectangle = new Rect(l, t, r, b);
            paint.Color = MenuBgBrush;
            canvas.DrawRect(rectangle, paint);
        }

        void DrawKeys(Canvas canvas) {
            foreach (var kvm in DC.Keys) {
                DrawKey(canvas,kvm);
            }
        }

        void DrawKey(Canvas canvas,KeyViewModel kvm) {
            if (!kvm.IsVisible) {
                return;
            }
            float x = kvm.X.UnscaledF() + (kvm.OuterPadX.UnscaledF() / 2);
            float y = kvm.Parent.MenuHeight.UnscaledF() + kvm.Y.UnscaledF() + (kvm.OuterPadY.UnscaledF() / 2);
            float w = kvm.InnerWidth.UnscaledF();
            float h = kvm.InnerHeight.UnscaledF();
            string pv = kvm.PrimaryValue;
            string sv = kvm.SecondaryValue;

            object bg = default;
            Color fg = FgBrush;
            if (kvm.IsSpecial) {
                bg = kvm.IsPressed ? SpecialPressedBgBrush : SpecialBgGradient;
                if (kvm.SpecialKeyType == SpecialKeyType.Shift) {
                    if (kvm.IsShiftOn) {
                        fg = ShiftBrush;
                    } else if (kvm.IsShiftLock) {
                        bg = ShiftBrush;
                    }
                } else if (kvm.IsPrimarySpecial) {
                    bg = kvm.IsPressed ? PrimarySpecialPressedBgBrush : PrimarySpecialBgGradient;
                }
            } else if (kvm.IsPopupKey) {
                bg = kvm.IsActiveKey ? HoldFocusBgBrush : HoldBgBrush;
                fg = HoldFgBrush;
            } else {
                bg = kvm.IsPressed ? PressedBgBrush : DefaultBgGradient;
            }

            var cr = kvm.CornerRadius;

            var corners = new double[]{
                cr.TopLeft, cr.TopLeft,        // Top, left in px
                cr.TopRight, cr.TopRight,        // Top, right in px
                cr.BottomRight, cr.BottomRight,          // Bottom, right in px
                cr.BottomLeft, cr.BottomLeft           // Bottom,left in px
            }.Select(x => x.UnscaledF()).ToArray();


            Path path = new Path();
            float l = x;
            float t = y;
            float r = x + w;
            float b = y + h;
            var rect = new RectF(l, t, r, b);
            if (bg is Color bgColor) {
                paint.Color = bgColor;
            } else if (bg is Shader shader) {
                paint.SetShader(shader);
            }

            path.AddRoundRect(rect, corners, Path.Direction.Cw);
            canvas.DrawPath(path, paint);
            paint.SetShader(null);

            if (!string.IsNullOrEmpty(kvm.PrimaryValue)) {
                paint.TextAlign = Paint.Align.Left;
                paint.TextSize = kvm.PrimaryFontSize.UnscaledF();
                var ptb = new Rect();
                paint.GetTextBounds(kvm.PrimaryValue.ToCharArray(), 0, kvm.PrimaryValue.Length, ptb);
                float px = rect.CenterX() + kvm.PrimaryTranslateOffsetX.UnscaledF() - ((float)ptb.Width() / 2f);
                float py = rect.CenterY() + kvm.PrimaryTranslateOffsetY.UnscaledF() + ((float)ptb.Height() / 2f);
                paint.Color = fg;
                canvas.DrawText(kvm.PrimaryValue, px, py, paint);
                canvas.DrawText(kvm.PrimaryValue, px, py, paint);
            }
            if (kvm.IsSecondaryVisible) {
                paint.TextAlign = Paint.Align.Left;
                paint.TextSize = kvm.SecondaryFontSize.UnscaledF();
                paint.Color = FgBrush2;

                var stb = new Rect();
                paint.GetTextBounds(kvm.SecondaryValue.ToCharArray(), 0, kvm.SecondaryValue.Length, stb);
                float sx = r + kvm.SecondaryTranslateOffsetX.UnscaledF() - ((float)stb.Width() / 2f);
                float sy = t + kvm.SecondaryTranslateOffsetY.UnscaledF() + ((float)stb.Height());
                canvas.DrawText(kvm.SecondaryValue, sx, sy, paint);
            }

        }

        void DrawCursorControl(Canvas canvas) {
            if (!DC.IsCursorControlEnabled) {
                return;
            }
        }
    }
public class AdKeyboardView : TextureView, TextureView.ISurfaceTextureListener {
        #region Private Variables
        private RenderingThread mThread;
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
        public event EventHandler<MotionEvent> OnMotionEvent;
        #endregion

        #region Constructors
        public AdKeyboardView(Context context, IKeyboardInputConnection conn) : base(context) {
            Focusable = false;

            var kb_size = KeyboardViewModel.GetTotalSizeByScreenSize(AndroidDisplayInfo.ScaledSize);
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
        #endregion

        #region Public Methods

        public override bool OnTouchEvent(MotionEvent e) {
            OnMotionEvent?.Invoke(this, e);
            return base.OnTouchEvent(e);
        }
        #endregion

        #region Protected Methods

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec) {
            int w = (int)(DC.TotalWidth * AndroidDisplayInfo.Scaling);
            int h = (int)(DC.TotalHeight * AndroidDisplayInfo.Scaling) + 20;
            base.OnMeasure(MeasureSpec.MakeMeasureSpec(w, MeasureSpecMode.Exactly), MeasureSpec.MakeMeasureSpec(h, MeasureSpecMode.Exactly));
        }
        #endregion

        #region Private Methods

        private Paint SetupPaint() {
            var paint = new Paint();
            paint.AntiAlias = true;
            paint.SetTypeface(Resources.GetFont(Resource.Font.Nunito_Regular));
            return paint;
        }


        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height) {
            mThread = new RenderingThread(this,SetupPaint(),DC);
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
}
