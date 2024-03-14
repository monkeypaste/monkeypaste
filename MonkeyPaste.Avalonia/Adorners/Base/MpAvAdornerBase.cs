using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
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
            if (AdornedControl != null) {
                AdornedControl.Unloaded += AdornedControl_Unloaded;
                AdornedControl.Loaded += AdornedControl_Loaded;
                if (AdornedControl.IsLoaded) {
                    AdornedControl_Loaded(AdornedControl, null);
                }
            }
        }

        private void AdornedControl_Loaded(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            var adornerLayer = AdornerLayer.GetAdornerLayer(AdornedControl);
            adornerLayer.Children.Add(this);
            AdornerLayer.SetAdornedElement(this, AdornedControl);
        }

        private void AdornedControl_Unloaded(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            Remove();
            if (sender is not Control c) {
                return;
            }
            c.Unloaded -= AdornedControl_Unloaded;
        }


        public virtual void Remove() {
            if (AdornedControl is not Control adornedControl ||
                AdornerLayer.GetAdornerLayer(adornedControl) is not { } al) {
                return;
            }
            al.Children.Remove(this);
            ((ISetLogicalParent)this).SetParent(null);
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
