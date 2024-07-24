using Android.Content;
using Android.Graphics;
using Android.Views;
using System.Collections.Generic;

namespace iosKeyboardTest.Android {
    public class KeyGridView : CustomViewGroup, IKeyboardViewRenderer {
        public KeyboardViewModel DC { get; set; }

        public KeyGridView(Context context, Paint paint, KeyboardViewModel dC) : base(context,paint) {
            DC = dC;
            AddOrResetKeys();            
        }

        public void AddOrResetKeys() {
            this.RemoveAllViews();

            foreach (var kvm in DC.Keys) {
                var kv = new KeyView(kvm, this.Context, SharedPaint).SetDefaultProps($"Key '{kvm.CurrentChar}'");
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
            Frame = DC.KeyboardRect.ToRectF();


            for (int i = 0; i < ChildCount; i++) {
                if (GetChildAt(i) is not KeyView kv) {
                    continue;
                }
                kv.Measure(invalidate);
            }
            this.Measure(
                View.MeasureSpec.MakeMeasureSpec((int)Frame.Width(), MeasureSpecMode.Exactly),
                View.MeasureSpec.MakeMeasureSpec((int)Frame.Height(), MeasureSpecMode.Exactly));
            this.Layout((int)Frame.Left, (int)Frame.Top, (int)Frame.Right, (int)Frame.Bottom);
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