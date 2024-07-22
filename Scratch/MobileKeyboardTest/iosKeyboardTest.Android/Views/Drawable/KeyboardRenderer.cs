using Android.Graphics;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Java.Util;
using System.Collections.Generic;

namespace iosKeyboardTest.Android {
    public class KeyboardRenderer : Thread, IKeyboardViewRenderer {
        //public static KeyboardRenderer Instance { get; private set; }
        private Paint paint;
        private TextureView mSurface;
        KeyboardViewModel DC { get; set; }
        private volatile bool mRunning = true;
        List<KeyRenderer> KeyRenderers { get; set; } = [];

        public KeyboardRenderer(TextureView surface, Paint paint, KeyboardViewModel kbvm) {
            //Instance = this;

            DC = kbvm;
            this.paint = paint;
            mSurface = surface;
            KeyboardPalette.SetTheme(DC.IsThemeDark);

            foreach(var kvm in DC.Keys) {
                KeyRenderers.Add(new KeyRenderer(kvm/*, paint, surface*/));
            }
        }
        public override void Run() {
            //paint.setColor(0xff00ff00);

            while (mRunning && !Thread.Interrupted()) {
                var canvas = mSurface.LockCanvas(null);
                try {
                    DrawKeyboard(canvas);
                }
                finally {
                    mSurface.UnlockCanvasAndPost(canvas);
                    //LockedCanvas = null;
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
            paint.Color = KeyboardPalette.BgHex.ToColor();
            canvas.DrawRect(new Rect(0, 0, canvas.Width, canvas.Height), paint);
        }

        void DrawMenu(Canvas canvas) {
            int l = 0;
            int t = 0;
            int r = canvas.Width;
            int b = DC.MenuHeight.UnscaledI();
            Rect rectangle = new Rect(l, t, r, b);
            paint.Color = KeyboardPalette.MenuBgHex.ToColor();
            canvas.DrawRect(rectangle, paint);
        }

        void DrawKeys(Canvas canvas) {
            foreach(var kr in KeyRenderers) {
                kr.DrawKey(canvas,paint);
            }
        }       

        void DrawCursorControl(Canvas canvas) {
            if (!DC.IsCursorControlEnabled) {
                return;
            }
        }

        public void Layout(bool invalidate) {
            if(invalidate) {
                RenderInternal();
            }
        }

        public void Measure(bool invalidate) {
            if (invalidate) {
                RenderInternal();
            }
        }

        void IKeyboardViewRenderer.Paint(bool invalidate) {
            if (invalidate) {
                RenderInternal();
            }
        }

        public void Render(bool invalidate) {
            if (invalidate) {
                RenderInternal();
            }
        }

        public void RenderInternal(bool fromLoop = false) {
            if(!fromLoop) {
                return;
            }
           
        }
    }
}
