using Avalonia.Threading;
using MonkeyPaste.Common;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvAppViewModel :
        MpViewModelBase<MpAvAppCollectionViewModel>,
        MpISelectableViewModel,
        MpIHoverableViewModel,
        MpIIsValueEqual<MpAvAppViewModel>
        //MpISourceItemViewModel 
        {
        #region Interfaces

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

        #endregion

        #region Properties

        #region View Models
        public MpAppClipboardFormatInfoCollectionViewModel ClipboardFormatInfos { get; set; }

        public MpPasteShortcutViewModel PasteShortcutViewModel { get; set; }
        #endregion

        #region Appearance

        #endregion

        #region State

        public bool IsSelected { get; set; }

        public DateTime LastSelectedDateTime { get; set; }

        public bool IsHovering { get; set; }

        public bool IsAnyBusy => IsBusy || ClipboardFormatInfos.IsBusy;

        #endregion

        #region Model

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
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpApp app) {
            IsBusy = true;


            App = app;

            OnPropertyChanged(nameof(IconId));

            await ClipboardFormatInfos.InitializeAsync(AppId);

            MpAppPasteShortcut aps = await MpDataModelProvider.GetAppPasteShortcutAsync(AppId);
            if (aps != null) {
                PasteShortcutViewModel = new MpPasteShortcutViewModel(this);
                await PasteShortcutViewModel.InitializeAsync(aps);
            }

            while (ClipboardFormatInfos.IsAnyBusy || (PasteShortcutViewModel != null && PasteShortcutViewModel.IsBusy)) {
                await Task.Delay(100);
            }

            IsBusy = false;
        }

        public async Task RejectApp() {
            IsBusy = true;
            bool wasCanceled = false;

            var clipsFromApp = await MpDataModelProvider.GetCopyItemsBySourceTypeAndIdAsync(MpTransactionSourceType.App, AppId);

            if (clipsFromApp != null && clipsFromApp.Count > 0) {
                //MessageBoxResult confirmExclusionResult = MessageBox.Show("Would you also like to remove all clips from '" + AppName + "'", "Remove associated clips?", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Yes, MessageBoxOptions.DefaultDesktopOnly);
                //if (confirmExclusionResult == MessageBoxResult.Cancel) {
                //    wasCanceled = true;
                //}
            }
            if (wasCanceled) {
                IsBusy = false;
                return;
            }

            await Task.WhenAll(clipsFromApp.Select(x => x.DeleteFromDatabaseAsync()));
            IsBusy = false;
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

                        Parent.OnPropertyChanged(nameof(Parent.SelectedItem));
                        //CollectionViewSource.GetDefaultView(ClipboardFormatInfos.Items).Refresh();
                        ClipboardFormatInfos.OnPropertyChanged(nameof(ClipboardFormatInfos.Items));
                    }
                    break;
                case nameof(IsRejected):
                    if (IsRejected) {
                        Dispatcher.UIThread.Post(async () => { await RejectApp(); });
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

        #endregion
    }
}
