using MonkeyPaste;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpDetectedImageObjectViewModel : 
        MpViewModelBase<MpDetectedImageObjectCollectionViewModel>,
        MpIResizableViewModel,
        MpIMovableViewModel,
        MpISelectableViewModel,
        MpIHoverableViewModel{

        #region Private Variables       

        #endregion

        #region Properties
                
        #region State

        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; }

        #endregion

        #region MpIHoverableViewModel Implementation

        public bool IsHovering { get; set; }

        #endregion

        #region MpIResizableViewModel Implementation

        public bool IsResizing { get; set; }
        public bool CanResize { get; set; }

        #endregion

        #region MpIMovableViewModel Implementation

        public bool IsMoving { get; set; }
        public bool CanMove { get; set; }

        #endregion

        public bool IsNameReadOnly { get; set; }

        #endregion

        #region Appearance 


        public double FontSize {
            get {
                return Math.Max(8, Math.Min(Width, Height) / 5);
            }
        }

        public Brush BorderBrush {
            get {
                if (IsHovering) {
                    return Brushes.Yellow;
                }
                return Brushes.Blue;
            }
        }

        public string BorderHexColorStr {
            get {
                if(IsSelected) {
                    return MpSystemColors.IsSelectedBorderColor;
                }
                if(IsHovering) {
                    return MpSystemColors.IsHoveringBorderColor;
                }
                return HexColor;
            }
        }

        public double DisplayScore { get; set; } = 0;

        public ProgressArc BackgroundCircle => new ProgressArc(1);

        public ProgressArc ValueCircle => new ProgressArc(DisplayScore);

        public int ScoreLabelValue => (int)(DisplayScore * 100);

        #endregion

        #region Model

        public string HexColor {
            get {
                if(DetectedImageObject == null) {
                    return MpSystemColors.Transparent;
                }
                return DetectedImageObject.HexColor;
            }
            set {
                if(HexColor != value) {
                    DetectedImageObject.HexColor = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(HexColor));
                }
            }
        }

        public double X {
            get {
                if (DetectedImageObject == null) {
                    return 0;
                }
                return DetectedImageObject.X;
            }
            set {
                if(X != value) {
                    DetectedImageObject.X = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(X));
                }
            }
        }

        public double Y {
            get {
                if (DetectedImageObject == null) {
                    return 0;
                }
                return DetectedImageObject.Y;
            }
            set {
                if (Y != value) {
                    DetectedImageObject.Y = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(Y));
                }
            }
        }

        public double Width {
            get {
                if (DetectedImageObject == null) {
                    return 0;
                }
                return DetectedImageObject.Width;
            }
            set {
                if (Height != value) {
                    DetectedImageObject.Width = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(Width));
                }
            }
        }

        public double Height {
            get {
                if (DetectedImageObject == null) {
                    return 0;
                }
                return DetectedImageObject.Height;
            }
            set {
                if(Height != value) {
                    DetectedImageObject.Height = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(Height));
                }
            }
        }

        public double Score {
            get {
                if (DetectedImageObject == null) {
                    return 0;
                }
                return DetectedImageObject.Score;
            }
            set {
                if (Score != value) {
                    DetectedImageObject.Score = value;
                    OnPropertyChanged(nameof(Score));
                    OnPropertyChanged(nameof(ValueCircle));
                }
            }
        }

        public string Label {
            get {
                if (DetectedImageObject == null) {
                    return string.Empty;
                }
                return DetectedImageObject.Label;
            }
            set {
                if (Label != value) {
                    DetectedImageObject.Label = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(Label));
                }
            }
        }

        public int CopyItemId {
            get {
                if(DetectedImageObject == null) {
                    return 0;
                }
                return DetectedImageObject.CopyItemId;
            }
            set {
                if (CopyItemId != value) {
                    DetectedImageObject.CopyItemId = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(DetectedImageObject));
                }
            }
        }

        public MpDetectedImageObject DetectedImageObject { get; set; }

        #endregion

        #endregion

        #region Public Methods

        public MpDetectedImageObjectViewModel() : base(null) { }

        public MpDetectedImageObjectViewModel(MpDetectedImageObjectCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpDetectedImageObjectViewModel_PropertyChanged;
        }

        private void MpDetectedImageObjectViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(HasModelChanged):
                    if(HasModelChanged && !IsBusy) {
                        Task.Run(async () => {
                            await DetectedImageObject.WriteToDatabaseAsync();
                            HasModelChanged = false;
                        });
                    }
                    break;
            }
        }

        public async Task InitializeAsync(MpDetectedImageObject dio, bool isDrawing = false) {
            IsBusy = true;

            await Task.Delay(1);

            DetectedImageObject = dio;

            IsBusy = false;
        }
        
        #endregion

        #region Private Methods
        
        #endregion

        #region Commands

        #endregion

        #region Overrides
        #endregion
    }
}
