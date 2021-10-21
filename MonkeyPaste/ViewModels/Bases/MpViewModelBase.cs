using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace MonkeyPaste {
    public abstract class MpViewModelBase1<P> : INotifyPropertyChanged, IDisposable where P : class {
        #region Private Variables
        #endregion

        #region Properties

        private P _parent;
        public P Parent { get; set; }
        //public P Parent {
        //    get {
        //        return _parent;
        //    }
        //    set {
        //        if(_parent != value) {
        //            _parent = value;
        //            OnPropertyChanged(nameof(Parent));
        //        }
        //    }
        //}


        private bool _isBusy = false;

        public bool IsBusy { get; set; }

        public bool? IsItemVisible { get; set; } = true;

        #endregion

        #region Events
        public event EventHandler ViewModelLoaded;
        protected virtual void OnViewModelLoaded() => ViewModelLoaded?.Invoke(this, EventArgs.Empty);
        #endregion

        #region Constructors

        protected MpViewModelBase1(P parent) {
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

        public bool ThrowOnInvalidPropertyName { get; private set; } = false;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            //MpHelpers.Instance.RunOnMainThreadAsync(() => {
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

    public class MpViewModelBase : MpObservableObject {
        #region Private Variables
        #endregion        

        #region Properties

        #region View Models
        //public MpMainShellViewModel MainShellViewModel {
        //    get {
        //        return Application.Current.MainPage.BindingContext as MpMainShellViewModel;
        //    }
        //}
        public MpViewModelBase ParentViewModel { get; set; }
        #endregion

        public MpINavigate Navigation { get; set; } = new MpNavigator();
        public static string User { get; set; }

        
        public bool CanAcceptChildren { get; set; } = true;

        private bool _isLoading;
        public bool IsLoading {
            get {
                return _isLoading;
            }
            set {
                if (_isLoading != value) {
                    _isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }


        private static bool _designMode = false;
        protected bool IsInDesignMode {
            get {
                return _designMode;
            }
        }

        private string _name = string.Empty;
        public string Name {
            get {
                return _name;
            }
            set {
                if (_name != value) {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get {
                return _isBusy;
            }
            set
            {
                if(_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged(nameof(IsBusy));
                }
            }
        }
        public bool IsNotBusy => !IsBusy;

        #endregion

        

        #region Protected Methods
        protected MpViewModelBase() : base() { }

        #endregion

        #region Private methods
        #endregion

        
    }
}
