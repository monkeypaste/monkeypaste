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
        Browse,
        Installed,
        Updates
    }

    public class MpAvPluginBrowserViewModel :
        MpViewModelBase,
        MpIChildWindowViewModel,
        MpIWantsTopmostWindowViewModel {
        #region Private Variables
        #endregion

        #region Constants
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
                    _tabs = new ObservableCollection<string>() { "Browse", "Installed", "Updates" };
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

        public bool IsInitialized { get; private set; }
        public MpPluginBrowserTabType SelectedTabType =>
            (MpPluginBrowserTabType)SelectedTabIdx;

        public int SelectedTabIdx { get; set; }

        public string FilterText { get; set; }

        public bool IsFiltering { get; set; }
        public bool IsSelecting { get; set; }
        #endregion

        #endregion

        #region Constructors
        public MpAvPluginBrowserViewModel() {
            PropertyChanged += MpAvPluginBrowserViewModel_PropertyChanged;

            Selection = new SelectionModel<MpAvPluginItemViewModel>(Items);
            Selection.SelectionChanged += Selection_SelectionChanged;
        }


        #endregion

        #region Public Methods
        public async Task InitializeAsync() {
            IsBusy = true;

            await AddOrUpdateRecentFilterTextsAsync(null);
            // select installed by default
            SelectTabCommand.Execute(null);
            IsBusy = false;
            IsInitialized = true;
        }
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
                    SelectTabCommand.Execute(SelectedTabType);
                    break;
                case nameof(SelectedItem):
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsSelected)));
                    break;
            }
        }

        private void Selection_SelectionChanged(object sender, SelectionModelSelectionChangedEventArgs<MpAvPluginItemViewModel> e) {
            Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsSelected)));
            OnPropertyChanged(nameof(Selection));
            OnPropertyChanged(nameof(SelectedItem));
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
        private void OpenPluginBrowserWindow() {
            MpAvWindow pbw = new MpAvWindow() {
                Width = 800,
                Height = 500,
                DataContext = this,
                ShowInTaskbar = true,
                Icon = MpAvIconSourceObjToBitmapConverter.Instance.Convert("JigsawImage", typeof(WindowIcon), null, null) as WindowIcon,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Content = new MpAvPluginBrowserView(),
                Topmost = true,
                Title = "Plugin Browser"
            };

            pbw.Bind(
                Window.TitleProperty,
                new Binding() {
                    Source = this,
                    Path = nameof(WindowTitle),
                    Converter = MpAvStringToWindowTitleConverter.Instance
                });
            pbw.ShowChild();

            OnPropertyChanged(nameof(IsOpen));
        }
        #endregion

        #region Commands

        public ICommand ShowPluginBrowserCommand => new MpAsyncCommand(
            async () => {
                if (!IsInitialized) {
                    await InitializeAsync();
                }
                OpenPluginBrowserWindow();
            }, () => {
                return !IsOpen;
            });

        public MpIAsyncCommand PerformFilterCommand => new MpAsyncCommand(
            async () => {
                IsFiltering = true;
                AddOrUpdateRecentFilterTextsAsync(FilterText).FireAndForgetSafeAsync(this);
                Items.Clear();

                IEnumerable<MpManifestFormat> manifests_to_filter = null;
                switch (SelectedTabType) {
                    case MpPluginBrowserTabType.Browse:

                        break;
                    case MpPluginBrowserTabType.Installed:
                        manifests_to_filter = MpPluginLoader.Plugins.Select(x => x.Value);
                        break;
                }
                if (manifests_to_filter == null) {
                    IsFiltering = false;
                    return;
                }

                var filtered_vml =
                    await Task.WhenAll(
                        manifests_to_filter
                        .Where(x => (x as MpIFilterMatch).IsMatch(FilterText))
                        .Select(x => CreatePluginItemViewModelAsync(x)));
                Items.AddRange(filtered_vml);
                OnPropertyChanged(nameof(Items));
                while (Items.Any(x => x.IsBusy)) {
                    await Task.Delay(100);
                }


                IsFiltering = false;
            });

        public MpIAsyncCommand<object> SelectTabCommand => new MpAsyncCommand<object>(
            async (args) => {
                MpPluginBrowserTabType to_select_tab = MpPluginBrowserTabType.Installed;
                if (args is MpPluginBrowserTabType pbtt) {
                    to_select_tab = pbtt;
                }
                SelectedTabIdx = (int)to_select_tab;
                PerformFilterCommand.Execute(null);
            });
        #endregion
    }
}
