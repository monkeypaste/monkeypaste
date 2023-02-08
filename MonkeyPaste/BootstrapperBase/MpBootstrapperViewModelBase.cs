using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MonkeyPaste {

    public interface MpIStartupObjectLocator {
        IEnumerable<object> Items { get; }
    }
    public abstract class MpBootstrapperViewModelBase : 
        MpViewModelBase,
        MpIStartupState,
        MpIProgressLoader,
        MpIStartupObjectLocator {


        #region Statics

        public static bool IS_PARALLEL_LOADING_ENABLED = false;
        public static bool IsCoreLoaded { get; protected set; } = false;
        public static bool IsPlatformLoaded { get; protected set; } = false;

        protected static List<MpBootstrappedItemViewModel> _coreItems { get; private set; } = new List<MpBootstrappedItemViewModel>();
        protected static List<MpBootstrappedItemViewModel> _platformItems { get; private set; } = new List<MpBootstrappedItemViewModel>();

        #endregion

        #region Interfaces

        #region MpIStartupObjectLocator Implementation

        IEnumerable<object> MpIStartupObjectLocator.Items => _coreItems.Union(_platformItems);
        #endregion

        #region MpIStartupState Implementation

        public DateTime? LoadedDateTime { get; private set; } = null;

        #endregion

        #region MpIProgressLoader Implementation

        public string IconResourceKey => 
            MpBase64Images.AppIcon;

        public string Title { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public string Detail {
            get => $"{(int)(PercentLoaded * 100.0)}%";
            set => throw new NotImplementedException();
        }

        public double PercentLoaded => 
            (double)LoadedCount / (double)(_coreItems.Count);

        public MpNotificationType DialogType => MpNotificationType.Loader;

        #endregion
        #endregion

        #region Properties
        public int LoadedCount { get; set; } = 0;

        #endregion

        #region Constructors
        public MpBootstrapperViewModelBase() { }

        #endregion

        #region Public Methhods

        public abstract Task InitAsync();

        public virtual async Task BeginLoaderAsync() {
            await LoadItemsAsync(_coreItems);
            await Task.Delay(1000);
        }

        public virtual async Task FinishLoaderAsync() {
            // once mw and all mw views are loaded load platform items
            await LoadItemsAsync(_platformItems);
            LoadedDateTime = DateTime.Now;
        }

        #endregion

        #region Protected Methods

        protected abstract void CreateLoaderItems();

        protected abstract Task LoadItemAsync(MpBootstrappedItemViewModel item, int index);
        #endregion

        #region Private Methods
        private async Task LoadItemsAsync(List<MpBootstrappedItemViewModel> items) {
            if (IS_PARALLEL_LOADING_ENABLED) {
                await LoadItemsParallelAsync(items);
            } else {
                await LoadItemsSequentialAsync(items);
            }
        }
        private async Task LoadItemsParallelAsync(List<MpBootstrappedItemViewModel> items) {
            await Task.WhenAll(items.Select((x,idx) => LoadItemAsync(x,idx))); 
            while (items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }
        }

        private async Task LoadItemsSequentialAsync(List<MpBootstrappedItemViewModel> items) {
            for (int i = 0; i < items.Count; i++) {
                await LoadItemAsync(items[i], i);
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



