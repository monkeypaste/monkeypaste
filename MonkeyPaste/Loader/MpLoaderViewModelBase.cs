using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste {

    public abstract class MpLoaderViewModelBase :
        MpViewModelBase,
        MpIStartupState,
        MpIProgressLoaderViewModel {
        #region Private Variables

        #endregion

        #region Statics        
        #endregion

        #region Interfaces

        #region MpIStartupState Implementation

        public DateTime? LoadedDateTime { get; private set; } = null;
        public bool IsCoreLoaded { get; protected set; } = false;
        public bool IsPlatformLoaded { get; protected set; } = false;
        public MpStartupFlags StartupFlags { get; protected set; }
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
            (double)LoadedCount / (double)(Items.Count);

        public MpNotificationType DialogType => MpNotificationType.Loader;

        public bool ShowSpinner =>
            PercentLoaded >= 100;
        #endregion

        #endregion

        #region Properties

        #region View Models
        public List<MpLoaderItemViewModel> BaseItems { get; private set; } = new List<MpLoaderItemViewModel>();
        public List<MpLoaderItemViewModel> CoreItems { get; private set; } = new List<MpLoaderItemViewModel>();
        public List<MpLoaderItemViewModel> PlatformItems { get; private set; } = new List<MpLoaderItemViewModel>();

        public IList<MpLoaderItemViewModel> Items =>
            CoreItems.Union(PlatformItems).ToList();

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
            IsCoreLoaded = true;
            MpConsole.WriteLine("Core load complete");
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



