using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;

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

    public abstract class MpSingletonBehavior<T,B> : MpBehavior<T> 
        where B : new()
        where T : FrameworkElement {
        #region Singleton Definition
        private static readonly Lazy<B> _Lazy = new Lazy<B>(() => new B());
        public static B Instance { get { return _Lazy.Value; } }
        #endregion
    }
}
