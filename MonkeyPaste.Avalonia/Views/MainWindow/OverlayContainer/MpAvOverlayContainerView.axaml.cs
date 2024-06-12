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
using DynamicData;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvOverlayContainerView : MpAvUserControl {
        #region Private Variables
        List<MpAvChildWindow> AnimatingWindows { get; set; } = [];
        #endregion

        #region Constants
        const double DEFAULT_ANIM_TIME_S = 0.25d; 
        const double DEFAULT_ANIM_FPS = 50; 

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

        #region Events
        public event EventHandler<MpAvChildWindow> OnChildRemoved;
        #endregion

        #region Constructors
        public MpAvOverlayContainerView() {
            MpDebug.Assert(_instance == null, "Singleton error");
            _instance = this;
            InitializeComponent();
            this.IsHitTestVisible = false;
            OverlayGrid.Children.CollectionChanged += Children_CollectionChanged;
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

            if (!CanShowOrHide(cw)) {
                MpConsole.WriteLine($"Window '{cw.DataContext}' SHOW CANCELED");
                return;
            }
            if (OverlayGrid.Children.Contains(cw)) {
                OverlayGrid.Children.Move(OverlayGrid.Children.IndexOf(cw),OverlayGrid.Children.Count - 1);
                return;
            }

            void OnChildLoaded(object sender, EventArgs e) {
                cw.Loaded -= OnChildLoaded;
                BusySpinner.IsVisible = false;
                Dispatcher.UIThread.Post(async () => {
                    cw.SetCurrentValue(OpacityProperty, 1);
                    await AnimateAsync(cw, false);

                    if (cw.DataContext is MpILoadableViewModel lvm &&
                        lvm.IsLoadable) {
                        lvm.IsLoaded = true;
                    }
                    cw.Activate();
                });
            }
            if(cw.DataContext is MpILoadableViewModel lvm &&
                lvm.IsLoadable) {
                lvm.IsLoaded = false;
            }
            BusySpinner.IsVisible = true;
            // load window transparent to get dimensions 
            cw.SetCurrentValue(OpacityProperty, 0);
            cw.Loaded += OnChildLoaded;
            OverlayGrid.Children.Add(cw);

            this.IsHitTestVisible = OverlayGrid.Children.Any();
        }

        public bool RemoveWindow(MpAvChildWindow cw) {
            if(!OverlayGrid.Children.Contains(cw)) {
                return false;
            }
            if (!CanShowOrHide(cw)) {
                MpConsole.WriteLine($"Window '{cw.DataContext}' CLOSE CANCELED");
                return false;
            }
            Dispatcher.UIThread.Post(async () => {
            await AnimateAsync(cw, true);
            if (OverlayGrid.Children.Remove(cw)) {
                OnChildRemoved?.Invoke(this, cw);
            }
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
        private async Task AnimateAsync(MpAvChildWindow cw, bool isClosing, double t_s = DEFAULT_ANIM_TIME_S, double fps = DEFAULT_ANIM_FPS) {
            if (cw.RenderTransform is not TransformGroup tg ||
                tg.Children.OfType<TranslateTransform>().FirstOrDefault() is not { } tt) {
                return;
            }
            double offset_factor = 0.5;
            MpPoint tt_start = new(tt.X,tt.Y);
            MpPoint tt_end = new(tt.X,tt.Y);
            double op_start = cw.Opacity;
            double op_end = cw.Opacity;
            MpChildWindowTransition trans;
            trans = isClosing ? cw.CloseTransition : cw.OpenTransition;
            foreach(MpChildWindowTransition cwt in Enum.GetValues(typeof(MpChildWindowTransition))) {
                if(!trans.HasFlag(cwt)) {
                    continue;
                }
                switch (cwt) {
                    case MpChildWindowTransition.SlideInFromTop:
                        tt_start.Y = -cw.Bounds.Height * offset_factor;
                        break;
                    case MpChildWindowTransition.SlideOutToTop:
                        tt_end.Y = -cw.Bounds.Height * offset_factor;
                        break;
                    case MpChildWindowTransition.SlideInFromLeft:
                        tt_start.X = -cw.Bounds.Width * offset_factor;
                        break;
                    case MpChildWindowTransition.SlideOutToLeft:
                        tt_end.X = -cw.Bounds.Width * offset_factor;
                        break;
                    case MpChildWindowTransition.FadeIn:
                        op_start = 0;
                        op_end = 1;
                        break;
                    case MpChildWindowTransition.FadeOut:
                        op_end = 0;
                        break;
                }
            }
            tt.X = tt_start.X;
            tt.Y = tt_start.Y;
            cw.Opacity = op_start;
            

            MpConsole.WriteLine($"Animation [STARTED] '{cw.DataContext}' start:{tt_start} end:{tt_end} is_out:{isClosing}");

            double dt = 0;
            double time_step = fps.FpsToTimeStep();
            int delay_ms = fps.FpsToDelayTime();

            var tt_d = tt_end - tt_start;
            var tt_v = (tt_d / t_s) * time_step;
            double op_d = op_end - op_start;
            double op_v = (op_d / t_s) * time_step;
            while (true) {
                tt.X += tt_v.X;
                tt.Y += tt_v.Y;
                cw.Opacity += op_v;
                await Task.Delay(delay_ms);
                dt += time_step;
                if (dt >= t_s) {
                    // animation complete, ensure it uses end props 
                    cw.Opacity = op_end;
                    tt.X = tt_end.X;
                    tt.Y = tt_end.Y;
                    break;
                }
            }
            RemoveAnimation(cw);
        }

        private bool RemoveAnimation(MpAvChildWindow cw) {
            if (cw.DataContext is MpIAnimatable anim_vm) {
                anim_vm.IsAnimating = false;
            }
            return AnimatingWindows.Remove(cw);
        }
        private bool CanShowOrHide(MpAvChildWindow cw) {
            if (AnimatingWindows.Contains(cw)) {
                return false;
            }
            AnimatingWindows.Add(cw);
            if(cw.DataContext is MpIAnimatable anim_vm) {
                anim_vm.IsAnimating = true;
            }
            return true;
        }
        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            Dispatcher.UIThread.Post(async () => {
                // wait for collection change to finish to avoid weird enumerable exceptions
                await Task.Delay(300);
                HandleAirSpaceWindows();
            });
        }

        private void HandleAirSpaceWindows() {
            var airspace_windows = OverlayGrid.Children.Where(x => x.Classes.Contains("air-space"));
            if(!airspace_windows.Any()) {
                // no problems or top window has air-space
                return;
            }
            if(airspace_windows.Contains(TopWindow)) {
                if(!TopWindow.IsVisible) {
                    // restore visibility
                    TopWindow.IsVisible = true;
                }
                return;
            }
            // some lower window has air space
            airspace_windows.ForEach(x => x.IsVisible = false);
        }
        #endregion

        #region Commands
        #endregion




    }
}
