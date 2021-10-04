using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using MonkeyPaste;


namespace MpWpfApp {
    public class MpSelectionBehavior : Behavior<FrameworkElement> {
        bool isTile;
        protected override void OnAttached() {
            isTile = AssociatedObject is MpClipTileView;
            AssociatedObject.MouseLeftButtonDown += AssociatedObject_MouseLeftButtonDown;
        }

        private void AssociatedObject_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if(isTile) {
                var ctvm = AssociatedObject.DataContext as MpClipTileViewModel;
            } 
        }
    }
}
