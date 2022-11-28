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
        public static bool IsPlatformLoaded { get; protected set; } = false;

        protected static List<MpBootstrappedItemViewModel> _coreItems { get; private set; } = new List<MpBootstrappedItemViewModel>();
        protected static List<MpBootstrappedItemViewModel> _platformItems { get; private set; } = new List<MpBootstrappedItemViewModel>();

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

        public MpNotificationType DialogType => MpNotificationType.Loader;

        #endregion

        public int LoadedCount { get; set; } = 0;

        public async Task StartLoaderAsync() {
            for (int i = 0; i < _coreItems.Count; i++) {
                await LoadItemAsync(_coreItems[i], i);
                while (IsBusy) {
                    await Task.Delay(100);
                }
            }
            while (_coreItems.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }
            await Task.Delay(1000);
            //for (int i = 1; i <= 1000; i++) {
            //    await MpCopyItem.Create(
            //        sourceId: MpPrefViewModel.Instance.ThisAppSourceId,
            //        title: $"{i} This is a test title BOO!",
            //        data: $"This is the content for test {i}",
            //        itemType: MpCopyItemType.Text);
            //}
        }

        public virtual async Task FinishLoaderAsync() {
            // once mw and all mw views are loaded load platform items
            for (int i = 0; i < _platformItems.Count(); i++) {
                await LoadItemAsync(_platformItems[i], i);
                while (IsBusy) {
                    await Task.Delay(100);
                }
            }
            while (_platformItems.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }
        }
        #endregion

        public MpBootstrapperViewModelBase() {
            
        }

        public abstract Task InitAsync();

        protected virtual void CreateLoaderItems() {
            _coreItems.AddRange(
                new List<MpBootstrappedItemViewModel>() {
                    new MpBootstrappedItemViewModel(this,typeof(MpConsole)),
                    new MpBootstrappedItemViewModel(this,typeof(MpTempFileManager)),
                    new MpBootstrappedItemViewModel(this,typeof(MpDb)),
                    //new MpBootstrappedItemViewModel(this,typeof(MpDataModelProvider)),
                    new MpBootstrappedItemViewModel(this,typeof(MpMasterTemplateModelCollectionViewModel)),
                    new MpBootstrappedItemViewModel(this,typeof(MpPluginLoader))
                });
        }

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



