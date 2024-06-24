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

        public MpAvAdornerBase(Control ac) : base() {
            IsVisible = false;
            AdornedControl = ac;
            if (AdornedControl != null) {
                AdornedControl.Unloaded += AdornedControl_Unloaded;
                AdornedControl.Loaded += AdornedControl_Loaded;
                if (AdornedControl.IsLoaded) {
                    AdornedControl_Loaded(AdornedControl, null);
                }
            }
        }

        private void AdornedControl_Loaded(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (AdornedControl is not Control ac ||
                AdornerLayer.GetAdornerLayer(ac) is not { } al) {
                return;
            }
            al.Children.Add(this);
            AdornerLayer.SetAdornedElement(this, ac);
        }

        private void AdornedControl_Unloaded(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            Remove();
            if (sender is not Control c) {
                return;
            }
            c.Unloaded -= AdornedControl_Unloaded;
        }


        public virtual void Remove() {
            if (AdornedControl is not Control ac ||
                AdornerLayer.GetAdornerLayer(ac) is not { } al ||
                this is not ISetLogicalParent slp) {
                return;
            }
            al.Children.Remove(this);
            slp.SetParent(null);
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
