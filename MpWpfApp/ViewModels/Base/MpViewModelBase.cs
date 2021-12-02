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

        #region IDisposable Implementation
        // based on http://support.surroundtech.com/thread/memory-management-best-practices-in-wpf/
        // and https://web.archive.org/web/20200720045029/https://docs.microsoft.com/en-us/archive/blogs/jgoldb/finding-memory-leaks-in-wpf-based-applications
        
        public virtual void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            // Release unmanaged memory
            if (disposing) {
                // Release other objects
                IsBusy = false;

                MpDb.Instance.OnItemAdded -= Instance_OnItemAdded;
                MpDb.Instance.OnItemUpdated -= Instance_OnItemUpdated;
                MpDb.Instance.OnItemDeleted -= Instance_OnItemDeleted;
                MpDb.Instance.SyncAdd -= Instance_SyncAdd;
                MpDb.Instance.SyncUpdate -= Instance_SyncUpdate;
                MpDb.Instance.SyncDelete -= Instance_SyncDelete;
            }
        }

        ~MpViewModelBase() {
            Dispose(false);
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
            return;

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
