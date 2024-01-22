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

        #endregion

        #region State
        // NOTE Default means no special settings for this app,
        // uses whatever presets and params are enabled in cb sidebar
        public bool IsCustom =>
            Parent != null && Parent.HasCustomOle;

        public bool IsDefault {
            get {
                if (!IsCustom) {
                    return true;
                }
                return IsReaderDefault && IsWriterDefault;

            }
        }
        public bool IsReaderDefault {
            get {
                if (!IsCustom) {
                    return true;
                }
                return
                    !Readers
                    .Select(x => x.PresetId)
                    .Difference(
                        MpAvClipboardHandlerCollectionViewModel.Instance
                        .EnabledReaders
                        .Select(x => x.PresetId))
                    .Any();
            }
        }
        public bool IsWriterDefault {
            get {
                if (!IsCustom) {
                    return true;
                }
                return
                    !Writers
                    .Select(x => x.PresetId)
                    .Difference(
                        MpAvClipboardHandlerCollectionViewModel.Instance
                        .EnabledWriters
                        .Select(x => x.PresetId))
                    .Any();
            }
        }

        // NOTE NoOp is primarily used to differentiate
        // a default app with one that has NO formats to read/write repectively
        // primarily used during deselect all but can also be intended i guess
        public bool IsAllNoOp =>
            !IsDefault && Items.Count == 0;
        public bool IsReadersOnlyNoOp =>
            IsCustom && Readers.Count == 0;

        public bool IsWritersOnlyNoOp =>
            IsCustom && Writers.Count == 0;

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

        public async Task<MpAvAppOlePresetViewModel> AddAppOlePresetViewModelByPresetIdAsync(int presetId) {
            if (IsDefault) {
                await CreateDefaultInfosCommand.ExecuteAsync();
            }

            if (Items.FirstOrDefault(x => x.PresetId == presetId) is { } dup_aofivm) {
                return dup_aofivm;
            }
            // NOTE ignoreFormat ignored for create, update after (but before adding)
            // TODO this and drop widget save preset do same thing, should combine...
            MpAppOlePreset new_aofi = await MpAppOlePreset.CreateAsync(
                    appId: Parent.AppId,
                    presetId: presetId);
            var aofivm = await CreateAppOlePresetViewModel(new_aofi);
            Items.Add(aofivm);

            return aofivm;
        }
        public async Task RemoveAppOlePresetViewModelByPresetIdAsync(int presetId) {
            if (Items.FirstOrDefault(x => x.PresetId == presetId) is not MpAvAppOlePresetViewModel aopvm) {
                return;
            }

            await aopvm.AppOlePreset.DeleteFromDatabaseAsync();
            Items.Remove(aopvm);
        }

        public bool IsFormatEnabledByPresetId(int presetId) {
            return Items.Any(x => x.PresetId == presetId);
        }

        public async Task<bool> SetIsEnabledAsync(int preset_id, bool is_enabled) {
            if (preset_id <= 0) {
                return is_enabled;
            }

            if (is_enabled) {
                await AddAppOlePresetViewModelByPresetIdAsync(preset_id);
            } else {
                await RemoveAppOlePresetViewModelByPresetIdAsync(preset_id);
            }
            return IsFormatEnabledByPresetId(preset_id);
        }

        public bool ValidateAppOleInfos() {
            // NOTE unused just for diagnostics
            var dup_readers =
                Readers
                .GroupBy(x => x.ClipboardPresetViewModel.FormatName)
                .Where(x => x.Count() > 1);
            var dup_writers =
                Writers
                .GroupBy(x => x.ClipboardPresetViewModel.FormatName)
                .Where(x => x.Count() > 1);
            bool is_valid = !dup_readers.Any() && !dup_writers.Any();
            if (!is_valid) {
                MpDebug.Break($"Dup formats detected");
            }
            return is_valid;
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
            RefreshOleStateProps();
        }

        private void RefreshOleStateProps() {
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(IsDefault));
            OnPropertyChanged(nameof(IsReaderDefault));
            OnPropertyChanged(nameof(IsWriterDefault));
            OnPropertyChanged(nameof(IsReadersOnlyNoOp));
            OnPropertyChanged(nameof(IsWritersOnlyNoOp));
            OnPropertyChanged(nameof(Readers));
            OnPropertyChanged(nameof(Writers));
        }
        #endregion

        #region Commands

        public MpIAsyncCommand CreateDefaultInfosCommand => new MpAsyncCommand(
            async () => {
                Parent.HasCustomOle = true;
                await Task.Delay(50);
                while (Parent.HasModelChanged) {
                    await Task.Delay(100);
                }

                var default_preset_ids =
                            MpAvClipboardHandlerCollectionViewModel.Instance
                            .EnabledFormats
                            .Select(x => x.PresetId);

                // CRITICAL SECTION
                // when avm becomes non-default and enabled presets are stored 
                // this should only happen ONCE so toggling in parallel is BAD
                foreach (var preset_id in default_preset_ids) {
                    await AddAppOlePresetViewModelByPresetIdAsync(preset_id);
                }
            }, () => {
                return !IsCustom;
            });

        public MpIAsyncCommand RemoveCustomInfosCommand => new MpAsyncCommand(
            async () => {
                Parent.HasCustomOle = false;
                await Task.Delay(50);
                while (Parent.HasModelChanged) {
                    await Task.Delay(100);
                }
                await Task.WhenAll(Items.Select(x => x.AppOlePreset.DeleteFromDatabaseAsync()));
                Items.Clear();
            }, () => {
                return IsCustom;
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
