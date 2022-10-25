using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;
using MonkeyPaste.Common.Avalonia;
using Avalonia.Threading;
using Avalonia;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public abstract class MpAvAdornerBase : Control {
        private bool _wasVisible = false;
        public Control AdornedControl { get; private set; }

        public MpAvAdornerBase(Control adornedControl) : base() {            
            AdornedControl = adornedControl;
            //adornedControl.GetObservable(Control.IsVisibleProperty).Subscribe(value => Draw());
            //adornedControl.GetObservable(Control.BoundsProperty).Subscribe(value => Draw());
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
            if(forceIsVisibleValue.HasValue) {
                IsVisible = forceIsVisibleValue.Value;
            } else {
                IsVisible = true;
            }
            Dispatcher.UIThread.VerifyAccess();
            this.InvalidateVisual();
        }
    }
}
