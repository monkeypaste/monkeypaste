using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace MonkeyPaste {    
    public interface MpIViewModel {

        [JsonIgnore]
        bool IsBusy { get; set; }


        [JsonIgnore]
        bool HasModelChanged { get; set; }

        void OnPropertyChanged(
            [CallerMemberName] string propertyName = null, 
            [CallerFilePath] string path = null, 
            [CallerMemberName] string memName = null, 
            [CallerLineNumber] int line = 0);

        event PropertyChangedEventHandler PropertyChanged;
    }

    public abstract class MpViewModelBase : 
        INotifyPropertyChanged, 
        MpIErrorHandler,
        MpIAsyncObject {

        #region Private Variables

        private List<MpUndoableProperty> _undoables;

        #endregion

        #region Properties
        protected List<MpUndoableProperty> Undoable {
            get {
                if (_undoables == null) {
                    _undoables = new List<MpUndoableProperty>();
                }
                return _undoables;
            }
        }

        public virtual MpDbModelBase Model { get; set; }

        public virtual ObservableCollection<MpDbModelBase> ModelHistory { get; set; }

        public virtual object ParentObj { get; protected set; }

        [JsonIgnore]
        public virtual MpViewModelBase SelfBindingRef => this;

        [JsonIgnore]
        private bool _isBusy = false;

        [JsonIgnore]
        public bool IsBusy {
            get => _isBusy;
            set {
                SetProperty(ref _isBusy, value);
                //if(_isBusy) {
                //    // NOTE normally null should be passed for target but for wait cursor pass viewmodel
                //    MpPlatformWrapper.Services.Cursor.SetCursor(this, MpCursorType.Waiting);
                //} else {
                //    MpPlatformWrapper.Services.Cursor.UnsetCursor(this);
                //}
            }
        }


        [JsonIgnore]
        public bool SupressPropertyChangedNotification { get; set; } = false;

        [JsonIgnore]
        public bool SuprressNextHasModelChangedHandling { get; set; } = false;

        [JsonIgnore]
        public virtual bool HasModelChanged { get; set; } = false;

        [JsonIgnore]
        public bool LogPropertyChangedEvents { get; set; } = false;

        #endregion

        #region Events

        public event EventHandler ViewModelLoaded;
        protected virtual void OnViewModelLoaded() => ViewModelLoaded?.Invoke(this, EventArgs.Empty);

        #endregion

        #region Constructors

        protected MpViewModelBase() { }

        protected MpViewModelBase(object parent) {
            //if (parent == null) {
            //    string typeStr = this.GetType().Name;
            //    if (_instanceCountLookup == null) {
            //        _instanceCountLookup = new Dictionary<string, int>();
            //    }
            //    if (_instanceCountLookup.ContainsKey(typeStr)) {
            //        _instanceCountLookup[typeStr]++;
            //    } else {
            //        _instanceCountLookup.Add(typeStr, 1);
            //    }
            //}

            ParentObj = parent;

            MpDb.OnItemAdded += Instance_OnItemAdded;
            MpDb.OnItemUpdated += Instance_OnItemUpdated;
            MpDb.OnItemDeleted += Instance_OnItemDeleted;
            MpDb.SyncAdd += Instance_SyncAdd;
            MpDb.SyncUpdate += Instance_SyncUpdate;
            MpDb.SyncDelete += Instance_SyncDelete;
        }

        #endregion

        #region MpIErrorHandler Implementation

        public void HandleError(Exception ex) {
            MpConsole.WriteTraceLine(ex);
        }

        #endregion

        #region IDisposable Implementation
        // based on http://support.surroundtech.com/thread/memory-management-best-practices-in-wpf/
        // and https://web.archive.org/web/20200720045029/https://docs.microsoft.com/en-us/archive/blogs/jgoldb/finding-memory-leaks-in-wpf-based-applications

        public virtual void DisposeViewModel() {
            DisposeViewModel(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void DisposeViewModel(bool disposing) {
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
            DisposeViewModel(false);
        }

        #endregion

        #region Protected Methods

        protected void NotifyModelChanged(object model, string changedPropName, object newVal) {
            object oldVal = model.GetPropertyValue(changedPropName);
            if(oldVal == newVal) {
                // probably an error, property didn't change
                Debugger.Break();
            }
            model.SetPropertyValue(changedPropName, newVal);
            HasModelChanged = true;
            MpConsole.WriteLine($"View Model '{this}' Model has changed (writing to db). Property: '{changedPropName}' OldVal: '{oldVal}' NewVal: '{newVal}'");
        }

        #region Undo/Redo
        /// <summary>
        /// Add an item to the undoable list.
        /// </summary>
        /// <param name="instance">The instance to add the undoable item against.</param>
        /// <param name="property">The property change.</param>
        /// <param name="oldValue">The original value.</param>
        /// <param name="newValue">The updated value.</param>
        protected void AddUndo(object oldValue, object newValue, string name = "", [CallerMemberName] string property = "") {
            AddUndo(this, property, oldValue, newValue, string.IsNullOrEmpty(name) ? property : name);
        }

        /// <summary>
        /// Add an item to the undoable list.
        /// </summary>
        /// <param name="instance">The instance to add the undoable item against.</param>
        /// <param name="property">The property change.</param>
        /// <param name="oldValue">The original value.</param>
        /// <param name="newValue">The updated value.</param>
        /// <param name="name">The name of the undo operation.</param>
        protected void AddUndo(object instance, string property, object oldValue, object newValue, string name) {
            Undoable.Add(new MpUndoableProperty(instance, property, oldValue, newValue, name));
        }

        #endregion

        #region Db Events

        protected virtual void Instance_SyncDelete(object sender, MpDbSyncEventArgs e) { }

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

        [JsonIgnore]
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

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null, [CallerFilePath] string path = null, [CallerMemberName] string memName = null,[CallerLineNumber] int line = 0) {
            if(SupressPropertyChangedNotification) {
                return;
            }
            var e = new PropertyChangedEventArgs(propertyName);

            if (LogPropertyChangedEvents) {
                MpConsole.WriteLine($"{this} {e.PropertyName} => {this.GetPropertyValue(e.PropertyName)?.ToString()}");
            }

            PropertyChanged?.Invoke(this, e);
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

        [JsonIgnore]
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
