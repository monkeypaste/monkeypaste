using Avalonia.Threading;
using MonkeyPaste.Common;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using static Org.BouncyCastle.Bcpg.Attr.ImageAttrib;

namespace MonkeyPaste.Avalonia {
    public class MpAppClipboardFormatInfoCollectionViewModel :
        MpViewModelBase<MpAvAppViewModel> {
        #region Private Variables

        //private static readonly MpClipboardFormatType[] _DefaultFormats = new MpClipboardFormatType[] {
        //    MpClipboardFormatType.Text,
        //    MpClipboardFormatType.Html,
        //    MpClipboardFormatType.Rtf,
        //    MpClipboardFormatType.Csv,
        //    MpClipboardFormatType.Bitmap,
        //    MpClipboardFormatType.FileDrop
        //};


        #endregion

        #region Statics
        #endregion

        #region Properties

        #region View Models

        public ObservableCollection<MpAppClipboardFormatInfoViewModel> Items { get; set; }
        //public ObservableCollection<MpAppClipboardFormatInfoViewModel> Items =>
        //    new ObservableCollection<MpAppClipboardFormatInfoViewModel>(
        //        Items.Where(x => !x.IsFormatIgnored).OrderByDescending(x=>x.Priority));

        #endregion

        #region State
        public bool IsEmpty =>
            Items == null || Items.Count == 0;

        public bool IsAnyBusy => IsBusy || (Items != null && Items.Any(x => x.IsBusy));

        #endregion

        #endregion

        #region Constructors       

        public MpAppClipboardFormatInfoCollectionViewModel(MpAvAppViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(int appId) {
            IsBusy = true;
            if (Items != null) {

                Items.Clear();
            }

            var overrideInfos = await MpDataModelProvider.GetAppClipboardFormatInfosByAppIdAsync(appId);
            if (overrideInfos.Any()) {
                if (Items == null) {
                    Items = new ObservableCollection<MpAppClipboardFormatInfoViewModel>();
                }
            } else {
                // no overrides null items
                Items = null;
                IsBusy = false;
                return;
            }

            //foreach (var format in MpPortableDataFormats.RegisteredFormats) {
            //    if (overrideInfos.Any(x => x.FormatType == format)) {
            //        continue;
            //    }

            //    var defInfo = new MpAppClipboardFormatInfo() {
            //        AppId = appId,
            //        FormatType = format,
            //        IgnoreFormatValue = overrideInfos.Count
            //    };
            //    overrideInfos.Add(defInfo);
            //}

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

        public async Task<MpAppClipboardFormatInfoViewModel> CreateAppClipboardFormatViewModel(MpAppClipboardFormatInfo ais) {
            MpAppClipboardFormatInfoViewModel aisvm = new MpAppClipboardFormatInfoViewModel(this);
            await aisvm.InitializeAsync(ais);
            return aisvm;
        }

        #endregion

        #region Protected Methods

        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if (e is MpAppClipboardFormatInfo acfi && Parent != null && Parent.AppId == acfi.AppId) {
                Dispatcher.UIThread.Post(async () => {
                    if (Items == null) {
                        Items = new ObservableCollection<MpAppClipboardFormatInfoViewModel>();
                    }
                    var acfvm = Items.FirstOrDefault(x => x.ClipboardFormat == acfi.FormatType);
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
            if (e is MpAppClipboardFormatInfo acfi && Parent != null && Parent.AppId == acfi.AppId) {
                Dispatcher.UIThread.Post(async () => {
                    if (Items == null) {
                        MpDebug.Break("App clipboard format error, out of sync w/ db");
                        Items = new ObservableCollection<MpAppClipboardFormatInfoViewModel>();
                    }
                    var acfvm = Items.FirstOrDefault(x => x.AppInteropSettingId == acfi.Id);
                    if (acfvm == null) {
                        MpDebug.Break("App clipboard format error, out of sync w/ db");
                    } else {
                        await acfvm.InitializeAsync(acfi);
                    }
                });
            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpAppClipboardFormatInfo acfi && Parent != null && Parent.AppId == acfi.AppId) {
                Dispatcher.UIThread.Post(() => {
                    if (Items == null) {
                        MpDebug.Break("App clipboard format error, out of sync w/ db");
                        return;
                    }
                    var acfvm = Items.FirstOrDefault(x => x.AppInteropSettingId == acfi.Id);
                    Items.Remove(acfvm);
                });
            }
        }
        #endregion

        #region Commands

        public ICommand DeleteClipboardFormatTypeCommand => new MpAsyncCommand<object>(
            async (cfaisvmArg) => {
                var cfaisvm = cfaisvmArg as MpAppClipboardFormatInfoViewModel;
                if (cfaisvm == null) {
                    return;
                }
                IsBusy = true;

                Items.Remove(cfaisvm);

                await cfaisvm.AppClipboardFormatInfo.DeleteFromDatabaseAsync();

                OnPropertyChanged(nameof(Items));

                IsBusy = false;

            }, (args) => !IsEmpty);

        public ICommand AddClipboardFormatTypeCommand => new MpCommand(
            async () => {
                IsBusy = true;

                var cfais = await MpAppClipboardFormatInfo.CreateAsync(
                    appId: Parent.AppId);

                var cfaisvm = await CreateAppClipboardFormatViewModel(cfais);
                if (Items == null) {
                    Items = new ObservableCollection<MpAppClipboardFormatInfoViewModel>();
                }
                Items.Add(cfaisvm);

                while (cfaisvm.IsBusy) {
                    await Task.Delay(100);
                }

                OnPropertyChanged(nameof(Items));

                cfaisvm.IsSelected = true;

                IsBusy = false;

            });
        #endregion
    }
}
