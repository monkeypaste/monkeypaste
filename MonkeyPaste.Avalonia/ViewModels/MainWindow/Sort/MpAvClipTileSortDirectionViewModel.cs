using Avalonia.Controls;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System.Threading.Tasks;
using System.Windows.Input;
using FocusManager = Avalonia.Input.FocusManager;

namespace MonkeyPaste.Avalonia {
    public class MpAvClipTileSortDirectionViewModel :
        MpViewModelBase,
        MpIExpandableViewModel {
        #region Private Variables

        #endregion

        #region Statics
        private static MpAvClipTileSortDirectionViewModel _instance;
        public static MpAvClipTileSortDirectionViewModel Instance => _instance ?? (_instance = new MpAvClipTileSortDirectionViewModel());


        #endregion

        #region MpIExpandableViewModel Implementation

        public bool IsExpanded { get; set; }

        #endregion

        #region Properties

        #region Appearance

        public string SortDirIconResourceKey =>
            !IsSortDescending ?
                "DescendingSvg" :
                "AscendingSvg";
        #endregion

        #region State
        public int DoubleClickCount { get; set; } = 0;
        public bool IsSortDescending { get; set; } = true;

        public bool IsAnySortDirOrFieldFocused {
            get {
                if (MpAvClipTileSortFieldViewModel.Instance.IsSortDropDownOpen) {
                    return true;
                }
                if (Mp.Services.FocusMonitor.FocusElement is Control c && (
                    c.TryGetSelfOrAncestorDataContext<MpAvFilterMenuViewModel>(out _) ||
                    c.TryGetSelfOrAncestorDataContext<MpAvClipTileSortDirectionViewModel>(out _) ||
                    c.TryGetSelfOrAncestorDataContext<MpAvClipTileSortFieldViewModel>(out _))) {
                    return true;
                }
                return false;
            }
        }

        public bool CanChangeDir =>
            MpAvClipTrayViewModel.Instance.QueryCommand.CanExecute(null);

        #endregion
        #endregion

        #region Constructors
        public MpAvClipTileSortDirectionViewModel() : base(null) {
            PropertyChanged += MpAvClipTileSortDirectionViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods
        public void Init() {
            //await Task.Delay(1);

            //ResetToDefault(true);
        }
        #endregion

        #region Private Methods

        private void MpAvClipTileSortDirectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSortDescending):
                    OnPropertyChanged(nameof(SortDirIconResourceKey));
                    MpMessenger.SendGlobal(MpMessageType.QuerySortChanged);
                    Mp.Services.Query.NotifyQueryChanged();
                    break;
                case nameof(IsExpanded):
                    MpMessenger.SendGlobal(MpMessageType.FilterItemSizeChanged);
                    if (IsExpanded &&
                        MpAvMainWindowViewModel.Instance.IsVerticalOrientation &&
                        MpAvSearchBoxViewModel.Instance.IsExpanded) {
                        MpAvSearchBoxViewModel.Instance.ToggleIsSearchBoxExpandedCommand.Execute(null);
                    }
                    Dispatcher.UIThread.Post(async () => {
                        await Task.Delay(MpAvFilterMenuViewModel.Instance.FilterAnimTimeMs);
                        MpMessenger.SendGlobal(MpMessageType.FilterItemSizeChanged);
                        while (true) {
                            if (!IsExpanded) {
                                break;
                            }
                            if (!IsAnySortDirOrFieldFocused) {
                                IsExpanded = false;
                                break;
                            }
                            await Task.Delay(1000);
                        }
                    });
                    break;
            }
        }
        #endregion

        #region Commands

        public ICommand ToggleSortDirectionCommand => new MpCommand(
            () => {
                IsSortDescending = !IsSortDescending;
            }, () => {
                // toggling while querying will get button out of sync w/ query
                // if query cannot execute
                return CanChangeDir;
            });
        #endregion
    }

}
