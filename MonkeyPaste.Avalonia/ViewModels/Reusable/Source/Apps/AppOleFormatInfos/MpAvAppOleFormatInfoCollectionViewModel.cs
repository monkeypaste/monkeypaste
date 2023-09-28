using Avalonia.Controls;
using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

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

        public MpAvAppOleFormatInfoCollectionViewModel(MpAvAppViewModel parent) : base(parent) {
            Items.CollectionChanged += Items_CollectionChanged;
        }


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

            if (aofivm.IsNoOpReaderOrWriter) {
                // before adding no op, get any presets for this format

                var to_remove =
                        Items
                        .Where(x =>
                            x.ClipboardPresetViewModel != null &&
                            x.ClipboardPresetViewModel.FormatName == aofivm.ClipboardPresetViewModel.FormatName &&
                            x.ClipboardPresetViewModel.IsReader == aofivm.IsReaderAppPreset)
                        .Distinct();

                // add no op BEFORE removing others so remove doesn't trigger default
                Items.Add(aofivm);

                // remove everything but no op for this format
                foreach (var pmvm in to_remove) {
                    await RemoveAppOlePresetViewModelByPresetIdAsync(pmvm.AppOlePresetId);
                }

            } else {
                Items.Add(aofivm);
                bool is_cur_no_op = aofivm.IsReaderAppPreset ? IsReadersOnlyNoOp : IsWritersOnlyNoOp;
                if (is_cur_no_op) {
                    // remove no op for this format
                    int no_op_id = aofivm.IsReaderAppPreset ? MpAppOlePreset.NO_OP_READER_ID : MpAppOlePreset.NO_OP_WRITER_ID;
                    await RemoveAppOlePresetViewModelByPresetIdAsync(NoOpReader.PresetId);
                }
            }

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

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(IsReadersOnlyNoOp));
            OnPropertyChanged(nameof(IsWritersOnlyNoOp));
            OnPropertyChanged(nameof(Readers));
            OnPropertyChanged(nameof(Writers));
        }
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

        public ICommand ShowOleFormatMenuCommand => new MpCommand<object>(
            (args) => {
                if (Parent == null) {
                    return;
                }
                var c = args as Control;
                string show_type = "full";
                if (c == null && args is object[] argParts) {
                    c = argParts[0] as Control;
                    show_type = argParts[1] as string;
                }


                MpAvAppCollectionViewModel.Instance
                .ShowAppPresetsContextMenuCommand
                .Execute(new object[] {
                    c,
                    new MpPortableProcessInfo() {
                        ProcessPath = Parent.AppPath
                    },
                    MpPoint.Zero,
                    show_type
                });

            });
        #endregion
    }
}
