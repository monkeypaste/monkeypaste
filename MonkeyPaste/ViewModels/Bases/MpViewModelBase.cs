using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MonkeyPaste {    
    public abstract class MpViewModelBase : INotifyPropertyChanged {
        
        //public static event EventHandler<bool> OnBusyChanged;

        private static Dictionary<string, int> _instanceCountLookup;

        #region Properties

        public virtual object ParentObj { get; protected set; }

        private bool _isBusy = false;
        public bool IsBusy {
            get => _isBusy;
            set {
                SetProperty(ref _isBusy, value);
                if(_isBusy) {
                    MpCursorStack.PushCursor(this, MpCursorType.Waiting);
                } else {
                    MpCursorStack.PopCursor(this);
                }
                //OnBusyChanged?.Invoke(this, _isBusy);
                //MpCursorStack.NotifyAppBusy(_isBusy);
                //MpMessenger.Send(MpMessageType.Busy);
            }
        }
        public bool IsNotBusy => !IsBusy;


        public bool SupressPropertyChangedNotification { get; set; } = false;
        public bool SuprressWriteModelChangedToDatabase { get; set; } = false;

        public virtual bool HasModelChanged { get; set; } = false;

        #endregion

        #region Events

        public event EventHandler ViewModelLoaded;
        protected virtual void OnViewModelLoaded() => ViewModelLoaded?.Invoke(this, EventArgs.Empty);

        #endregion

        #region Constructors

        protected MpViewModelBase() { }

        protected MpViewModelBase(object parent) {
            if (parent == null) {
                string typeStr = this.GetType().Name;
                if (_instanceCountLookup == null) {
                    _instanceCountLookup = new Dictionary<string, int>();
                }
                if (_instanceCountLookup.ContainsKey(typeStr)) {
                    _instanceCountLookup[typeStr]++;
                } else {
                    _instanceCountLookup.Add(typeStr, 1);
                }
            }

            ParentObj = parent;

            MpDb.OnItemAdded += Instance_OnItemAdded;
            MpDb.OnItemUpdated += Instance_OnItemUpdated;
            MpDb.OnItemDeleted += Instance_OnItemDeleted;
            MpDb.SyncAdd += Instance_SyncAdd;
            MpDb.SyncUpdate += Instance_SyncUpdate;
            MpDb.SyncDelete += Instance_SyncDelete;
        }

        #endregion

        public static void PrintInstanceCount() {
            foreach (var kvp in _instanceCountLookup) {
                MpConsole.WriteLine($"'{kvp.Key}': {kvp.Value}");
            }
        }

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

                MpDb.OnItemAdded -= Instance_OnItemAdded;
                MpDb.OnItemUpdated -= Instance_OnItemUpdated;
                MpDb.OnItemDeleted -= Instance_OnItemDeleted;
                MpDb.SyncAdd -= Instance_SyncAdd;
                MpDb.SyncUpdate -= Instance_SyncUpdate;
                MpDb.SyncDelete -= Instance_SyncDelete;
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

        protected virtual void Instance_SyncUpdate(object sender, MpDbSyncEventArgs e) { }

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

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null, [CallerFilePath] string path = null, [CallerMemberName] string memName = null,[CallerLineNumber] int line = 0) {
            if(SupressPropertyChangedNotification) {
                return;
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            //return;

            //MpHelpers.RunOnMainThreadAsync(() => {
            //    //check if property has affects child attribute
            //    var affectsAttributes = GetType().GetProperty(propertyName).GetCustomAttributes<MpAffectsBaseAttribute>();
            //    int affectCount = affectsAttributes.Sum(x => x.FindAndNotifyProperties(this, propertyName));
            //    if (affectCount == 0 && ThrowOnInvalidPropertyName) {
            //        throw new Exception($"{this.GetType().Name}.{propertyName} has affects children with no children found");
            //    }
            //}, System.Windows.Threading.DispatcherPriority.Normal);
        }


        #endregion
    }

    public abstract class MpViewModelBase<P> : MpViewModelBase where P: class {
        #region Private Variables
        #endregion

        #region Properties

        public P Parent {
            get {
                if (ParentObj == null) {
                    return null;
                }
                return (P)ParentObj;
            }
            private set {
                if(Parent != value) {
                    ParentObj = value;
                    OnPropertyChanged(nameof(Parent));
                }
            }
        }

        #endregion

        #region Constructors

        protected MpViewModelBase(P parent) : base(parent) {
            Parent = parent;
        }

        #endregion

    }
}
