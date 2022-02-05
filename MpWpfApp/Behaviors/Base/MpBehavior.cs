using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Documents;

namespace MpWpfApp {
    public abstract class MpBehavior<T> : Behavior<T> where T: FrameworkElement {
        protected bool _wasUnloaded;
        protected object _dataContext;
        protected bool _isLoaded = false;
        
        protected override void OnAttached() {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;
            if (AssociatedObject.IsLoaded) {
                _dataContext = AssociatedObject.DataContext;
                OnLoad();
            }
        }

        protected virtual void OnLoad() {
            MpMainWindowViewModel.Instance.OnMainWindowShow += OnMainWindowShow; 
            MpMainWindowViewModel.Instance.OnMainWindowHide += OnMainWindowHide;
            if(AssociatedObject != null) {
                _dataContext = AssociatedObject.DataContext;
            }

            _isLoaded = true;
        }
        protected virtual void OnUnload() {
            _wasUnloaded = true;
            _isLoaded = false;
        }

        protected virtual void OnMainWindowHide(object sender, EventArgs e) { }

        protected virtual void OnMainWindowShow(object sender, EventArgs e) { }
        
        private void AssociatedObject_Unloaded(object sender, RoutedEventArgs e) {
            OnUnload();
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e) {
            OnLoad();
        }
    }

    
}
