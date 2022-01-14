using GongSolutions.Wpf.DragDrop.Utilities;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MpWpfApp {
    public class MpNotificationAdorner : Adorner {
        #region Private Variables
        
        private MpIHasNotification _notifier;

        #endregion

        #region Properties

        public bool HasNotification {
            get {
                if(_notifier == null) {
                    return false;
                }
                return _notifier.HasNotification;
            }
        }

        #endregion

        #region Public Methods
        public MpNotificationAdorner(UIElement uie) : base(uie) { 
            if(uie != null && uie is FrameworkElement fe && fe.DataContext is MpIHasNotification) {
                _notifier = fe.DataContext as MpIHasNotification;
            }
        }
        #endregion

        #region Overrides
        protected override void OnRender(DrawingContext drawingContext) {
            var rect = (AdornedElement as FrameworkElement).Bounds();

            if(!HasNotification) {
                Visibility = Visibility.Hidden;
                return;
            }

            Visibility = Visibility.Visible;
            drawingContext.DrawEllipse(Brushes.Red, new Pen(Brushes.White, 1), rect.TopLeft, 5, 5);            
        }
        #endregion
    }
}
