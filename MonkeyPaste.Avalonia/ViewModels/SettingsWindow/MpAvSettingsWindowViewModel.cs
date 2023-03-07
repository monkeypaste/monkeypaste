using Avalonia.Controls;
using MonkeyPaste.Common;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvSettingsWindowViewModel : MpViewModelBase {
        #region Private Variables

        private Window _settingsWindow;
        #endregion

        #region Statics

        private static MpAvSettingsWindowViewModel _instance;
        public static MpAvSettingsWindowViewModel Instance => _instance ?? (_instance = new MpAvSettingsWindowViewModel());


        #endregion

        #region Properties

        #region State

        public bool IsVisible { get; set; } = false;

        public int SelectedTabIdx {
            get {
                for (int i = 0; i < IsTabSelected.Count; i++) {
                    if (IsTabSelected[i]) {
                        return i;
                    }

                }
                return -1;
            }
            set {
                if (SelectedTabIdx != value) {
                    for (int i = 0; i < IsTabSelected.Count; i++) {
                        IsTabSelected[i] = i == value;
                    }
                    OnPropertyChanged(nameof(SelectedTabIdx));
                }
            }
        }

        public ObservableCollection<bool> IsTabSelected { get; set; }

        #endregion

        #endregion

        #region Constructors

        public MpAvSettingsWindowViewModel() : base() {
            PropertyChanged += MpAvSettingsWindowViewModel_PropertyChanged;
            IsTabSelected = new ObservableCollection<bool>(Enumerable.Repeat(false, 6));
            IsTabSelected.CollectionChanged += IsTabSelected_CollectionChanged;
        }


        #endregion

        #region Public Methods

        public async Task InitAsync() {
            await Task.Delay(1);
        }

        #endregion

        #region Private Methods

        private void MpAvSettingsWindowViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsVisible):
                    //MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = IsVisible;

                    if (IsVisible) {
                        _settingsWindow = _settingsWindow ?? new MpAvSettingsWindow();
                        _settingsWindow.Show();
                    } else {
                        if (_settingsWindow != null) {
                            _settingsWindow.Close();
                        }

                    }
                    break;
            }
        }


        private void IsTabSelected_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(IsTabSelected));
        }
        #endregion

        #region Commands

        public ICommand ResetSettingsCommand => new MpCommand(
            () => {

            });
        public ICommand SaveSettingsCommand => new MpCommand(
            () => {
                IsVisible = false;
            });

        public ICommand CancelSettingsCommand => new MpCommand(
            () => {
                IsVisible = false;
            });
        public ICommand SelectTabCommand => new MpCommand<object>(
            (args) => {
                int tab_idx = 0;
                if (args is int intArg) {
                    tab_idx = intArg;
                } else if (args is string strArg) {
                    try {
                        tab_idx = int.Parse(strArg);
                    }
                    catch { }
                }

                SelectedTabIdx = tab_idx;
            });

        public ICommand ShowSettingsWindowCommand => new MpCommand<object>(
            (args) => {
                SelectTabCommand.Execute(args);
                IsVisible = true;
            });
        #endregion
    }
}
