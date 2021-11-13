using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Globalization;
using System.Windows.Controls;
using System.Reflection;
using MonkeyPaste;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections;
using System.Runtime.CompilerServices;

namespace MpWpfApp {    
    public abstract class MpViewModelBase<P,C> : MpViewModelBase<P> 
        where P: class
        where C: class {
        
        public ObservableCollection<C> Children { get; private set; } = new ObservableCollection<C>();

        public MpViewModelBase(P p) : base(p) { }

        public void UpdateChildren(MpChildViewModelAttribute cvma, PropertyInfo cpi) {
            if (cvma.IsCollection) {
                //child view model is an observable collection
                Children = (ObservableCollection<C>)cpi.GetValue(this);
            } else {
                //child is just a view mdoel
                //var propValue = vmpi.GetValue(vm);
                //if (propValue == null) {
                //    continue;
                //}
                //childVms = new List<object> { propValue };
            }
        }

        public override void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            base.OnPropertyChanged(propertyName);

            MpHelpers.Instance.RunOnMainThreadAsync(() => {
                //check if property has affects child attribute
                var childPropAttribute = GetType().GetProperty(propertyName).GetCustomAttribute<MpChildViewModelAttribute>();
                if(childPropAttribute != null) {
                    UpdateChildren(childPropAttribute, GetType().GetProperty(propertyName));
                }
            }, System.Windows.Threading.DispatcherPriority.Normal);
        }
    }

    public abstract class MpViewModelBase<P> : INotifyPropertyChanged, IDisposable where P: class {
        #region Private Variables
        #endregion

        #region Properties

        private P _parent;
        public virtual P Parent {
            get => _parent;
            private set => SetProperty(ref _parent, value);
        }

        private bool _isBusy = false;

        public bool IsBusy {
            get => _isBusy;
            set {
                SetProperty(ref _isBusy, value);
                MpMouseViewModel.Instance.NotifyAppBusy(_isBusy);
            }
        }

        private Visibility _itemVisibility = Visibility.Visible;
        public Visibility ItemVisibility {
            get => _itemVisibility;
            set => SetProperty(ref _itemVisibility, value);
        }
        #endregion

        #region Events
        #endregion

        #region Constructors

        protected MpViewModelBase(P parent) {
            Parent = parent;

            MpDb.Instance.OnItemAdded += Instance_OnItemAdded;
            MpDb.Instance.OnItemUpdated += Instance_OnItemUpdated;
            MpDb.Instance.OnItemDeleted += Instance_OnItemDeleted;
            MpDb.Instance.SyncAdd += Instance_SyncAdd;
            MpDb.Instance.SyncUpdate += Instance_SyncUpdate;
            MpDb.Instance.SyncDelete += Instance_SyncDelete;
        }

        #endregion

        #region Public Methods

        public virtual void Dispose() {
            MpDb.Instance.OnItemAdded -= Instance_OnItemAdded;
            MpDb.Instance.OnItemUpdated -= Instance_OnItemUpdated;
            MpDb.Instance.OnItemDeleted -= Instance_OnItemDeleted;
            MpDb.Instance.SyncAdd -= Instance_SyncAdd;
            MpDb.Instance.SyncUpdate -= Instance_SyncUpdate;
            MpDb.Instance.SyncDelete -= Instance_SyncDelete;
        }

        #endregion

        #region Protected Methods


        #region Db Events

        protected virtual void Instance_SyncDelete(object sender, MpDbSyncEventArgs e) {
            
        }

        protected virtual void Instance_SyncUpdate(object sender, MpDbSyncEventArgs e) {  }

        protected virtual void Instance_SyncAdd(object sender, MpDbSyncEventArgs e) { }

        protected virtual void Instance_OnItemDeleted(object sender, MpDbModelBase e) { }

        protected virtual void Instance_OnItemUpdated(object sender, MpDbModelBase e) { }

        protected virtual void Instance_OnItemAdded(object sender, MpDbModelBase e) { }

        #endregion

        #endregion

        #region Private methods

        #endregion

        #region INotifyPropertyChanged 

        public event PropertyChangedEventHandler PropertyChanged;

        private bool ThrowOnInvalidPropertyName => false;

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "") {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                var handler = PropertyChanged;
                if (handler != null) {
                    handler(this, new PropertyChangedEventArgs(name));
                }
            }
        }

        // SetProperty example below
        //private int unitsInStock;
        //public int UnitsInStock {
        //    get { return unitsInStock; }
        //    set {
        //        SetProperty(ref unitsInStock, value);
        //    }
        //}

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            MpHelpers.Instance.RunOnMainThreadAsync(() => {
                //check if property has affects child attribute
                var affectsAttributes = GetType().GetProperty(propertyName).GetCustomAttributes<MpAffectsBaseAttribute>();
                int affectCount = affectsAttributes.Sum(x => x.FindAndNotifyProperties(this, propertyName));
                if (affectCount == 0 && ThrowOnInvalidPropertyName) {
                    throw new Exception($"{this.GetType().Name}.{propertyName} has affects children with no children found");
                }
            }, System.Windows.Threading.DispatcherPriority.Normal);
        }


        #endregion
    }
}
