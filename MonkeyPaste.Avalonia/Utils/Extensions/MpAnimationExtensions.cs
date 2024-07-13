using Avalonia.Controls;
using Avalonia.Threading;
using MonkeyPaste.Common;
using System;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MonkeyPaste.Avalonia {
    public static class MpAnimationExtensions {
        public static void AnimateSize(
            this MpIBoundSizeViewModel bsvm,
            MpSize new_size,
            Control control = null,
            Func<bool> onComplete = null,
            Func<MpSizeChangeEventArgs, bool> onTick = null,
            double ntfAnimThresholdDelta = 10.0d) {
            double zeta, omega, fps;

            double cw = bsvm.ContainerBoundWidth;
            double ch = bsvm.ContainerBoundHeight;
            double nw = new_size.Width;
            double nh = new_size.Height;

            if (!nw.IsNumber()) {

            }
            if (!nh.IsNumber()) {
            }
            if (!bsvm.ContainerBoundWidth.IsNumber()) {
                bsvm.ContainerBoundWidth = cw;
            }
            if (!bsvm.ContainerBoundHeight.IsNumber()) {
                bsvm.ContainerBoundHeight = ch;
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
                if (bsvm is MpIAnimatedSizeViewModel asvm_begin &&
                    Math.Abs(new MpPoint(nw, nh).Length - new MpPoint(cw, ch).Length) >= ntfAnimThresholdDelta) {
                    // change is sig enough to ntf 
                    // (currently used for scroll anchoring when sidebar opens/closes)
                    asvm_begin.IsAnimating = true;
                }

                double lw, lh;
                while (true) {
                    lw = bsvm.ContainerBoundWidth;
                    lh = bsvm.ContainerBoundHeight;
                    MpAnimationHelpers.Spring(ref cw, ref vx, nw, delay_ms / 1000.0d, zeta, omega);
                    MpAnimationHelpers.Spring(ref ch, ref vy, nh, delay_ms / 1000.0d, zeta, omega);
                    bsvm.ContainerBoundWidth = cw;
                    bsvm.ContainerBoundHeight = ch;
                    if (control != null) {
                        control.Width = cw;
                        control.Height = ch;
                    }
                    onTick?.Invoke(new MpSizeChangeEventArgs(new MpSize(lw, lh), new MpSize(cw, ch)));

                    await Task.Delay(delay_ms);

                    bool is_v_zero = Math.Abs(vx) < 0.1d;
                    if (is_v_zero) {
                        break;
                    }
                }
                lw = bsvm.ContainerBoundWidth;
                lh = bsvm.ContainerBoundHeight;
                bsvm.ContainerBoundWidth = nw;
                bsvm.ContainerBoundHeight = nh;
                if (control != null) {
                    control.Width = nw;
                    control.Height = nh;
                }

                onTick?.Invoke(new MpSizeChangeEventArgs(new MpSize(lw, lh), new MpSize(cw, ch)));
                onComplete?.Invoke();

                if (bsvm is MpIAnimatedSizeViewModel asvm_end) {
                    asvm_end.IsAnimating = false;
                }
            });
        }

        public static double FpsToTimeStep(this double fps) {
            return 1000d / fps / 1000d;
        }
        public static TimeSpan FpsToDelayTime(this double fps) {
            return TimeSpan.FromMilliseconds(1000d / fps);
        }

        public static async Task AnimatePointAsync(this MpPoint start, MpPoint end, double tts, double fps, Action<MpPoint> tick, bool tickWithVelocity = false) {
            double dt = 0;
            double time_step = fps.FpsToTimeStep();
            var delay_ms = fps.FpsToDelayTime();

            var d = end - start;
            var v = (d / tts) * time_step;
            MpPoint cur = start;
            while (true) {
                tick?.Invoke(tickWithVelocity ? v : cur);
                cur += v;
                await Task.Delay(delay_ms);
                dt += time_step;
                if (dt >= tts) {
                    // animation complete, ensure it uses end props      
                    tick?.Invoke(end);
                    break;
                }
            }
        }
        
        public static async Task AnimateDoubleAsync(this double start, double end, double tts, double fps, Action<double> tick, bool tickWithVelocity = false) {
            double dt = 0;
            double time_step = fps.FpsToTimeStep();
            var delay_ms = fps.FpsToDelayTime();

            var d = end - start;
            var v = (d / tts) * time_step;
            double cur = start;
            while (true) {
                tick?.Invoke(tickWithVelocity ? v : cur);
                cur += v;
                await Task.Delay(delay_ms);
                dt += time_step;
                if (dt >= tts) {
                    // animation complete, ensure it uses end props      
                    tick?.Invoke(end);
                    break;
                }
            }
        }
    }
}
