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

        public ObservableCollection<MpAvAppOlePresetViewModel> Items { get; } = new ObservableCollection<MpAvAppOlePresetViewModel>();
        public IList<MpAvAppOlePresetViewModel> Readers =>
            Items.Where(x => x.IsReaderAppPreset).ToList();

        public IList<MpAvAppOlePresetViewModel> Writers =>
            Items.Where(x => x.IsWriterAppPreset).ToList();

        MpAvAppOlePresetViewModel NoOpReader =>
            Readers.FirstOrDefault(x => x.IsReaderNoOp);

        MpAvAppOlePresetViewModel NoOpWriter =>
            Writers.FirstOrDefault(x => x.IsWriterNoOp);
        #endregion

        #region State
        // NOTE Default means no special settings for this app,
        // uses whatever presets and params are enabled in cb sidebar
        public bool IsDefault =>
            IsReaderDefault && IsWriterDefault;
        public bool IsReaderDefault =>
            Readers.Count == 0;
        public bool IsWriterDefault =>
            Writers.Count == 0;

        // NOTE NoOp is primarily used to differentiate
        // a default app with one that has NO formats to read/write repectively
        // primarily used during deselect all but can also be intended i guess
        public bool IsReadersOnlyNoOp =>
            Readers.Count == 1 &&
            NoOpReader != null;

        public bool IsWritersOnlyNoOp =>
            Writers.Count == 1 &&
            NoOpWriter != null;
        public bool HasCustomReaders =>
            !IsReadersOnlyNoOp &&
            Readers.Any();

        public bool HasCustomWriters =>
            !IsWritersOnlyNoOp &&
            Writers.Any();

        public bool IsAnyBusy =>
            IsBusy || Items.Any(x => x.IsBusy);

        #endregion

        #endregion

        #region Constructors       

        public MpAvAppOleFormatInfoCollectionViewModel(MpAvAppViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(int appId) {
            IsBusy = true;
            Items.Clear();

            var overrideInfos = await MpDataModelProvider.GetAppOlePresetsByAppIdAsync(appId);
            foreach (var ais in overrideInfos) {
                var aisvm = await CreateAppOlePresetViewModel(ais);
                Items.Add(aisvm);
            }

            while (Items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }

            OnPropertyChanged(nameof(Items));

            IsBusy = false;
        }

        public async Task<MpAvAppOlePresetViewModel> CreateAppOlePresetViewModel(MpAppOlePreset ais) {
            MpAvAppOlePresetViewModel aisvm = new MpAvAppOlePresetViewModel(this);
            await aisvm.InitializeAsync(ais);
            return aisvm;
        }

        public async Task RemoveAppOlePresetViewModelByPresetIdAsync(int presetId) {
            if (Items.FirstOrDefault(x => x.PresetId == presetId) is not MpAvAppOlePresetViewModel aopvm) {
                return;
            }
            // NOTE if this is the last info for the app and it is NOT
            // the no op then it will NEED the no op.
            // no op is removed by toggling relativeRoot from false in menu cmd
            bool isReader = aopvm.IsReaderAppPreset;
            bool needs_no_op =
                Items
                .Where(x => x.IsReaderAppPreset == isReader && !x.IsNoOpReaderOrWriter)
                .Count() == 1;
            await aopvm.AppOlePreset.DeleteFromDatabaseAsync();
            Items.Remove(aopvm);
            if (needs_no_op) {
                // make sure no op is present for continuing
                await AddAppOlePresetViewModelByPresetIdAsync(isReader ? MpAppOlePreset.NO_OP_READER_ID : MpAppOlePreset.NO_OP_WRITER_ID);
            }
        }
        public async Task<MpAvAppOlePresetViewModel> AddAppOlePresetViewModelByPresetIdAsync(int presetId) {

            // NOTE ignoreFormat ignored for create, update after (but before adding)
            // TODO this and drop widget save preset do same thing, should combine...
            MpAppOlePreset new_aofi = await MpAppOlePreset.CreateAsync(
                    appId: Parent.AppId,
                    presetId: presetId);
            if (Items.FirstOrDefault(x => x.AppOlePresetId == new_aofi.Id) is MpAvAppOlePresetViewModel aopvm) {
                // preset is dup
                MpDebug.Assert(new_aofi.WasDupOnCreate, $"app ole items and db out of sync");
                return aopvm;
            } else {
                MpDebug.Assert(!new_aofi.WasDupOnCreate, $"app ole items and db out of sync");
            }
            var aofivm = await CreateAppOlePresetViewModel(new_aofi);

            if (aofivm.IsReaderAppPreset) {
                if (aofivm.IsReaderNoOp) {
                    MpDebug.Assert(Readers.Count == 0, $"No op reader error, all readers should be removed before adding no op");
                } else if (IsReadersOnlyNoOp) {
                    // remove no op reader
                    await RemoveAppOlePresetViewModelByPresetIdAsync(NoOpReader.PresetId);
                }
            } else {
                if (aofivm.IsWriterNoOp) {
                    MpDebug.Assert(Readers.Count == 0, $"No op writer error, all writers should be removed before adding no op");
                } else if (IsWritersOnlyNoOp) {
                    // remove no op writer
                    await RemoveAppOlePresetViewModelByPresetIdAsync(NoOpWriter.PresetId);
                }
            }
            Items.Add(aofivm);

            return aofivm;

        }
        public bool IsFormatEnabledByPresetId(int presetId) {
            return GetAppOleFormatInfoByPresetId(presetId) != null;
        }
        public MpAvAppOlePresetViewModel GetAppOleFormatInfoByPresetId(int presetId) {
            if (Items == null) {
                return null;
            }
            return Items.FirstOrDefault(x => x.PresetId == presetId);
        }
        #endregion

        #region Protected Methods

        //protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
        //    if (e is not MpAppOlePreset acfi ||
        //        Parent == null ||
        //        Parent.AppId != acfi.AppId) {
        //        return;
        //    }
        //}
        //protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
        //    if (e is MpAppOlePreset acfi && Parent != null && Parent.AppId == acfi.AppId) {
        //        Dispatcher.UIThread.Post(async () => {
        //            if (Items == null) {
        //                MpDebug.Break("App clipboard format error, out of sync w/ db");
        //                Items = new ObservableCollection<MpAvAppOlePresetViewModel>();
        //            }
        //            var acfvm = Items.FirstOrDefault(x => x.AppOlePresetId == acfi.Id);
        //            if (acfvm == null) {
        //                MpDebug.Break("App clipboard format error, out of sync w/ db");
        //            } else {
        //                await acfvm.InitializeAsync(acfi);
        //            }
        //        });
        //    }
        //}

        //protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
        //    if (e is MpAppOlePreset acfi && Parent != null && Parent.AppId == acfi.AppId) {
        //        Dispatcher.UIThread.Post(() => {
        //            if (Items == null) {
        //                MpDebug.Break("App clipboard format error, out of sync w/ db");
        //                return;
        //            }
        //            var acfvm = Items.FirstOrDefault(x => x.AppOlePresetId == acfi.Id);
        //            Items.Remove(acfvm);
        //        });
        //    }
        //}

        #endregion

        #region Private Methods
        #endregion

        #region Commands

        public MpIAsyncCommand<object> ToggleFormatEnabledCommand => new MpAsyncCommand<object>(
            async (args) => {
                int presetId = 0;

                if (args is MpAvClipboardFormatPresetViewModel cfpvm) {
                    presetId = cfpvm.PresetId;
                } else if (args is bool isReaderNoOp) {
                    presetId = isReaderNoOp ? MpAppOlePreset.NO_OP_READER_ID : MpAppOlePreset.NO_OP_WRITER_ID;
                }
                if (GetAppOleFormatInfoByPresetId(presetId) is MpAvAppOlePresetViewModel aofivm) {
                    // remove preset
                    await RemoveAppOlePresetViewModelByPresetIdAsync(presetId);
                } else {
                    //add preset
                    await AddAppOlePresetViewModelByPresetIdAsync(presetId);
                }
            });

        #endregion
    }
}
