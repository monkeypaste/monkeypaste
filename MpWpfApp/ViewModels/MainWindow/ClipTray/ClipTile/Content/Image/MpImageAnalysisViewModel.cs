using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MpWpfApp {
    public class MpImageAnalysisCollectionViewModel  : MpObservableCollectionViewModel<MpImageAnalysisViewModel> {
        public MpImageAnalysisCollectionViewModel() : base() {
            this.Add(new MpImageAnalysisViewModel());
        }
        public MpImageAnalysisCollectionViewModel(string analysisData) {
            this.Add(new MpImageAnalysisViewModel(analysisData));
        }
    }

    public class MpImageAnalysisViewModel : MpViewModelBase {
        #region Private Variables
        private double _minScore = 0.0;
        #endregion

        #region Properties
        private BitmapSource _sourceBmp = null;
        public BitmapSource SourceBmp {
            get {
                return _sourceBmp;
            }
            set {
                if(_sourceBmp != value) {
                    _sourceBmp = value;
                    OnPropertyChanged(nameof(SourceBmp));
                }
            }
        }
        public List<string> Captions {
            get {
                var captions = new List<string>();
                if (ImageAnalysis == null) {
                    return captions;
                }
                foreach(var caption in ImageAnalysis.description.captions) {
                    if(caption.confidence < _minScore) {
                        continue;
                    }
                    captions.Add(caption.text);
                }
                return captions;
            }
        }

        public List<string> Tags {
            get {
                var tags = new List<string>();
                if (ImageAnalysis == null) {
                    return tags;
                }
                foreach (var tag in ImageAnalysis.description.tags) {
                    tags.Add(tag);
                }
                return tags;
            }
        }

        public List<string> Categories {
            get {
                var categories = new List<string>();
                if (ImageAnalysis == null) {
                    return categories;
                }
                foreach (var category in ImageAnalysis.categories) {
                    categories.Add(category.name);
                }
                return categories;
            }
        }

        public string Format {
            get {
                if(ImageAnalysis == null) {
                    return string.Empty;
                }
                return ImageAnalysis.metadata.format;
            }
        }

        public string Width {
            get {
                if (ImageAnalysis == null) {
                    return string.Empty;
                }
                return ImageAnalysis.metadata.width.ToString();
            }
        }

        public string Height {
            get {
                if (ImageAnalysis == null) {
                    return string.Empty;
                }
                return ImageAnalysis.metadata.height.ToString();
            }
        }

        public string DominantForegroundColorName {
            get {
                if(ImageAnalysis == null) {
                    return string.Empty;
                }
                return ImageAnalysis.color.dominantColorForeground;
            }
        }

        public string DominantBackgroundColorName {
            get {
                if (ImageAnalysis == null) {
                    return string.Empty;
                }
                return ImageAnalysis.color.dominantColorBackground;
            }
        }

        public List<string> DominantColorNames {
            get {
                var dominantColorNames = new List<string>();
                if (ImageAnalysis == null) {
                    return dominantColorNames;
                }
                foreach(var dc in ImageAnalysis.color.dominantColors) {
                    dominantColorNames.Add(dc);
                }
                return dominantColorNames;
            }
        }

        public string AccentColorName {
            get {
                if (ImageAnalysis == null) {
                    return string.Empty;
                }
                return ImageAnalysis.color.accentColor;
            }
        }

        public bool IsBw {
            get {
                if (ImageAnalysis == null) {
                    return false;
                }
                return ImageAnalysis.color.isBwImg;
            }
        }

        private MpImageAnalysis _imageAnalysis = null;
        public MpImageAnalysis ImageAnalysis {
            get {
                return _imageAnalysis;
            }
            set {
                if(_imageAnalysis != value) {
                    _imageAnalysis = value;
                    OnPropertyChanged(nameof(ImageAnalysis));
                    OnPropertyChanged(nameof(IsBusy));
                    OnPropertyChanged(nameof(AccentColorName));
                    OnPropertyChanged(nameof(DominantBackgroundColorName));
                    OnPropertyChanged(nameof(DominantForegroundColorName));
                    OnPropertyChanged(nameof(DominantColorNames));
                    OnPropertyChanged(nameof(Width));
                    OnPropertyChanged(nameof(Height));
                    OnPropertyChanged(nameof(Format));
                    OnPropertyChanged(nameof(Tags));
                    OnPropertyChanged(nameof(Captions));
                    OnPropertyChanged(nameof(Categories));
                }
            }
        }

        #endregion

        #region Public Methods
        public MpImageAnalysisViewModel() : base() {
            var ia = new MpImageAnalysis();
            ia.categories = new List<MpImageCategory>();
            var testcategory = new MpImageCategory();
            testcategory.name = "Test Category";
            ia.categories.Add(testcategory);
            ia.categories.Add(testcategory);

            ia.description = new MpImageDescription();
            ia.description.tags = new List<string>();
            ia.description.tags.Add("Test Tag1");
            ia.description.tags.Add("Test Tag2");
            ia.description.captions = new List<MpImageCaptions>();
            var testcaption = new MpImageCaptions();
            testcaption.text = "Test Caption";
            ia.description.captions.Add(testcaption);
            ia.description.captions.Add(testcaption);

            ia.metadata = new MpImageMetaData();
            ia.metadata.format = @"Png";
            ia.metadata.width = 500;
            ia.metadata.height = 500;

            ia.color = new MpImageColor();
            ia.color.dominantColorForeground = "domFgColor";
            ia.color.dominantColorBackground = "domBgColor";
            ia.color.dominantColors = new List<string>();
            ia.color.dominantColors.Add("DomColor1");
            ia.color.dominantColors.Add("DomColor2");
            ia.color.accentColor = "accentColor";
            ia.color.isBwImg = false;

            ImageAnalysis = ia;

            //SourceBmp = (BitmapSource)new BitmapImage(new Uri(@"pack://application:,,,/Resources/Images/joystick.png"));
        }

        public MpImageAnalysisViewModel(string serializedAnalysis) : this() {
            if(string.IsNullOrEmpty(serializedAnalysis)) {
                return;
            }
            ImageAnalysis = JsonConvert.DeserializeObject<MpImageAnalysis>(serializedAnalysis);
        }
        #endregion
    }
}
