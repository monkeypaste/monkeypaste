using Avalonia.Threading;
using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvAppOleFormatInfoCollectionViewModel :
        MpAvViewModelBase<MpAvAppViewModel> {
        #region Private Variables
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region View Models

        public ObservableCollection<MpAvAppOleFormatInfoViewModel> Items { get; set; }
        public ObservableCollection<MpAvAppOleFormatInfoViewModel> AllFormatsForThisApp { get; set; }
        #endregion

        #region State

        public bool HasCustomInfo =>
            !IsEmpty;
        public bool IsEmpty =>
            Items == null || Items.Count == 0;

        public bool IsAnyBusy => IsBusy || (Items != null && Items.Any(x => x.IsBusy));

        #endregion

        #endregion

        #region Constructors       

        public MpAvAppOleFormatInfoCollectionViewModel(MpAvAppViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(int appId) {
            IsBusy = true;
            if (Items != null) {

                Items.Clear();
            }

            var overrideInfos = await MpDataModelProvider.GetAppOleFormatInfosByAppIdAsync(appId);
            if (overrideInfos.Any()) {
                if (Items == null) {
                    Items = new ObservableCollection<MpAvAppOleFormatInfoViewModel>();
                }
            } else {
                // no overrides null items
                Items = null;
                IsBusy = false;
                return;
            }
            foreach (var ais in overrideInfos.OrderBy(x => x.IgnoreFormatValue)) {
                var aisvm = await CreateAppClipboardFormatViewModel(ais);
                Items.Add(aisvm);
            }

            while (Items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }

            OnPropertyChanged(nameof(Items));

            IsBusy = false;
        }

        public async Task<MpAvAppOleFormatInfoViewModel> CreateAppClipboardFormatViewModel(MpAppOleFormatInfo ais) {
            MpAvAppOleFormatInfoViewModel aisvm = new MpAvAppOleFormatInfoViewModel(this);
            await aisvm.InitializeAsync(ais);
            return aisvm;
        }

        public async Task<MpAvAppOleFormatInfoViewModel> CreateOleFormatInfoViewModelByPresetAsync(MpAvClipboardFormatPresetViewModel cfpvm) {

            // NOTE ignoreFormat ignored for create, update after (but before adding)
            // TODO this and drop widget save preset do same thing, should combine...
            MpAppOleFormatInfo new_aofi = await MpAppOleFormatInfo.CreateAsync(
                    appId: Parent.AppId,
                    format: cfpvm.ClipboardFormat.clipboardName,
                    formatInfo: cfpvm.GetPresetParamJson(),
                    writerPresetId: cfpvm.PresetId);

            var aofivm = await CreateAppClipboardFormatViewModel(new_aofi);

            while (aofivm == null) {
                await Task.Delay(100);
                aofivm = GetAppOleFormatInfoByFormatPreset(cfpvm);
            }
            return aofivm;

        }
        public bool IsFormatEnabledByPreset(MpAvClipboardFormatPresetViewModel cfpvm) {
            return IsEmpty ? cfpvm.IsEnabled : GetAppOleFormatInfoByFormatPreset(cfpvm) != null;
        }
        #endregion

        #region Protected Methods

        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if (e is MpAppOleFormatInfo acfi && Parent != null && Parent.AppId == acfi.AppId) {
                Dispatcher.UIThread.Post(async () => {
                    if (Items == null) {
                        Items = new ObservableCollection<MpAvAppOleFormatInfoViewModel>();
                    }
                    var acfvm = Items.FirstOrDefault(x => x.FormatName == acfi.FormatName);
                    if (acfvm == null) {
                        acfvm = await CreateAppClipboardFormatViewModel(acfi);

                        Items.Add(acfvm);
                    } else {
                        await acfvm.InitializeAsync(acfi);
                    }
                });
            }
        }
        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpAppOleFormatInfo acfi && Parent != null && Parent.AppId == acfi.AppId) {
                Dispatcher.UIThread.Post(async () => {
                    if (Items == null) {
                        MpDebug.Break("App clipboard format error, out of sync w/ db");
                        Items = new ObservableCollection<MpAvAppOleFormatInfoViewModel>();
                    }
                    var acfvm = Items.FirstOrDefault(x => x.AppOleInfoId == acfi.Id);
                    if (acfvm == null) {
                        MpDebug.Break("App clipboard format error, out of sync w/ db");
                    } else {
                        await acfvm.InitializeAsync(acfi);
                    }
                });
            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpAppOleFormatInfo acfi && Parent != null && Parent.AppId == acfi.AppId) {
                Dispatcher.UIThread.Post(() => {
                    if (Items == null) {
                        MpDebug.Break("App clipboard format error, out of sync w/ db");
                        return;
                    }
                    var acfvm = Items.FirstOrDefault(x => x.AppOleInfoId == acfi.Id);
                    Items.Remove(acfvm);
                });
            }
        }

        public MpAvAppOleFormatInfoViewModel GetAppOleFormatInfoByFormatPreset(MpAvClipboardFormatPresetViewModel cfpvm) {
            if (cfpvm == null ||
                Items == null) {
                return null;
            }
            return Items.FirstOrDefault(x => x.WriterPresetId == cfpvm.PresetId);
        }


        private async Task CheckInfosAndResetIfDefaultAsync() {
            if (IsEmpty) {
                // already default
                return;
            }

            // compare this apps infos formats/state to def formats/state
            // if not exactly the same then don't treat as default

            IEnumerable<(string, bool)> app_info_format_enabled_lookup =
                Items.Select(x => (x.FormatName, !x.IgnoreFormat));

            IEnumerable<(string, bool)> def_format_enabled_lookup =
                MpAvClipboardHandlerCollectionViewModel.Instance.EnabledWriters
                    .Select(x => (x.ClipboardFormat.clipboardName, true));

            var diffs = app_info_format_enabled_lookup.Difference(def_format_enabled_lookup);
            if (diffs.Any()) {
                // has unique info, leave it be
                return;
            }
            // discard infos
            await Task.WhenAll(Items.Select(x => x.AppOleFormatInfo.DeleteFromDatabaseAsync()));
            await InitializeAsync(Parent.AppId);
        }
        #endregion

        #region Commands

        public MpIAsyncCommand<object> ToggleFormatEnabledCommand => new MpAsyncCommand<object>(
            async (args) => {
                if (args is not MpAvClipboardFormatPresetViewModel cfpvm) {
                    return;
                }
                if (GetAppOleFormatInfoByFormatPreset(cfpvm) is MpAvAppOleFormatInfoViewModel aofivm) {
                    await aofivm.AppOleFormatInfo.DeleteFromDatabaseAsync();
                } else {
                    await CreateOleFormatInfoViewModelByPresetAsync(cfpvm);
                }
            });

        #endregion
    }
}
