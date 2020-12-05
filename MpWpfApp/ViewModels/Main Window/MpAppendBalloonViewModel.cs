using GalaSoft.MvvmLight.CommandWpf;
using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpAppendBalloonViewModel : MpViewModelBase {
        private static readonly Lazy<MpAppendBalloonViewModel> _Lazy = new Lazy<MpAppendBalloonViewModel>(() => new MpAppendBalloonViewModel());
        public static MpAppendBalloonViewModel Instance { get { return _Lazy.Value; } }

        #region Static Variables
        #endregion

        #region Private Variables
        #endregion

        #region View Models
        private MpClipTileViewModel _clipTileViewModel;
        public MpClipTileViewModel ClipTileViewModel {
            get {
                return _clipTileViewModel;
            }
            set {
                if (_clipTileViewModel != value) {
                    _clipTileViewModel = value;
                    OnPropertyChanged(nameof(ClipTileViewModel));
                }
            }
        }
        #endregion

        #region Properties

        private ObservableCollection<string> _clipTileStrings = new ObservableCollection<string>();
        public ObservableCollection<string> ClipTileStrings {
            get {
                return _clipTileStrings;
            }
            set {
                if (_clipTileStrings != value) {
                    _clipTileStrings = value;
                    OnPropertyChanged(nameof(ClipTileStrings));
                }
            }
        }
        #endregion

        #region Static Methods
        public static void ShowAppendBalloon(MpClipTileViewModel ctvm) {
            TaskbarIcon tbi = new TaskbarIcon(); 
            var abc = new MpAppendBalloonControl();
            abc.DataContext = new MpAppendBalloonViewModel(ctvm);
            tbi.ShowCustomBalloon(abc, System.Windows.Controls.Primitives.PopupAnimation.Slide, Properties.Settings.Default.NotificationBalloonVisibilityTimeMs);            
        }
        #endregion

        #region Private Methods

        private MpAppendBalloonViewModel(MpClipTileViewModel ctvm) : base() {
            PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(ClipTileViewModel):
                        ClipTileStrings.Clear();
                        break;
                }
            };
            ClipTileViewModel = ctvm;
        }

        #endregion

        #region Public Methods
        public MpAppendBalloonViewModel() : this(null) { }

        public void AppendBalloonControl_Loaded(object sender, RoutedEventArgs e) {
            
        }
        #endregion

        #region Commands
        private RelayCommand _cancelCommand;
        public ICommand CancelCommand {
            get {
                if (_cancelCommand == null) {
                    _cancelCommand = new RelayCommand(Cancel);
                }
                return _cancelCommand;
            }
        }
        private void Cancel() {
        }
        #endregion
    }
}
