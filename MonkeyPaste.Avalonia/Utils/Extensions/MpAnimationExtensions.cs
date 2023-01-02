using Avalonia.Threading;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAnimationExtensions {
        public static void AnimateSize(
            this MpIBoundSizeViewModel bsvm, 
            MpSize new_size, 
            Func<bool> onComplete = null, 
            Func<MpSizeChangeEventArgs, bool> onTick = null) {
            double zeta, omega, fps;

            double cw = bsvm.BoundWidth;
            double ch = bsvm.BoundHeight;
            double nw = new_size.Width;
            double nh = new_size.Height;

            if (!bsvm.BoundWidth.IsNumber()) {
                bsvm.BoundWidth = cw;
            }
            if (!bsvm.BoundHeight.IsNumber()) {
                bsvm.BoundHeight = ch;
            }

            if (nw > cw || nh > ch) {
                zeta = 0.5d;
                omega = 30.0d;
                fps = 40.0d;
            } else {
                zeta = 1.0d;
                omega = 30.0d;
                fps = 40.0d;
            }

            int delay_ms = (int)(1000 / fps);
            double dw = nw - cw;
            double dh = nh - ch;
            double step_w = dw / delay_ms;
            double step_h = dh / delay_ms;
            double vx = 0;
            double vy = 0;
            Dispatcher.UIThread.Post(async () => {
                double lw, lh;
                while (true) {
                    lw = bsvm.BoundWidth;
                    lh = bsvm.BoundHeight;
                    MpAnimationHelpers.Spring(ref cw, ref vx, nw, delay_ms / 1000.0d, zeta, omega);
                    MpAnimationHelpers.Spring(ref ch, ref vy, nh, delay_ms / 1000.0d, zeta, omega);
                    bsvm.BoundWidth = cw;
                    bsvm.BoundHeight = ch;
                    onTick?.Invoke(new MpSizeChangeEventArgs(new MpSize(lw,lh),new MpSize(cw,ch)));

                    await Task.Delay(delay_ms);

                    bool is_v_zero = Math.Abs(vx) < 0.1d;
                    if (is_v_zero) {
                        break;
                    }
                }
                lw = bsvm.BoundWidth;
                lh = bsvm.BoundHeight;
                bsvm.BoundWidth = nw;
                bsvm.BoundHeight = nh;

                onTick?.Invoke(new MpSizeChangeEventArgs(new MpSize(lw, lh), new MpSize(cw, ch)));
                onComplete?.Invoke();
            });
        }
    }
}
