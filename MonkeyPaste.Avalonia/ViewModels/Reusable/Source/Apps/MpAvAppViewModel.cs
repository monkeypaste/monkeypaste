using Avalonia.Threading;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {

    public class MpAvAppViewModel :
        MpAvViewModelBase<MpAvAppCollectionViewModel>,
        MpIHoverableViewModel,
        MpAvIPulseViewModel,
        MpIFilterMatch,
        MpIIsValueEqual<MpAvAppViewModel> {
        #region Interfaces

        #region MpIFilterMatch Implementation
        bool MpIFilterMatch.IsFilterMatch(string filter) {
            if (string.IsNullOrEmpty(filter)) {
                return true;
            }
            return
                AppName.ToLower().Contains(filter.ToLower()) ||
                AppPath.ToLower().Contains(filter.ToLower()) ||
                OleFormatInfos.Items.Any(x => x.ClipboardPresetViewModel.FormatName.ToLower().Contains(filter.ToLower()));
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

        #endregion

        #region Properties

        #region View Models
        public MpAvAppOleFormatInfoCollectionViewModel OleFormatInfos { get; set; }

        public MpAvAppClipboardShortcutViewModel PasteShortcutViewModel { get; set; }
        public MpAvAppClipboardShortcutViewModel CopyShortcutViewModel { get; set; }
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

        public string AppDisplayName =>
            string.IsNullOrEmpty(AppName) ?
                Path.GetFileNameWithoutExtension(AppPath) :
                AppName;

        #endregion

        #region State
        public bool DoFocusPulse { get; set; }
        public bool HasAnyShortcut =>
            ClipboardShortcutsId > 0;

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

        public bool IsAnyBusy => IsBusy || OleFormatInfos.IsBusy;

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

        public bool HasCustomOle {
            get {
                if (App == null) {
                    return false;
                }
                return App.HasOleFormats;
            }
            set {
                if (HasCustomOle != value) {
                    App.HasOleFormats = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(HasCustomOle));
                }
            }
        }

        public int ClipboardShortcutsId { get; private set; }
        public MpApp App { get; set; }

        #endregion

        #endregion

        #region Constructors
        public MpAvAppViewModel() : this(null) { }

        public MpAvAppViewModel(MpAvAppCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpAppViewModel_PropertyChanged;
            OleFormatInfos = new MpAvAppOleFormatInfoCollectionViewModel(this);
            PasteShortcutViewModel = new MpAvAppClipboardShortcutViewModel(this, false);
            CopyShortcutViewModel = new MpAvAppClipboardShortcutViewModel(this, true);
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpApp app) {
            IsBusy = true;

            App = app;

            OnPropertyChanged(nameof(IconId));

            await OleFormatInfos.InitializeAsync(AppId);

            if (!HasCustomOle) {
                // if formats exist HasCustomOle should be true
                MpDebug.Assert(OleFormatInfos.Items.Count == 0, $"Ole format mismatch for {this}");
            }

            MpAppClipboardShortcuts aps = await MpDataModelProvider.GetAppClipboardShortcutsAsync(AppId);
            ClipboardShortcutsId = aps == null ? 0 : aps.Id;
            await PasteShortcutViewModel.InitializeAsync(aps);
            await CopyShortcutViewModel.InitializeAsync(aps);

            while (OleFormatInfos.IsAnyBusy ||
                    PasteShortcutViewModel.IsBusy ||
                    CopyShortcutViewModel.IsBusy) {
                await Task.Delay(100);
            }

            IsBusy = false;
        }

        public async Task<bool> VerifyAndApplyRejectAsync() {
            bool rejectContent = false;

            var clipsFromApp = await MpDataModelProvider.GetCopyItemsBySourceTypeAndIdAsync(MpTransactionSourceType.App, AppId);

            if (clipsFromApp != null && clipsFromApp.Count > 0) {
                var result = await Mp.Services.PlatformMessageBox.ShowYesNoCancelMessageBoxAsync(
                    title: UiStrings.NtfRejectRemoveClipsTitle,
                    message: string.Format(UiStrings.NtfRejectRemoveClipsBody, clipsFromApp.Count(), AppName),
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
                        OleFormatInfos.OnPropertyChanged(nameof(OleFormatInfos.Items));
                        if (PasteShortcutViewModel != null) {
                            PasteShortcutViewModel.OnPropertyChanged(nameof(PasteShortcutViewModel.ShortcutCmdKeyString));
                        }
                        OleFormatInfos.OnPropertyChanged(nameof(OleFormatInfos.Items));
                    }
                    break;
                case nameof(HasModelChanged):
                    if (HasModelChanged) {
                        Task.Run(async () => {
                            await App.WriteToDatabaseAsync();
                            Dispatcher.UIThread.Post(() => {
                                HasModelChanged = false;
                            });
                        });
                    }

                    break;
                case nameof(DoFocusPulse):
                    if (Parent != null) {
                        Parent.OnPropertyChanged(nameof(Parent.DoSelectedPulse));
                    }
                    MpAvThemeViewModel.Instance.HandlePulse(this);
                    break;
                case nameof(HasCustomOle):
                    if (Parent != null) {
                        Parent.OnPropertyChanged(nameof(Parent.CustomClipboardFormatItems));
                    }
                    break;
            }
        }
        #endregion

        #region Commands

        public MpIAsyncCommand ToggleIsRejectedCommand => new MpAsyncCommand(
            async () => {
                IsRejected = !IsRejected;
                if (IsRejected) {
                    bool was_confirmed = await VerifyAndApplyRejectAsync();
                    if (!was_confirmed) {
                        // canceled from delete content msgbox
                        IsRejected = false;
                    }
                }
            });


        #endregion
    }
}
