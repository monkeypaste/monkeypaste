using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public abstract class MpBootstrapperViewModelBase : 
        MpViewModelBase, 
        MpIProgressLoader {
        #region Statics

        public static bool IsLoaded { get; protected set; } = false;

        protected static List<MpBootstrappedItemViewModel> _items = new List<MpBootstrappedItemViewModel>();

        #endregion

        #region Properties

        #region MpIProgressLoader Implementation

        public string IconResourceKey => MpBase64Images.AppIcon;

        public string Title { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public string Detail {
            get => $"{(int)(PercentLoaded * 100.0)}%";
            set => throw new NotImplementedException();
        }

        public double PercentLoaded => (double)LoadedCount / (double)_items.Count;
        //public double PercentLoaded { get; set; }

        public MpNotificationDialogType DialogType => MpNotificationDialogType.StartupLoader;

        #endregion

        public int LoadedCount { get; set; } = 0;


        #endregion

        public MpBootstrapperViewModelBase(MpIPlatformWrapper niw) {
            MpPlatformWrapper.Init(niw);
            _items.AddRange(
                new List<MpBootstrappedItemViewModel>() {
                    new MpBootstrappedItemViewModel(this,typeof(MpConsole)),
                    new MpBootstrappedItemViewModel(this,typeof(MpCursor),niw.Cursor),
                    new MpBootstrappedItemViewModel(this,typeof(MpPreferences),niw.PreferenceIO),
                    new MpBootstrappedItemViewModel(this,typeof(MpTempFileManager)),
                    new MpBootstrappedItemViewModel(this,typeof(MpDb),niw.DbInfo),
                    new MpBootstrappedItemViewModel(this,typeof(MpDataModelProvider),niw.QueryInfo),
                    new MpBootstrappedItemViewModel(this,typeof(MpMasterTemplateModelCollectionViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpPluginLoader)),
                    new MpBootstrappedItemViewModel(this,typeof(MpPortableDataFormats),niw.DataObjectRegistrar)
                });
        }

        public abstract Task Init();

        protected async Task LoadItem(MpBootstrappedItemViewModel item, int index) {
            await MpPlatformWrapper.Services.MainThreadMarshal.RunOnMainThread(async() => {
                MpConsole.WriteLine("Loading " + item.Label + " at idx: " + index);

                var lnvm = MpNotificationCollectionViewModel.Instance.Notifications.FirstOrDefault(x => x is MpLoaderNotificationViewModel);
                if (lnvm == null) {
                    // NOTE this occurs when warnings exist and loader is finished
                    return;
                }

                

                await item.LoadItemAsync();

                //PercentLoaded = (double)(index + 1) / (double)_items.Count;

                LoadedCount++;
                OnPropertyChanged(nameof(PercentLoaded));

                OnPropertyChanged(nameof(Detail));

                Body = string.IsNullOrWhiteSpace(item.Label) ? Body : item.Label;

                int dotCount = index % 4;
                Title = "LOADING";
                for (int i = 0; i < dotCount; i++) {
                    Title += ".";
                }
            });
        }

        //protected void ReportItemLoading(MpBootstrappedItemViewModel item, int index) {
        //    MpConsole.WriteLine("Loading " + item.Label + " at idx: " + index);

        //    var lnvm = MpNotificationCollectionViewModel.Instance.Notifications.FirstOrDefault(x => x is MpLoaderNotificationViewModel);
        //    if (lnvm == null) {
        //        // NOTE this occurs when warnings exist and loader is finished
        //        return;
        //    }

        //    PercentLoaded = (double)(index + 1) / (double)_items.Count;

        //    OnPropertyChanged(nameof(Detail));

        //    Body = string.IsNullOrWhiteSpace(item.Label) ? Body : item.Label;

        //    int dotCount = index % 4;
        //    Title = "LOADING";
        //    for (int i = 0; i < dotCount; i++) {
        //        Title += ".";
        //    }
        //}
    }
}



