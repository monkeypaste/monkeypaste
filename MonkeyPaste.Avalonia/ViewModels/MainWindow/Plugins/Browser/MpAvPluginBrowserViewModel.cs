using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Data;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public enum MpPluginBrowserTabType {
        Installed,
        Browse,
        Updates
    }

    public class MpAvPluginBrowserViewModel :
        MpViewModelBase,
        MpIChildWindowViewModel,
        MpIWantsTopmostWindowViewModel {
        #region Private Variables
        #endregion

        #region Constants

        public const string LEDGER_URL = @"https://github.com/monkeypaste/mp-plugin-list/raw/main/ledger.json";

        #endregion

        #region Statics

        private static MpAvPluginBrowserViewModel _instance;
        public static MpAvPluginBrowserViewModel Instance =>
            _instance ?? (_instance = new MpAvPluginBrowserViewModel());

        #endregion

        #region Interfaces

        #region MpIWantsTopmostWindowViewModel Implementation
        bool MpIWantsTopmostWindowViewModel.WantsTopmost =>
            true;

        #endregion
        #region MpIChildWindowViewModel Implementation

        public bool IsOpen { get; set; }
        public MpWindowType WindowType =>
            MpWindowType.PopOut;

        #endregion

        #endregion

        #region Properties

        #region View Models

        private ObservableCollection<string> _tabs;
        public ObservableCollection<string> Tabs {
            get {
                if (_tabs == null) {
                    _tabs = new ObservableCollection<string>(typeof(MpPluginBrowserTabType).EnumToLabels());
                }
                return _tabs;
            }
        }
        public IList<string> RecentPluginSearches { get; private set; }

        public ObservableCollection<MpAvPluginItemViewModel> Items { get; private set; } = new ObservableCollection<MpAvPluginItemViewModel>();

        public MpAvPluginItemViewModel SelectedItem { get; set; }
        #endregion

        #region Appearance

        public string WindowTitle {
            get {
                string sufffix = string.Empty;
                if (SelectedItem != null) {
                    sufffix = $": {SelectedItem.PluginTitle}";
                }
                return $"Plugin Browser{sufffix}";
            }
        }

        #endregion

        #region State

        public MpPluginBrowserTabType SelectedTabType =>
            (MpPluginBrowserTabType)SelectedTabIdx;

        public int SelectedTabIdx { get; set; }

        public string FilterText { get; set; }

        public WindowState WindowState { get; set; }
        public bool IsAnyBusy =>
            IsBusy || Items.Any(x => x.IsBusy);

        public bool IsSelectedBusy =>
            SelectedItem != null && SelectedItem.IsAnyBusy;

        public bool IsInitialized { get; set; } = false;
        #endregion

        #endregion

        #region Constructors
        private MpAvPluginBrowserViewModel() {
            MpConsole.WriteLine("plug browser ctor");
            PropertyChanged += MpAvPluginBrowserViewModel_PropertyChanged;
            Items.CollectionChanged += Items_CollectionChanged;

            SelectedTabIdx = (int)MpPluginBrowserTabType.Installed;
            AddOrUpdateRecentFilterTextsAsync(null).FireAndForgetSafeAsync(this);
        }

        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private void MpAvPluginBrowserViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(FilterText):
                    PerformFilterCommand.Execute(null);
                    break;
                case nameof(SelectedTabIdx):
                    PerformFilterCommand.Execute(null);
                    break;
                case nameof(SelectedItem):
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsSelected)));
                    OnPropertyChanged(nameof(IsSelectedBusy));
                    break;
                case nameof(IsAnyBusy):
                    OnPropertyChanged(nameof(IsSelectedBusy));
                    break;
                case nameof(IsOpen):
                    if (!IsOpen) {
                        break;
                    }

                    PerformFilterCommand.Execute(null);
                    break;
            }
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(Items));
        }
        private async Task<MpAvPluginItemViewModel> CreatePluginItemViewModelAsync(MpManifestFormat pf) {
            var pivm = new MpAvPluginItemViewModel(this);
            await pivm.InitializeAsync(pf);
            return pivm;
        }

        private async Task AddOrUpdateRecentFilterTextsAsync(string st) {
            while (MpPrefViewModel.Instance == null) {
                await Task.Delay(100);
            }
            RecentPluginSearches = await MpPrefViewModel.Instance.AddOrUpdateAutoCompleteTextAsync(nameof(MpPrefViewModel.Instance.RecentPluginSearchTexts), st);
        }

        private async Task<IEnumerable<MpManifestFormat>> GetRemoteManifests() {
            IsBusy = true;
            string ledger_json = await MpFileIo.ReadTextFromUriAsync(LEDGER_URL);
            var ledger = MpJsonConverter.DeserializeObject<MpManifestLedger>(ledger_json);
            IsBusy = false;
            if (ledger == null || ledger.manifests == null) {
                return Array.Empty<MpManifestFormat>();
            }
            return ledger.manifests;
        }
        private void OpenPluginBrowserWindow() {
            if (IsOpen) {
                if (WindowState == WindowState.Minimized) {
                    WindowState = WindowState.Normal;
                }
                return;
            }
            MpAvWindow pbw = new MpAvWindow() {
                Width = 800,
                Height = 500,
                DataContext = this,
                ShowInTaskbar = true,
                Icon = MpAvIconSourceObjToBitmapConverter.Instance.Convert("JigsawImage", typeof(WindowIcon), null, null) as WindowIcon,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Content = new MpAvPluginBrowserView(),
                Topmost = true
            };

            pbw.Bind(
                Window.TitleProperty,
                new Binding() {
                    Source = this,
                    Path = nameof(WindowTitle),
                    Converter = MpAvStringToWindowTitleConverter.Instance
                });

            pbw.Bind(
                Window.WindowStateProperty,
                new Binding() {
                    Source = this,
                    Path = nameof(WindowState),
                    Mode = BindingMode.TwoWay
                });
            pbw.ShowChild();
            OnPropertyChanged(nameof(IsOpen));
        }
        #endregion

        #region Commands

        public ICommand ShowPluginBrowserCommand => new MpCommand(
            () => {
                OpenPluginBrowserWindow();
            });

        public MpIAsyncCommand PerformFilterCommand => new MpAsyncCommand(
            async () => {
                //bool was_busy = IsBusy;
                IsBusy = true;

                await AddOrUpdateRecentFilterTextsAsync(FilterText);

                Items.Clear();
                SelectedItem = null;
                // BUG just to let list catch up...
                await Task.Delay(300);

                IEnumerable<MpManifestFormat> manifests_to_filter = null;
                switch (SelectedTabType) {
                    case MpPluginBrowserTabType.Browse:
                        manifests_to_filter = await GetRemoteManifests();
                        break;
                    case MpPluginBrowserTabType.Installed:
                        manifests_to_filter = MpPluginLoader.Plugins.Select(x => x.Value);
                        break;
                }
                if (manifests_to_filter == null) {
                    IsBusy = false;
                    return;
                }

                //var filtered_vml =
                //    await Task.WhenAll(
                //        manifests_to_filter
                //        .Where(x => (x as MpIFilterMatch).IsMatch(FilterText))
                //        .Select(x => CreatePluginItemViewModelAsync(x)));
                //foreach (var filtered_vm in filtered_vml) {
                //    Items.Add(filtered_vm);
                //}

                var filtered_ml =
                        //await Task.WhenAll(
                        manifests_to_filter
                        .Where(x => (x as MpIFilterMatch).IsMatch(FilterText));
                // .Select(x => CreatePluginItemViewModelAsync(x)));
                foreach (var filtered_m in filtered_ml) {
                    var mvm = await CreatePluginItemViewModelAsync(filtered_m);
                    if (IsBusy) {
                        IsBusy = false;
                    }
                    Items.Add(mvm);
                }
                //Items.AddRange(filtered_ml);
                while (Items.Any(x => x.IsAnyBusy)) {
                    await Task.Delay(100);
                }
                if (Items.Any()) {
                    // BUG on tab change/filter at + scroll offset, scrollviewer not reseting so 
                    // triggering select to reset scroll
                    SelectedItem = Items.FirstOrDefault();
                }

                IsBusy = false;
            }, () => {
                return IsOpen;
            });
        #endregion
    }
}
