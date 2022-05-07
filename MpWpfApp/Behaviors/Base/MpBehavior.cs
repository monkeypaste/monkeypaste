using Microsoft.Xaml.Behaviors;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Linq;

namespace MpWpfApp {
    
    public abstract class MpBehavior<T> : Behavior<T> where T: FrameworkElement {
        protected bool _wasUnloaded;
        protected object _dataContext;
        protected bool _isLoaded = false;


        public T AssociatedObjectRef => AssociatedObject;

        #region IsEnabled DependencyProperty

        public bool IsEnabled {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsEnabledProperty =
             DependencyProperty.Register(
                 "IsEnabled", typeof(bool),
                 typeof(MpBehavior<T>),
                 //new FrameworkPropertyMetadata(true));
                 new FrameworkPropertyMetadata() {
                     DefaultValue = true,
                     PropertyChangedCallback = (s, e) => {
                         bool isEnabled = (bool)e.NewValue;
                         if (!isEnabled) {
                             var b = s as MpBehavior<T>;
                             if (b != null) {
                                 b.Detach();
                             }
                         }
                     }
                 });

        #endregion

        public void Reattach() {
            //if(AssociatedObject == null && _dataContext != null) {
            //    AssociatedObject = Application.Current.MainWindow
            //                        .GetVisualDescendent<MpClipTrayContainerView>()
            //                        .GetVisualDescendents<MpContentView>()
            //                        .FirstOrDefault(x => x.DataContext == _dataContext) as T;
            //}
            var assocObj = AssociatedObject;
            Detach();
            Attach(assocObj);
        }

        protected override void OnAttached() {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;
            if (AssociatedObject.IsLoaded) {
                _dataContext = AssociatedObject.DataContext;
                OnLoad();
            }
        }

        protected override void OnDetaching() {
            base.OnDetaching();
            OnUnload();
        }

        protected virtual void OnLoad() {
            MpMainWindowViewModel.Instance.OnMainWindowShow += OnMainWindowShow; 
            MpMainWindowViewModel.Instance.OnMainWindowHidden += OnMainWindowHide;
            if(AssociatedObject != null) {
                _dataContext = AssociatedObject.DataContext;
            }

            _isLoaded = true;
        }
        protected virtual void OnUnload() {
            _wasUnloaded = true;
            _isLoaded = false;
            MpMainWindowViewModel.Instance.OnMainWindowShow -= OnMainWindowShow;
            MpMainWindowViewModel.Instance.OnMainWindowHidden -= OnMainWindowHide;
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
