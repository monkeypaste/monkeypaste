using Avalonia.Controls;
using Avalonia.Threading;
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
        public bool IsSortDescending { get; set; }

        public bool IsSortDirOrFieldFocused {
            get {
                var cf = FocusManager.Instance.Current;
                if (cf == null) {
                    return false;
                }
                if (cf is Control c &&
                    c.GetVisualAncestor<MpAvClipTileSortView>() != null) {
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
                case nameof(IsSortDirOrFieldFocused):
                    if (IsSortDirOrFieldFocused) {
                        break;
                    }
                    if (!IsExpanded) {
                        break;
                    }
                    Dispatcher.UIThread.Post(async () => {
                        // when field or dir looses focus wait a little to see if returns 
                        await Task.Delay(3000);
                        if (IsSortDirOrFieldFocused) {
                            return;
                        }
                        IsExpanded = false;
                    });
                    break;
            }
        }
        #endregion

        #region Commands
        public ICommand ClickCommand => new MpCommand(() => {
            if (IsExpanded) {
                if (CanChangeDir) {
                    // toggling while querying will get button out of sync w/ query
                    // if query cannot execute
                    IsSortDescending = !IsSortDescending;
                }

            } else {
                IsExpanded = true;
            }
        });
        public ICommand DoubleClickCommand => new MpCommand(() => {
            if (IsExpanded) {
                IsExpanded = false;
            } else {
                if (CanChangeDir) {
                    // toggling while querying will get button out of sync w/ query
                    // if query cannot execute
                    IsSortDescending = !IsSortDescending;
                }
            }
        });
        #endregion
    }

}
