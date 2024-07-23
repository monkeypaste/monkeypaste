using Android.Content;
using Android.Graphics;
using System.Collections.Generic;

namespace iosKeyboardTest.Android {
    public class KeyGridView : CustomViewGroup, IKeyboardViewRenderer {
        Paint SharedPaint { get; set; }
        public KeyboardViewModel DC { get; set; }

        public KeyGridView(Context context, Paint paint, KeyboardViewModel dC) : base(context,paint) {
            DC = dC;

            foreach (var kvm in DC.Keys) {
                var kv = new KeyView(kvm, context, paint).SetDefaultProps($"Key '{kvm.CurrentChar}'");
                this.AddView(kv);
            }
        }

        public void Layout(bool invalidate) {
            for (int i = 0; i < ChildCount; i++) {
                if(GetChildAt(i) is not KeyView kv) {
                    continue;
                }
                kv.Layout(invalidate);
            }
            if(invalidate) {
                this.Redraw();
            }
        }

        public void Measure(bool invalidate) {
            int[] p = new int[2];
            this.GetLocationOnScreen(p);

            float kgl = 0;
            float kgt = (float)DC.MenuHeight;
            float kgr = (float)DC.KeyboardWidth;
            float kgb = kgt + (float)DC.KeyboardHeight;
            Frame = new RectF(kgl, kgt, kgr, kgb);

            for (int i = 0; i < ChildCount; i++) {
                if (GetChildAt(i) is not KeyView kv) {
                    continue;
                }
                kv.Measure(invalidate);
            }
            if (invalidate) {
                this.Redraw();
            }
        }

        public void Paint(bool invalidate) {
            SetBackgroundColor(KeyboardPalette.BgHex.ToColor());

            for (int i = 0; i < ChildCount; i++) {
                if (GetChildAt(i) is not KeyView kv) {
                    continue;
                }
                kv.Paint(invalidate);
            }
            if (invalidate) {
                this.Redraw();
            }
        }

        public void Render(bool invalidate) {            
            Layout(false);
            Measure(false);
            Paint(false);
            for (int i = 0; i < ChildCount; i++) {
                if (GetChildAt(i) is not KeyView kv) {
                    continue;
                }
                kv.Render(invalidate);
            }
            if (invalidate) {
                this.Redraw();
            }
        }
    }
}