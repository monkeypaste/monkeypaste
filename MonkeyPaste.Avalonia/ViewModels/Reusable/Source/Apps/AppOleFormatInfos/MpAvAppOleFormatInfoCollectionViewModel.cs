using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Cursor = Avalonia.Input.Cursor;

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

        private MpAvAppOleFormatInfoViewModel GetAppOleFormatInfoByFormatPrset(MpAvClipboardFormatPresetViewModel cfpvm) {
            if (cfpvm == null ||
                Items == null) {
                return null;
            }
            return Items.FirstOrDefault(x => x.FormatName.ToLower() == cfpvm.ClipboardFormat.clipboardName);
        }
        private bool IsFormatEnabled(MpAvClipboardFormatPresetViewModel cfpvm) {
            if (cfpvm == null) {
                return false;
            }
            if (GetAppOleFormatInfoByFormatPrset(cfpvm) is not MpAvAppOleFormatInfoViewModel aofivm) {
                // no custom formats set, use default
                return cfpvm.IsEnabled;
            }

            return !aofivm.IgnoreFormat;
        }

        private async Task<MpAvAppOleFormatInfoViewModel> GetOrCreateOleFormatInfoViewModelAsync(
            MpAvClipboardFormatPresetViewModel cfpvm, bool ignoreFormat) {
            MpAvAppOleFormatInfoViewModel aofivm = null;
            if (GetAppOleFormatInfoByFormatPrset(cfpvm) is MpAvAppOleFormatInfoViewModel cur_aofivm) {
                aofivm = cur_aofivm;
            } else {
                // New item

                // NOTE ignoreFormat ignored for create, update after (but before adding)
                // TODO this and drop widget save preset do same thing, should combine...
                MpAppOleFormatInfo new_aofi = await MpAppOleFormatInfo.CreateAsync(
                        appId: Parent.AppId,
                        format: cfpvm.ClipboardFormat.clipboardName,
                        formatInfo: cfpvm.GetPresetParamJson());
                aofivm = await CreateAppClipboardFormatViewModel(new_aofi);

                if (Items == null) {
                    Items = new ObservableCollection<MpAvAppOleFormatInfoViewModel>();
                }
                Items.Add(aofivm);
            }
            aofivm.IgnoreFormat = true;
            while (aofivm.HasModelChanged) { await Task.Delay(100); }


            return aofivm;

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
                bool will_be_enabled = !IsFormatEnabled(cfpvm);

                await GetOrCreateOleFormatInfoViewModelAsync(cfpvm, will_be_enabled);
                await CheckInfosAndResetIfDefaultAsync();
            });

        public ICommand ShowAppFormatFlyoutMenuCommand => new MpCommand<object>(
            (args) => {
                var appFlyout = new MenuFlyout() {
                    ItemsSource =
                        MpAvClipboardHandlerCollectionViewModel.Instance.SortedAvailableEnabledWriters
                        .Select(x =>
                            new MenuItem() {
                                Cursor = new Cursor(StandardCursorType.Hand),
                                Background = IsFormatEnabled(x) ?
                                    Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeAccent5BgColor.ToString()) :
                                    Brushes.Transparent,
                                Icon = new Image() {
                                    Width = 20,
                                    Height = 20,
                                    Source = MpAvIconSourceObjToBitmapConverter.Instance.Convert(x.IconId, null, null, null) as Bitmap
                                },
                                Header = x.Parent.Title,
                                Command = ToggleFormatEnabledCommand,
                                CommandParameter = x
                            }).ToList()
                };
                Control anchor_control = args as Control;
                MpPoint anchor_offset = null;
                if (anchor_control == null && args is object[] argParts) {
                    anchor_control = argParts[0] as Control;
                    anchor_offset = argParts[1] as MpPoint;
                }
                Flyout.SetAttachedFlyout(anchor_control, appFlyout);
                if (anchor_offset != null) {
                }
                Flyout.ShowAttachedFlyout(anchor_control);
            });
        #endregion
    }
}
