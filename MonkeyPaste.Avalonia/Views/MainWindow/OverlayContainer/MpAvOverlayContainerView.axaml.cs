using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvOverlayContainerView : MpAvUserControl {
        private static MpAvOverlayContainerView _instance;
        public static MpAvOverlayContainerView Instance =>
            _instance;
        public MpAvOverlayContainerView() {
            MpDebug.Assert(_instance == null, "Singleton error");
            _instance = this;
            InitializeComponent();

            this.IsHitTestVisible = false;
            OverlayCanvas.Children.CollectionChanged += Children_CollectionChanged;
        }

        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            this.IsHitTestVisible = OverlayCanvas.Children.Any();
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e) {
            base.OnPointerReleased(e);

            if(e.Source is not Control sc) {
                return;
            }
            if(sc.GetVisualAncestor<MpAvWindow>() is not { } w) {
                // close overlay when tap bg
                OverlayCanvas.Children.OfType<MpAvWindow>().ForEach(x => x.Close());
            }
        }

        public void AddChild(MpAvChildWindow cw) {
#if WINDOWED
            if (cw is MpAvMainView || cw is MpAvMainWindow) {
                return;
            } 
#endif

            if (OverlayCanvas.Children.Contains(cw)) {
                // show or ignore here
            } else {
                OverlayCanvas.Children.Add(cw);

                cw.Bind(
                    Canvas.LeftProperty,
                    new Binding() {
                        Path = nameof(cw.CanvasX),
                        Mode = BindingMode.OneWay,
                        Source = cw
                    });

                cw.Bind(
                    Canvas.TopProperty,
                    new Binding() {
                        Path = nameof(cw.CanvasY),
                        Mode = BindingMode.OneWay,
                        Source = cw
                    });
            }
            MpConsole.WriteLine($"Child window '{this}' shown");
        }

        public bool RemoveChild(MpAvChildWindow cw) {
#if WINDOWED
            if (cw is MpAvMainView) {
                return false;
            } 
#endif
            return OverlayCanvas.Children.Remove(cw);
        }
    }
}
