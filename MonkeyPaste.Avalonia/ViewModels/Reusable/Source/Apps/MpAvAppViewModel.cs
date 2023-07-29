using Avalonia.Threading;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvAppViewModel :
        MpViewModelBase<MpAvAppCollectionViewModel>,
        MpIHoverableViewModel,
        MpIFilterMatch,
        MpAvIKeyGestureViewModel,
        MpIIsValueEqual<MpAvAppViewModel>
        //MpISourceItemViewModel 
        {
        #region Interfaces

        #region MpIFilterMatch Implementation
        bool MpIFilterMatch.IsFilterMatch(string filter) {
            if (string.IsNullOrEmpty(filter)) {
                return true;
            }
            return
                AppName.ToLower().Contains(filter.ToLower()) ||
                AppPath.ToLower().Contains(filter.ToLower());
        }

        #endregion

        #region MpIIsValueEqual Implementation

        public bool IsValueEqual(MpAvAppViewModel oavm) {
            if (oavm == null) {
                return false;
            }
            return
                AppPath.ToLower() == oavm.AppPath.ToLower() &&
                UserDeviceId == oavm.UserDeviceId;
        }
        #endregion

        #region MpAvIKeyGestureViewModel Implementation
        public ObservableCollection<MpAvShortcutKeyGroupViewModel> KeyGroups =>
           PasteShortcutViewModel.KeyGroups;

        #endregion

        #endregion

        #region Properties

        #region View Models
        public MpAppClipboardFormatInfoCollectionViewModel ClipboardFormatInfos { get; set; }

        public MpAvPasteShortcutViewModel PasteShortcutViewModel { get; set; }
        #endregion

        #region Appearance

        public string IconBase64 {
            get {
                if (MpAvIconCollectionViewModel.Instance.IconViewModels.FirstOrDefault(x => x.IconId == IconId) is MpAvIconViewModel ivm) {
                    return ivm.IconBase64;
                }
                return string.Empty;
            }
        }
        #endregion

        #region State

        // NOTE IsNew is used when adding app from dialog
        // but app already exists for add paste shortcut logic to remove when was new if canceled
        public bool IsNew { get; set; }
        public bool IsThisApp =>
            Parent != null && Parent.ThisAppViewModel == this;


        public bool IsSelected {
            get {
                if (Parent == null) {
                    return false;
                }
                return Parent.SelectedItem == this;
            }
        }

        public DateTime LastSelectedDateTime { get; set; }

        public bool IsHovering { get; set; }

        public bool IsAnyBusy => IsBusy || ClipboardFormatInfos.IsBusy;

        #endregion

        #region Model

        public List<string> ArgumentList {
            get {
                if (App == null) {
                    return null;
                }
                return App.Arguments.SplitNoEmpty(" ").ToList();
            }
        }

        public int UserDeviceId {
            get {
                if (App == null) {
                    return 0;
                }
                return App.UserDeviceId;
            }
        }

        public int AppId {
            get {
                if (App == null) {
                    return 0;
                }
                return App.Id;
            }
        }

        public int IconId {
            get {
                if (App == null) {
                    return 0;
                }
                return App.IconId;
            }
        }

        public string AppPath {
            get {
                if (App == null) {
                    return string.Empty;
                }
                return App.AppPath;
            }
        }

        public string AppName {
            get {
                if (App == null) {
                    return string.Empty;
                }
                return App.AppName;
            }
        }

        public bool IsRejected {
            get {
                if (App == null) {
                    return false;
                }
                return App.IsAppRejected;
            }
            set {
                if (App != null && App.IsAppRejected != value) {
                    App.IsAppRejected = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(IsRejected));
                    OnPropertyChanged(nameof(App));
                }
            }
        }

        public bool IsSubRejected => IsRejected;

        public MpApp App { get; set; }

        #endregion

        #endregion

        #region Constructors
        public MpAvAppViewModel() : this(null) { }

        public MpAvAppViewModel(MpAvAppCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpAppViewModel_PropertyChanged;
            ClipboardFormatInfos = new MpAppClipboardFormatInfoCollectionViewModel(this);
            PasteShortcutViewModel = new MpAvPasteShortcutViewModel(this);
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpApp app) {
            IsBusy = true;

            App = app;

            OnPropertyChanged(nameof(IconId));

            await ClipboardFormatInfos.InitializeAsync(AppId);

            MpAppPasteShortcut aps = await MpDataModelProvider.GetAppPasteShortcutAsync(AppId);
            await PasteShortcutViewModel.InitializeAsync(aps);

            while (ClipboardFormatInfos.IsAnyBusy ||
                    PasteShortcutViewModel.IsBusy) {
                await Task.Delay(100);
            }

            OnPropertyChanged(nameof(KeyGroups));

            IsBusy = false;
        }

        public async Task<bool> VerifyRejectAsync() {
            bool rejectContent = false;

            var clipsFromApp = await MpDataModelProvider.GetCopyItemsBySourceTypeAndIdAsync(MpTransactionSourceType.App, AppId);

            if (clipsFromApp != null && clipsFromApp.Count > 0) {
                var result = await Mp.Services.PlatformMessageBox.ShowYesNoCancelMessageBoxAsync(
                    title: $"Remove associated clips?",
                    message: $"Would you also like to remove all clips from '{AppName}'",
                    iconResourceObj: IconId);
                if (result.IsNull()) {
                    // flag as cancel so cmd will untoggle reject
                    return false;
                }
                rejectContent = result.IsTrue();
            }
            if (!rejectContent) {
                return true;
            }

            IsBusy = true;
            await Task.WhenAll(clipsFromApp.Select(x => x.DeleteFromDatabaseAsync()));
            IsBusy = false;
            return true;
        }

        public MpPortableProcessInfo ToProcessInfo() {
            return new MpPortableProcessInfo() {
                ProcessPath = AppPath,
                ApplicationName = AppName,
                MainWindowIconBase64 = IconBase64,
                ArgumentList = ArgumentList
            };
        }
        public override string ToString() {
            if (App == null) {
                return base.ToString();
            }
            return App.ToString();
        }

        #endregion

        #region Protected Methods

        #region Db Event Handlers

        #endregion

        #endregion

        #region Private Methods
        private void MpAppViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    if (IsSelected) {
                        ClipboardFormatInfos.OnPropertyChanged(nameof(ClipboardFormatInfos.Items));
                        if (PasteShortcutViewModel != null) {
                            PasteShortcutViewModel.OnPropertyChanged(nameof(PasteShortcutViewModel.PasteCmdKeyString));
                        }
                        ClipboardFormatInfos.OnPropertyChanged(nameof(ClipboardFormatInfos.Items));
                    }
                    break;
                case nameof(HasModelChanged):
                    if (HasModelChanged) {
                        Task.Run(async () => {
                            await App.WriteToDatabaseAsync();
                            HasModelChanged = false;
                        });
                    }

                    break;
            }
        }
        #endregion

        #region Commands




        //public ICommand AssignPasteShortcutCommand => new MpCommand(
        //   () => {
        //       ShowAssignDialogAsync().FireAndForgetSafeAsync(this);
        //   });


        #endregion
    }
}
