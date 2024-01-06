using Avalonia;
using Avalonia.Controls;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public abstract class MpAvAdornerBase :
        Control,
        MpIOverrideRender {

        #region Interfaces

        #region MpIOverrideRender Implementation
        public bool IgnoreRender { get; set; }
        #endregion

        #endregion
        public Control AdornedControl { get; private set; }

        public MpAvAdornerBase(Control adornedControl) : base() {
            IsVisible = false;
            AdornedControl = adornedControl;
            //adornedControl.GetObservable(Control.IsVisibleProperty).Subscribe(paramValue => Draw());
            //adornedControl.GetObservable(Control.BoundsProperty).Subscribe(paramValue => Draw());
            //adornedControl.DetachedFromVisualTree += AdornedControl_DetachedFromVisualTree;
            //adornedControl.DetachedFromLogicalTree += AdornedControl_DetachedFromLogicalTree;
            //adornedControl.EffectiveViewportChanged += AdornedControl_EffectiveViewportChanged;
        }

        private void AdornedControl_DetachedFromLogicalTree(object sender, global::Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e) {
            Draw(false);
        }

        private void AdornedControl_DetachedFromVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            Draw(false);
        }

        private void AdornedControl_EffectiveViewportChanged(object sender, global::Avalonia.Layout.EffectiveViewportChangedEventArgs e) {
            Draw();
        }

        public virtual void Draw(bool? forceIsVisibleValue = null) {
            if (forceIsVisibleValue.HasValue) {
                IsVisible = forceIsVisibleValue.Value;
            } else {
                IsVisible = true;
            }
            this.Redraw();
        }
    }
}
