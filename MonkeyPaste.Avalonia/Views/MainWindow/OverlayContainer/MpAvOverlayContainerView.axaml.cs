using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvOverlayContainerView : MpAvUserControl {
        #region Private Variables
        #endregion

        #region Constants
        const double DEFAULT_ANIM_TIME_S = 0.25d; // needs to match MpAvChildWindow anim durations
        #endregion

        #region Statics
        private static MpAvOverlayContainerView _instance;
        public static MpAvOverlayContainerView Instance =>
            _instance;

        #endregion

        #region Interfaces
        #endregion

        #region Properties

        public MpAvChildWindow TopWindow =>
            OverlayGrid.Children.LastOrDefault() as MpAvChildWindow;

        #endregion

        #region Constructors
        public MpAvOverlayContainerView() {
            MpDebug.Assert(_instance == null, "Singleton error");
            _instance = this;
            InitializeComponent();
            this.IsHitTestVisible = false;
        }

        #endregion

        #region Public Methods
        public void ShowWindow(MpAvChildWindow cw) {
#if WINDOWED
            if (cw == null ||
                cw == TopWindow) {
                return;
            }
#endif
            if(OverlayGrid.Children.Contains(cw)) {
                OverlayGrid.Children.Remove(cw);
            }

            void OnChildLoaded(object sender, EventArgs e) {
                cw.Loaded -= OnChildLoaded;
                if(cw.RenderTransform is not TransformGroup tg ||
                    tg.Children.OfType<TranslateTransform>().FirstOrDefault() is not { } tt) {
                    return;
                }

                if(cw.OpenTransition.HasFlag(MpChildWindowTransition.SlideInFromLeft)) {
                    tt.X = -cw.Bounds.Width;
                }
                if(cw.OpenTransition.HasFlag(MpChildWindowTransition.SlideInFromTop)) {
                    tt.Y = -cw.Bounds.Height;
                }
                Dispatcher.UIThread.Post(async () => {
                    await AnimateAsync(cw, tt, MpPoint.Zero, false);
                    cw.Activate();
                });
            }
            cw.SetCurrentValue(OpacityProperty, 0);
            cw.Loaded += OnChildLoaded;
            OverlayGrid.Children.Add(cw);
            this.IsHitTestVisible = OverlayGrid.Children.Any();
        }

        public bool RemoveWindow(MpAvChildWindow cw) {
            if(!OverlayGrid.Children.Contains(cw) ||
                cw.RenderTransform is not TransformGroup tg ||
                tg.Children.OfType<TranslateTransform>().FirstOrDefault() is not { } tt) {
                return false;
            }
            MpPoint end = new MpPoint(tt.X, tt.Y);
            if (cw.OpenTransition.HasFlag(MpChildWindowTransition.SlideOutToLeft)) {
                end.X = -cw.Bounds.Width;
            }
            if (cw.OpenTransition.HasFlag(MpChildWindowTransition.SlideOutToTop)) {
                end.Y = -cw.Bounds.Height;
            }
            Dispatcher.UIThread.Post(async () => {
                await AnimateAsync(cw, tt, end, true);
                OverlayGrid.Children.Remove(cw);
                this.IsHitTestVisible = OverlayGrid.Children.Any();

                if (TopWindow == null) {
#if MOBILE_OR_WINDOWED
                    MpAvMainView.Instance.Activate();
#endif
                } else {
                    TopWindow.Activate();
                }
            });
            return true;
        }
        #endregion

        #region Protected Methods

        protected override void OnPointerReleased(PointerReleasedEventArgs e) {
            base.OnPointerReleased(e);

            //if (e.Source is not Control sc ||
            //    sc.GetSelfAndVisualAncestors().Any(x => x == OverlayContentControl.Content) ||
            //    OverlayContentControl.Content is not MpAvChildWindow cw) {
            //    // touch on overlay don't close
            //    return;
            //}
            //cw.Close();
        }
        #endregion

        #region Private Methods


        private async Task AnimateAsync(MpAvChildWindow cw, TranslateTransform tt, MpPoint end, bool is_out, double t_s = DEFAULT_ANIM_TIME_S) {
            var start = new MpPoint(tt.X, tt.Y);
            var d = end - start;
            double time_step = 20d / 1000d;
            var tt_v = (d / t_s) * time_step;
            double op_v = (is_out ? -1 : 1) * ((1 / t_s) * time_step);
            double dt = 0;
            while (true) {
                tt.X += tt_v.X;
                tt.Y += tt_v.Y;
                cw.Opacity += op_v;
                if (dt >= t_s) {
                    cw.Opacity = is_out ? 0 : 1;
                    tt.X = end.X;
                    tt.Y = end.Y;
                    break;
                }
                await Task.Delay(20);
                dt += time_step;
            }
        }
        #endregion

        #region Commands
        #endregion




    }
}
