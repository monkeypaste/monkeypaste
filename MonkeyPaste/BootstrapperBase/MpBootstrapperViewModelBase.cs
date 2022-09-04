using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MonkeyPaste {
    public abstract class MpBootstrapperViewModelBase : 
        MpViewModelBase, 
        MpIProgressLoader {
        #region Statics

        public static bool IsCoreLoaded { get; protected set; } = false;

        protected static List<MpBootstrappedItemViewModel> _coreItems = new List<MpBootstrappedItemViewModel>();

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

        public double PercentLoaded => (double)LoadedCount / (double)_coreItems.Count;

        public MpNotificationDialogType DialogType => MpNotificationDialogType.StartupLoader;

        #endregion

        public int LoadedCount { get; set; } = 0;


        #endregion

        public MpBootstrapperViewModelBase() {
            _coreItems.AddRange(
                new List<MpBootstrappedItemViewModel>() {
                    new MpBootstrappedItemViewModel(this,typeof(MpConsole)),
                    new MpBootstrappedItemViewModel(this,typeof(MpCursor)),
                    new MpBootstrappedItemViewModel(this,typeof(MpTempFileManager)),
                    new MpBootstrappedItemViewModel(this,typeof(MpDb)),
                    new MpBootstrappedItemViewModel(this,typeof(MpDataModelProvider)),
                    new MpBootstrappedItemViewModel(this,typeof(MpMasterTemplateModelCollectionViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpPluginLoader)),
                    new MpBootstrappedItemViewModel(this,typeof(MpPortableDataFormats),MpPlatformWrapper.Services.DataObjectRegistrar)
                });
        }

        public abstract Task InitAsync();

        protected virtual async Task LoadItemAsync(MpBootstrappedItemViewModel item, int index) {
            IsBusy = true;
            await MpPlatformWrapper.Services.MainThreadMarshal.RunOnMainThreadAsync(async () => {
                var sw = Stopwatch.StartNew();

                await item.LoadItemAsync();
                sw.Stop();

                LoadedCount++;
                MpConsole.WriteLine("Loaded " + item.Label + " at idx: " + index + " Load Count: " + LoadedCount + " Load Percent: " + PercentLoaded + " Time(ms): " + sw.ElapsedMilliseconds);

                OnPropertyChanged(nameof(PercentLoaded));

                OnPropertyChanged(nameof(Detail));

                Body = string.IsNullOrWhiteSpace(item.Label) ? Body : item.Label;

                int dotCount = index % 4;
                Title = "LOADING";
                for (int i = 0; i < dotCount; i++) {
                    Title += ".";
                }

            });

            IsBusy = false;
        }
    }
}



