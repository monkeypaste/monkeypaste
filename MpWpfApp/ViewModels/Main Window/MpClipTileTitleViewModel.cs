using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpClipTileTitleViewModel : MpViewModelBase {
        #region View Models
        public MpClipTileViewModel ClipTileViewModel { get; set; }
        #endregion

        #region Properties

        private string _detailText = "This is empty detail text";
        public string DetailText {
            get {
                return _detailText;
            }
            set {
                if (_detailText != value) {
                    _detailText = value;
                    OnPropertyChanged(nameof(DetailText));
                }
            }
        }

        private Brush _detailTextColor = Brushes.Black;
        public Brush DetailTextColor {
            get {
                return _detailTextColor;
            }
            set {
                if (_detailTextColor != value) {
                    _detailTextColor = value;
                    OnPropertyChanged(nameof(DetailTextColor));
                }
            }
        }
        private ImageSource _icon = null;
        public ImageSource Icon {
            get {
                return _icon;
            }
            set {
                if (_icon != value) {
                    _icon = value;
                    OnPropertyChanged(nameof(Icon));
                }
            }
        }

        private string _title = string.Empty;
        public string Title {
            get {
                return _title;
            }
            set {
                if (_title != value) {
                    _title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        private Brush _titleColor;
        public Brush TitleColor {
            get {
                return _titleColor;
            }
            set {
                if (_titleColor != value) {
                    _titleColor = value;
                    OnPropertyChanged(nameof(TitleColor));
                }
            }
        }

        public Brush TitleColorLighter {
            get {
                return MpHelpers.ChangeBrushAlpha(
                    MpHelpers.ChangeBrushBrightness(
                        TitleColor,
                        -0.5f),
                    100);
            }
        }

        public Brush TitleColorDarker {
            get {
                return MpHelpers.ChangeBrushAlpha(
                    MpHelpers.ChangeBrushBrightness(
                        TitleColor,
                        -0.4f),
                    50);
            }
        }

        public Brush TitleColorAccent {
            get {
                return MpHelpers.ChangeBrushAlpha(
                    MpHelpers.ChangeBrushBrightness(
                        TitleColor,
                        -0.0f),
                    100);
            }
        }

        private BitmapSource _titleSwirl = null;
        public BitmapSource TitleSwirl {
            get {
                return _titleSwirl;
            }
            set {
                if (_titleSwirl != value) {
                    _titleSwirl = value;
                    OnPropertyChanged(nameof(TitleSwirl));
                }
            }
        }

        private bool _isTitleTextBoxFocused = false;
        public bool IsTitleTextBoxFocused {
            get {
                return _isTitleTextBoxFocused;
            }
            set {
                if (_isTitleTextBoxFocused != value) {
                    _isTitleTextBoxFocused = value;
                    OnPropertyChanged(nameof(IsTitleTextBoxFocused));
                }
            }
        }

        private bool _isEditingTitle = false;
        public bool IsEditingTitle {
            get {
                return _isEditingTitle;
            }
            set {
                if (_isEditingTitle != value) {
                    _isEditingTitle = value;
                    OnPropertyChanged(nameof(IsEditingTitle));
                }
            }
        }

        private Visibility _tileTitleTextBlockVisibility = Visibility.Visible;
        public Visibility TileTitleTextBlockVisibility {
            get {
                return _tileTitleTextBlockVisibility;
            }
            set {
                if (_tileTitleTextBlockVisibility != value) {
                    _tileTitleTextBlockVisibility = value;
                    OnPropertyChanged(nameof(TileTitleTextBlockVisibility));
                }
            }
        }

        private Visibility _tileTitleTextBoxVisibility = Visibility.Collapsed;
        public Visibility TileTitleTextBoxVisibility {
            get {
                return _tileTitleTextBoxVisibility;
            }
            set {
                if (_tileTitleTextBoxVisibility != value) {
                    _tileTitleTextBoxVisibility = value;
                    OnPropertyChanged(nameof(TileTitleTextBoxVisibility));
                }
            }
        }

        private double _tileTitleIconSize = MpMeasurements.Instance.ClipTileTitleIconSize;
        public double TileTitleIconSize {
            get {
                return _tileTitleIconSize;
            }
            set {
                if (_tileTitleIconSize != value) {
                    _tileTitleIconSize = value;
                    OnPropertyChanged(nameof(TileTitleIconSize));
                }
            }
        }
        #endregion

        #region Constructor/Inits
        public MpClipTileTitleViewModel(MpCopyItem ci, MpClipTileViewModel parent) {
            PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(IsEditingTitle):
                        if (IsEditingTitle) {
                            //show textbox and select all text
                            TileTitleTextBoxVisibility = Visibility.Visible;
                            TileTitleTextBlockVisibility = Visibility.Collapsed;
                            IsTitleTextBoxFocused = false;
                            IsTitleTextBoxFocused = true;
                        } else {
                            TileTitleTextBoxVisibility = Visibility.Collapsed;
                            TileTitleTextBlockVisibility = Visibility.Visible;
                            IsTitleTextBoxFocused = false;
                            ClipTileViewModel.CopyItem.Title = Title;
                            ClipTileViewModel.CopyItem.WriteToDatabase();
                        }
                        break;
                }
            };
            ClipTileViewModel = parent;
            Title = ci.Title;
            TitleColor = new SolidColorBrush(ci.ItemColor.Color);
            Icon = ci.App.Icon.IconImage;
        }

        public void ClipTileTitle_Loaded(object sender, RoutedEventArgs e) {
            if (TitleSwirl == null) {
                var swirl1 = (BitmapSource)new BitmapImage(new Uri("pack://application:,,,/Resources/title_swirl0001.png"));
                swirl1 = MpHelpers.TintBitmapSource(swirl1, ((SolidColorBrush)TitleColor).Color);
                var swirl2 = (BitmapSource)new BitmapImage(new Uri("pack://application:,,,/Resources/title_swirl0002.png"));
                swirl2 = MpHelpers.TintBitmapSource(swirl2, ((SolidColorBrush)TitleColorLighter).Color);
                var swirl3 = (BitmapSource)new BitmapImage(new Uri("pack://application:,,,/Resources/title_swirl0003.png"));
                swirl3 = MpHelpers.TintBitmapSource(swirl3, ((SolidColorBrush)TitleColorDarker).Color);
                var swirl4 = (BitmapSource)new BitmapImage(new Uri("pack://application:,,,/Resources/title_swirl0004.png"));
                swirl4 = MpHelpers.TintBitmapSource(swirl4, ((SolidColorBrush)TitleColorAccent).Color);

                TitleSwirl = MpHelpers.MergeImages(new List<BitmapSource>() { swirl1, swirl2, swirl3, swirl4 });
            }

            var titleCanvas = (Canvas)sender;
            var clipTileTitleTextBox = (TextBox)titleCanvas.FindName("ClipTileTitleTextBox");
            clipTileTitleTextBox.PreviewKeyDown += ClipTileViewModel.ClipTrayViewModel.MainWindowViewModel.MainWindow_PreviewKeyDown;
            clipTileTitleTextBox.LostFocus += (s, e4) => {
                IsEditingTitle = false;
            };

            var titleIconImage = (Image)titleCanvas.FindName("ClipTileAppIconImage");
            Canvas.SetLeft(titleIconImage, ClipTileViewModel.TileBorderSize - ClipTileViewModel.TileTitleHeight - 10);
            Canvas.SetTop(titleIconImage, 2);

            var titleDetailTextBlock = (TextBlock)titleCanvas.FindName("ClipTileTitleDetailTextBlock");
            Canvas.SetLeft(titleDetailTextBlock, 5);
            Canvas.SetTop(titleDetailTextBlock, ClipTileViewModel.TileTitleHeight - 14);
        }
        #endregion

        #region Commands

        #endregion
    }
}
