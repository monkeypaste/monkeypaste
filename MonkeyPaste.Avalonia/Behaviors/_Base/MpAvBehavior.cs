using Avalonia.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia;
using Avalonia.Data;
using Avalonia.Threading;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {

    [DoNotNotify]
    public abstract class MpAvBehavior<T> : Behavior<T> where T : Control {
        #region Protected Variables

        protected bool _wasUnloaded;
        protected object _dataContext;
        protected bool _isLoaded = false;

        #endregion

        #region Statics
        static MpAvBehavior() {
            IsEnabledProperty.Changed.AddClassHandler<Control>((x, y) => HandleIsEnabledChanged(x, y));
        }

        #endregion
        public T AssociatedObjectRef => AssociatedObject;

        public object test { get; set; }

        #region IsEnabled AvaloniaProperty
        public static bool GetIsEnabled(AvaloniaObject obj) {
            return obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(AvaloniaObject obj, bool value) {
            obj.SetValue(IsEnabledProperty, value);
        }

        public static readonly AttachedProperty<bool> IsEnabledProperty =
            AvaloniaProperty.RegisterAttached<object, Control, bool>(
                "IsEnabled",
                false,
                false);

        private static void HandleIsEnabledChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if (element is Control control &&
                e.NewValue is bool isEnabledVal) {
                if(!isEnabledVal) {

                }
            }
        }


        #endregion

        public void Reattach() {
            //if(AssociatedObject == null && _dataContext != null) {
            //    AssociatedObject = Application.Current.MainWindow
            //                        .GetVisualDescendent<MpClipTrayContainerView>()
            //                        .GetVisualDescendents<MpRtbContentView>()
            //                        .FirstOrDefault(x => x.DataContext == _dataContext) as T;
            //}
            Dispatcher.UIThread.Post(() => {
                var assocObj = AssociatedObject;
                Detach();
                Attach(assocObj);
            });
        }

        protected override void OnAttached() {
            base.OnAttached();
            OnLoad();
            test = AssociatedObject;

            AssociatedObject.AttachedToLogicalTree += AssociatedObject_AttachedToLogicalTree;
            AssociatedObject.DetachedFromLogicalTree += AssociatedObject_DetachedFromLogicalTree;
            if (AssociatedObject.IsInitialized) {
                AssociatedObject_AttachedToLogicalTree(AssociatedObject, null);
            }
        }


        protected override void OnDetaching() {
            base.OnDetaching();
            OnUnload();
        }

        protected virtual void OnLoad() {
            MpAvMainWindowViewModel.Instance.OnMainWindowOpened += OnMainWindowShow;
            MpAvMainWindowViewModel.Instance.OnMainWindowClosed += OnMainWindowHide;
            if (AssociatedObject != null) {
                _dataContext = AssociatedObject.DataContext;
            }

            _isLoaded = true;
        }
        protected virtual void OnUnload() {
            _wasUnloaded = true;
            _isLoaded = false;
            MpAvMainWindowViewModel.Instance.OnMainWindowOpened -= OnMainWindowShow;
            MpAvMainWindowViewModel.Instance.OnMainWindowClosed -= OnMainWindowHide;
        }


        protected virtual void OnMainWindowHide(object sender, EventArgs e) { }

        protected virtual void OnMainWindowShow(object sender, EventArgs e) { }


        private void AssociatedObject_DetachedFromLogicalTree(object sender, global::Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e) {
            OnUnload();
        }

        private void AssociatedObject_AttachedToLogicalTree(object sender, global::Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e) {
            _dataContext = AssociatedObject.DataContext;
            OnLoad();
        }
    }
}
