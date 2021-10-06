using GalaSoft.MvvmLight.CommandWpf;
using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpStandardBalloonViewModel : MpViewModelBase<object> {
        private static readonly Lazy<MpStandardBalloonViewModel> _Lazy = new Lazy<MpStandardBalloonViewModel>(() => new MpStandardBalloonViewModel());
        public static MpStandardBalloonViewModel Instance { get { return _Lazy.Value; } }

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

        private string _balloonTitle = "Title";
        public string BalloonTitle {
            get {
                return _balloonTitle;
            }
            set {
                if(_balloonTitle != value) {
                    _balloonTitle = value;
                    OnPropertyChanged(nameof(BalloonTitle));
                }
            }
        }

        private string _balloonContent = "Content";
        public string BalloonContent {
            get {
                return _balloonContent;
            }
            set {
                if (_balloonContent != value) {
                    _balloonContent = value;
                    OnPropertyChanged(nameof(BalloonContent));
                }
            }
        }

        private bool _doNotShowAgain = false;
        public bool DoNotShowAgain {
            get {
                return _doNotShowAgain;
            }
            set {
                if(_doNotShowAgain != value) {
                    _doNotShowAgain = value;
                    OnPropertyChanged(nameof(DoNotShowAgain));
                }
            }
        }

        private BitmapSource _balloonBitmapSource = null;
        public BitmapSource BalloonBitmapSource {
            get {
                return _balloonBitmapSource;
            }
            set {
                if(_balloonBitmapSource != value) {
                    _balloonBitmapSource = value;
                    OnPropertyChanged(nameof(BalloonBitmapSource));
                }
            }
        }

        #endregion

        #region Static Methods
        public static void ShowAppendBalloon(MpClipTileViewModel ctvm) {
            TaskbarIcon tbi = new TaskbarIcon(); 
            var abc = new MpStandardBalloonControl();
            abc.DataContext = new MpStandardBalloonViewModel(ctvm);
            tbi.ShowCustomBalloon(
                abc, 
                System.Windows.Controls.Primitives.PopupAnimation.Slide, 
                Properties.Settings.Default.NotificationBalloonVisibilityTimeMs);            
        }

        public static void ShowBalloon(string title, string content, string bitmapSourcePath = "") {
            MpHelpers.Instance.RunOnMainThread(() => {
                if (string.IsNullOrEmpty(bitmapSourcePath)) {
                    bitmapSourcePath = Properties.Settings.Default.AbsoluteResourcesPath + "/Images/monkey (2).png";
                }
                TaskbarIcon tbi = new TaskbarIcon();
                var sbc = new MpStandardBalloonControl(title, content, bitmapSourcePath);

                tbi.ShowCustomBalloon(
                    sbc,
                    System.Windows.Controls.Primitives.PopupAnimation.Slide,
                    Properties.Settings.Default.NotificationBalloonVisibilityTimeMs);
            },System.Windows.Threading.DispatcherPriority.Background);
        }
        #endregion

        #region Public Methods
        public MpStandardBalloonViewModel() : this(null) { }

        public MpStandardBalloonViewModel(string title, string content, string bitmapSourcePath) : base(null) {
            BalloonTitle = title;
            BalloonContent = content;
            BalloonBitmapSource = (BitmapSource)new BitmapImage(new Uri(bitmapSourcePath));
        }

        public void StandardBalloonControl_Loaded(object sender, RoutedEventArgs e) {
            var uc = (UserControl)sender;
        }
        #endregion


        #region Private Methods

        private MpStandardBalloonViewModel(MpClipTileViewModel ctvm) : base(null) {
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

        #region Commands
        private RelayCommand _showPreferncesPropertiesCommand;
        public ICommand ShowPreferncesPropertiesCommand {
            get {
                if (_showPreferncesPropertiesCommand == null) {
                    _showPreferncesPropertiesCommand = new RelayCommand(ShowPreferncesProperties);
                }
                return _showPreferncesPropertiesCommand;
            }
        }
        private void ShowPreferncesProperties() {
            var mwvm = (MpMainWindowViewModel)Application.Current.MainWindow.DataContext;

        }
        #endregion
    }
}