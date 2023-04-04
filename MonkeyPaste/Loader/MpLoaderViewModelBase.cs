using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste {

    public interface MpIStartupObjectLocator {
        IEnumerable<object> Items { get; }
    }
    public abstract class MpLoaderViewModelBase :
        MpViewModelBase,
        MpIStartupState,
        MpIProgressLoaderViewModel,
        MpIStartupObjectLocator {
        #region Private Variables

        #endregion


        #region Statics        
        #endregion

        #region Interfaces

        #region MpIStartupObjectLocator Implementation

        IEnumerable<object> MpIStartupObjectLocator.Items => CoreItems.Union(PlatformItems);
        #endregion

        #region MpIStartupState Implementation

        public DateTime? LoadedDateTime { get; private set; } = null;
        public bool IsCoreLoaded { get; protected set; } = false;
        public bool IsPlatformLoaded { get; protected set; } = false;
        public bool IsInitialStartup { get; protected set; } = false;

        #endregion

        #region MpIProgressLoaderViewModel Implementation

        public string IconResourceKey =>
            MpBase64Images.AppIcon;

        public string Title { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public string Detail {
            get => $"{(int)(PercentLoaded * 100.0)}%";
            set => throw new NotImplementedException();
        }

        public double PercentLoaded =>
            (double)LoadedCount / (double)(CoreItems.Count);

        public MpNotificationType DialogType => MpNotificationType.Loader;

        #endregion
        #endregion

        #region Properties

        #region View Models
        public List<MpLoaderItemViewModel> BaseItems { get; private set; } = new List<MpLoaderItemViewModel>();
        public List<MpLoaderItemViewModel> CoreItems { get; private set; } = new List<MpLoaderItemViewModel>();
        public List<MpLoaderItemViewModel> PlatformItems { get; private set; } = new List<MpLoaderItemViewModel>();


        #endregion

        #region State
        public bool IS_PARALLEL_LOADING_ENABLED =>
            false;

        public int LoadedCount { get; set; } = 0;

        #endregion

        #endregion

        #region Constructors
        public MpLoaderViewModelBase() { }

        #endregion

        #region Public Methhods
        public abstract Task CreatePlatformAsync(DateTime startup_datetime);

        public abstract Task InitAsync();

        public virtual async Task BeginLoaderAsync() {
            await LoadItemsAsync(CoreItems);
            MpConsole.WriteLine("Core load complete");
            await Task.Delay(1000);
        }

        public virtual async Task FinishLoaderAsync() {
            // once mw and all mw views are loaded load platform items
            await LoadItemsAsync(PlatformItems);
            MpConsole.WriteLine("Platform load complete");
            LoadedDateTime = DateTime.Now;
        }

        #endregion

        #region Protected Methods

        protected abstract void CreateLoaderItems();

        protected abstract Task LoadItemAsync(MpLoaderItemViewModel item, int index, bool affectsCount);

        protected async Task LoadItemsAsync(List<MpLoaderItemViewModel> items, bool affectsCount = true) {
            if (IS_PARALLEL_LOADING_ENABLED) {
                await LoadItemsParallelAsync(items, affectsCount);
            } else {
                await LoadItemsSequentialAsync(items, affectsCount);
            }
        }
        #endregion

        #region Private Methods
        private async Task LoadItemsParallelAsync(List<MpLoaderItemViewModel> items, bool affectsCount) {
            await Task.WhenAll(items.Select((x, idx) => LoadItemAsync(x, idx, affectsCount)));
            while (items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }
        }

        private async Task LoadItemsSequentialAsync(List<MpLoaderItemViewModel> items, bool affectsCount) {
            for (int i = 0; i < items.Count; i++) {
                await LoadItemAsync(items[i], i, affectsCount);
                while (IsBusy) {
                    await Task.Delay(100);
                }
            }
            while (items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }
        }

        #endregion

    }
}



