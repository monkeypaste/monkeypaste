using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MonkeyPaste.Avalonia {

    public abstract class MpAvViewModelBase :
        //ReactiveObject,
        //IActivatableViewModel,
        MpIHierarchialViewModel,
        INotifyPropertyChanged,
        MpIErrorHandler,
        MpIAsyncObject {

        #region Private Variables

        private List<MpUndoableProperty> _undoables;

        #endregion

        #region Statics

        #endregion

        #region Properties
        //public ViewModelActivator Activator { get; }

        protected List<MpUndoableProperty> Undoable {
            get {
                if (_undoables == null) {
                    _undoables = new List<MpUndoableProperty>();
                }
                return _undoables;
            }
        }

        public virtual object ParentObj { get; set; }

        [JsonIgnore]
        public bool IsBusy { get; set; }
        [JsonIgnore]
        public virtual bool IsLoaded { get; set; }

        public virtual bool UsesIsLoaded { get; }


        [JsonIgnore]
        public bool IgnoreHasModelChanged { get; set; } = false;

        [JsonIgnore]
        public virtual bool HasModelChanged { get; set; }

        [JsonIgnore]
        public bool LogPropertyChangedEvents { get; set; } = false;

        [JsonIgnore]
        public bool SupressPropertyChangedNotification { get; set; } = false;

        [JsonIgnore]
        public object TagObj { get; set; }
        #endregion

        #region Events
        #endregion

        #region Constructors

        protected MpAvViewModelBase() : base() {
            MpDb.OnItemAdded += Instance_OnItemAdded;
            MpDb.OnItemUpdated += Instance_OnItemUpdated;
            MpDb.OnItemDeleted += Instance_OnItemDeleted;
            MpDb.SyncAdd += Instance_SyncAdd;
            MpDb.SyncUpdate += Instance_SyncUpdate;
            MpDb.SyncDelete += Instance_SyncDelete;
        }

        protected MpAvViewModelBase(object parent) : this() {
            ParentObj = parent;

        }

        #endregion

        #region MpIErrorHandler Implementation

        public void HandleError(Exception ex, [CallerMemberName] string callerName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int lineNum = 0) {
            MpConsole.WriteTraceLine(ex.ToString(), null, MpLogLevel.Error, callerName, callerFilePath, lineNum);
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

        ~MpAvViewModelBase() {
            DisposeViewModel(false);
        }

        #endregion

        #region Protected Methods

        protected virtual void NotifyModelChanged(object model, string changedPropName, object newVal) {
            object oldVal = model.GetPropertyValue(changedPropName);
            if (oldVal == newVal) {
                // probably an error, property didn't change
                MpDebug.Break();
            }
            model.SetPropertyValue(changedPropName, newVal);
            HasModelChanged = true;
#if DEBUG
            //MpConsole.WriteLine($"View Model '{this}' Model has changed (writing to db). Property: '{changedPropName}'{Environment.NewLine}OldVal:{Environment.NewLine}'{oldVal}'{Environment.NewLine}NewVal:{Environment.NewLine}'{newVal}'{Environment.NewLine}");
#else
            //MpConsole.WriteLine($"View Model '{this}' Model has changed (writing to db). Property: '{changedPropName}'{Environment.NewLine}OldLen:{Environment.NewLine}'{oldVal.ToStringOrEmpty().Length}'{Environment.NewLine}NewLen:{Environment.NewLine}'{newVal.ToStringOrEmpty().Length}'{Environment.NewLine}");
#endif
        }

        #region Undo/Redo
        /// <summary>
        /// Add an item to the undoable list.
        /// </summary>
        /// <param name="property">The property change.</param>
        /// <param name="oldValue">The original paramValue.</param>
        /// <param name="newValue">The updated paramValue.</param>
        protected void AddUndo(object oldValue, object newValue, string name = "", [CallerMemberName] string property = "") {
            AddUndo(this, property, oldValue, newValue, string.IsNullOrEmpty(name) ? property : name);
        }

        /// <summary>
        /// Add an item to the undoable list.
        /// </summary>
        /// <param name="instance">The instance to add the undoable item against.</param>
        /// <param name="property">The property change.</param>
        /// <param name="oldValue">The original paramValue.</param>
        /// <param name="newValue">The updated paramValue.</param>
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


        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null, [CallerFilePath] string path = null, [CallerMemberName] string memName = null, [CallerLineNumber] int line = 0) {
            if (SupressPropertyChangedNotification ||
                PropertyChanged == null ||
                propertyName == null) {
                return;
            }
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));

            if (LogPropertyChangedEvents) {
                MpConsole.WriteLine($"{this} {propertyName} => {this.GetPropertyValue(propertyName)?.ToString()}");
            }
        }



        #endregion
    }

    public abstract class MpAvViewModelBase<P> : MpAvViewModelBase where P : class {
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
                if (Parent != value) {
                    ParentObj = value;
                    OnPropertyChanged(nameof(Parent));
                }
            }
        }



        #endregion

        #region Constructors

        protected MpAvViewModelBase(P parent) : base(parent) {
            Parent = parent;
        }

        #endregion

    }
}
