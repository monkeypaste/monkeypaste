using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpClipTileTitleSwirlViewModel : MpViewModelBase {
        #region Private Variables

        private BitmapSource _baseSwirl = null;

        #endregion
        #region Properties

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

        private MpClipTileViewModel _clipTileViewModel = null;
        public MpClipTileViewModel ClipTileViewModel {
            get {
                return _clipTileViewModel;
            }
            set {
                if(_clipTileViewModel != value) {
                    _clipTileViewModel = value;
                    OnPropertyChanged(nameof(ClipTileViewModel));
                }
            }
        }
        #endregion

        #region Public Methods

        public MpClipTileTitleSwirlViewModel(MpClipTileViewModel parent) {
            ClipTileViewModel = parent;
            InitSwirl();
        }

        public void ClipTileTitleSwirlImage_Loaded(object sender,RoutedEventArgs e) {
            if(TitleSwirl == null) {

            }
        }

        public static BitmapSource CreateSwirlBitmapSource(SolidColorBrush baseColorBrush) {
            SolidColorBrush lighterColor = MpHelpers.ChangeBrushAlpha(
                MpHelpers.ChangeBrushBrightness(baseColorBrush, -0.5f), 100);
            SolidColorBrush darkerColor = MpHelpers.ChangeBrushAlpha(
                    MpHelpers.ChangeBrushBrightness(baseColorBrush, -0.4f), 50);
            SolidColorBrush accentColor = MpHelpers.ChangeBrushAlpha(
                            MpHelpers.ChangeBrushBrightness(baseColorBrush, -0.0f), 100);

            var swirl1 = (BitmapSource)new BitmapImage(new Uri("pack://application:,,,/Resources/title_swirl0001.png"));
            swirl1 = MpHelpers.TintBitmapSource(swirl1, baseColorBrush.Color);

            var swirl2 = (BitmapSource)new BitmapImage(new Uri("pack://application:,,,/Resources/title_swirl0002.png"));
            swirl2 = MpHelpers.TintBitmapSource(swirl2, lighterColor.Color);

            var swirl3 = (BitmapSource)new BitmapImage(new Uri("pack://application:,,,/Resources/title_swirl0003.png"));
            swirl3 = MpHelpers.TintBitmapSource(swirl3, darkerColor.Color);

            var swirl4 = (BitmapSource)new BitmapImage(new Uri("pack://application:,,,/Resources/title_swirl0004.png"));
            swirl4 = MpHelpers.TintBitmapSource(swirl4, accentColor.Color);

            return MpHelpers.MergeImages(new List<BitmapSource>() { swirl1, swirl2, swirl3, swirl4 });
        }

        //sharedSwirl is used in multi-selected color change
        public void InitSwirl(BitmapSource sharedSwirl = null) {
            if (sharedSwirl == null) {
                TitleSwirl = CreateSwirlBitmapSource((SolidColorBrush)ClipTileViewModel.TitleColor);
            } else {
                TitleSwirl = sharedSwirl;
            }
            _baseSwirl = TitleSwirl;
        }

        public void Reset() {
            TitleSwirl = _baseSwirl;
        }
        #endregion
    }
}
