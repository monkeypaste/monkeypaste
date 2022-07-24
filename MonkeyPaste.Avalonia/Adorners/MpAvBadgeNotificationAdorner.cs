using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using PropertyChanged;
namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvBadgeNotificationAdorner : MpAvAdornerBase {
        #region Private Variables
        
        private MpIBadgeNotificationViewModel _notifier;

        #endregion

        #region Properties

        public bool HasNotification {
            get {
                if(_notifier == null) {
                    return false;
                }
                return _notifier.HasBadgeNotification;
            }
        }

        #endregion

        #region Public Methods
        public MpAvBadgeNotificationAdorner(Control control) : base(control) { 
            if(control != null && control.DataContext is MpIBadgeNotificationViewModel) {
                _notifier = control.DataContext as MpIBadgeNotificationViewModel;
            }
        }
        #endregion

        #region Overrides
        public override void Render(DrawingContext context) {
            if(AdornedControl == null) {
                return;
            }

            var rect = AdornedControl.Bounds;

            if(!HasNotification) {
                IsVisible = false;
                return;
            }

            IsVisible = true;
            context.DrawEllipse(Brushes.Red, new Pen(Brushes.White, 1), rect.TopLeft, 5, 5);

            base.Render(context);
        }
        #endregion
    }
}
