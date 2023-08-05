using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Data;
using Avalonia.Media;
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
        Browse,
        Installed,
        Updates
    }

    public class MpAvPluginBrowserViewModel :
        MpAvViewModelBase,
        MpICloseWindowViewModel,
        MpIWantsTopmostWindowViewModel {
        #region Private Variables
        #endregion

        #region Constants

        public const string LEDGER_URL = @"https://github.com/monkeypaste/mp-plugin-list/raw/main/ledger.json";

        #endregion

        #region Statics

        #endregion

        #region Interfaces

        #region MpIWantsTopmostWindowViewModel Implementation
        bool MpIWantsTopmostWindowViewModel.WantsTopmost =>
            true;

        #endregion
        #region MpIChildWindowViewModel Implementation

        public bool IsWindowOpen { get; set; }
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

        public SelectionModel<MpAvPluginItemViewModel> Selection { get; }
        public MpAvPluginItemViewModel SelectedItem =>
            Selection == null ? null : Selection.SelectedItem;
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

        public int SelectedTabIdx { get; set; } = (int)MpPluginBrowserTabType.Installed;

        public string FilterText { get; set; } = string.Empty;

        public WindowState WindowState { get; set; }
        public bool IsAnyBusy =>
            IsBusy || Items.Any(x => x.IsBusy);

        public bool IsSelectedBusy =>
            SelectedItem != null && SelectedItem.IsAnyBusy;

        public bool IsInitialized { get; set; } = false;

        #endregion

        #endregion

        #region Constructors
        public MpAvPluginBrowserViewModel() {
            MpConsole.WriteLine("plug browser ctor");
            PropertyChanged += MpAvPluginBrowserViewModel_PropertyChanged;
            Selection = new SelectionModel<MpAvPluginItemViewModel>(Items);
            Selection.SelectionChanged += Selection_SelectionChanged;
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
                case nameof(IsWindowOpen):
                    if (!IsWindowOpen) {
                        break;
                    }

                    PerformFilterCommand.Execute(null);
                    break;
            }
        }

        private void Selection_SelectionChanged(object sender, SelectionModelSelectionChangedEventArgs<MpAvPluginItemViewModel> e) {
            Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsSelected)));
            OnPropertyChanged(nameof(Selection));
            OnPropertyChanged(nameof(SelectedItem));
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
            while (MpAvPrefViewModel.Instance == null) {
                await Task.Delay(100);
            }
            RecentPluginSearches = await MpAvPrefViewModel.Instance.AddOrUpdateAutoCompleteTextAsync(nameof(MpAvPrefViewModel.Instance.RecentPluginSearchTexts), st);
        }

        private async Task<IEnumerable<MpManifestFormat>> GetRemoteManifests() {
            string ledger_json = await MpFileIo.ReadTextFromUriAsync(LEDGER_URL);
            var ledger = MpJsonConverter.DeserializeObject<MpManifestLedger>(ledger_json);
            if (ledger == null || ledger.manifests == null) {
                return Array.Empty<MpManifestFormat>();
            }
            return ledger.manifests;
        }
        public void OpenPluginBrowserWindow(string selectedGuid) {
            if (IsWindowOpen) {
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
                Background = Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeGrayAccent3Color.ToString()),
                Content = new MpAvPluginBrowserView(),
                Topmost = true
            };
            if (pbw.Content is MpAvPluginBrowserView pbv &&
                pbv.FindControl<TabStrip>("PluginTabStrip") is TabStrip ts) {
                ts.SelectedItem = Tabs[(int)MpPluginBrowserTabType.Installed];
            }

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

            if (!string.IsNullOrEmpty(selectedGuid)) {
                pbw.Opened += async (s, e) => {
                    while (IsBusy) {
                        await Task.Delay(100);
                    }
                    if (Items.FirstOrDefault(x => x.PluginGuid == selectedGuid) is MpAvPluginItemViewModel pivm) {
                        Selection.SelectedItem = pivm;
                    }
                };
            }
            pbw.ShowChild();
            OnPropertyChanged(nameof(IsWindowOpen));
        }
        #endregion

        #region Commands


        public MpIAsyncCommand PerformFilterCommand => new MpAsyncCommand(
            async () => {
                while (IsBusy) {
                    await Task.Delay(100);
                }

                IsBusy = true;

                IEnumerable<MpManifestFormat> manifests_to_filter = new List<MpManifestFormat>();
                switch (SelectedTabType) {
                    case MpPluginBrowserTabType.Browse:
                        manifests_to_filter = await GetRemoteManifests();
                        break;
                    case MpPluginBrowserTabType.Installed:
                        manifests_to_filter = MpPluginLoader.Plugins.Select(x => x.Value);
                        break;
                }

                var filtered_ml =
                        manifests_to_filter
                        .Cast<MpIFilterMatch>()
                        .Where(x => x.IsFilterMatch(FilterText))
                        .Cast<MpManifestFormat>();

                Items.Clear();
                Selection.Clear();
                foreach (var filtered_m in filtered_ml) {
                    var mvm = await CreatePluginItemViewModelAsync(filtered_m);
                    Items.Add(mvm);
                }

                if (Items.Any()) {
                    Selection.Select(0);
                }

                IsBusy = false;
                OnPropertyChanged(nameof(Items));
                OnPropertyChanged(nameof(SelectedItem));

                MpConsole.WriteLine($"Items COunt: {Items.Count} Filtered Count: {filtered_ml.Count()} Manifest COunt: {manifests_to_filter.Count()}");

                await AddOrUpdateRecentFilterTextsAsync(FilterText);
            });
        #endregion
    }
}
