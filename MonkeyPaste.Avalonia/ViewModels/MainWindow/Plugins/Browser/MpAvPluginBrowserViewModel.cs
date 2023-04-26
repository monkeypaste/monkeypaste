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

        public MpPluginBrowserTabType SelectedTabType =>
            (MpPluginBrowserTabType)SelectedTabIdx;

        public int SelectedTabIdx { get; set; }

        public string FilterText { get; set; }

        //public bool IsFiltering { get; set; }
        //public bool IsSelecting { get; set; }
        public WindowState WindowState { get; set; }
        public bool IsAnyBusy =>
            IsBusy || Items.Any(x => x.IsBusy);

        public bool IsSelectedBusy =>
            SelectedItem != null && SelectedItem.IsAnyBusy;
        #endregion

        #endregion

        #region Constructors
        private MpAvPluginBrowserViewModel() {
            MpConsole.WriteLine("plug browser ctor");
            PropertyChanged += MpAvPluginBrowserViewModel_PropertyChanged;
            Items.CollectionChanged += Items_CollectionChanged;

            Selection = new SelectionModel<MpAvPluginItemViewModel>(Items);
            Selection.SelectionChanged += Selection_SelectionChanged;

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
            }
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(Items));
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

        public ICommand ShowPluginBrowserCommand => new MpAsyncCommand(
            async () => {
                OpenPluginBrowserWindow();
            });

        public MpIAsyncCommand PerformFilterCommand => new MpAsyncCommand(
            async () => {
                bool was_busy = IsBusy;
                IsBusy = true;

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
                    IsBusy = was_busy;
                    return;
                }

                var filtered_vml =
                    await Task.WhenAll(
                        manifests_to_filter
                        .Where(x => (x as MpIFilterMatch).IsMatch(FilterText))
                        .Select(x => CreatePluginItemViewModelAsync(x)));
                Items.AddRange(filtered_vml);
                while (Items.Any(x => x.IsAnyBusy)) {
                    await Task.Delay(100);
                }

                IsBusy = was_busy;
            });
        #endregion
    }
}
