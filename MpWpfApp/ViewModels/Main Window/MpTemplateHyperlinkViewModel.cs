using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpTemplateHyperlinkViewModel : MpViewModelBase {
        #region Private Variables

        #endregion

        #region View Models
        public MpClipTileViewModel ClipTileViewModel { get; set; } = null;
        #endregion

        #region Properties
        public Brush TemplateBorderBrush {
            get {
                if (IsHovering) {
                    return Brushes.Yellow;
                }
                return Brushes.White;
            }
        }

        public Brush TemplateForegroundBrush { 
            get {
                if(MpHelpers.IsBright(((SolidColorBrush)TemplateBackgroundBrush).Color)) {
                    return Brushes.Black;
                }
                return Brushes.White;
            }
        }

        private Brush _templateBackgroundBrush = Brushes.Pink;
        public Brush TemplateBackgroundBrush {
            get {
                return _templateBackgroundBrush;
            }
            set {
                if (_templateBackgroundBrush != value) {
                    _templateBackgroundBrush = value;
                    OnPropertyChanged(nameof(TemplateBackgroundBrush));
                    OnPropertyChanged(nameof(TemplateForegroundBrush));
                }
            }
        }

        private string _templateText = string.Empty;
        public string TemplateText { 
            get {
                return _templateText;
            }
            set {
                if(_templateText != value) {
                    _templateText = value;
                    OnPropertyChanged(nameof(TemplateText));
                    OnPropertyChanged(nameof(TemplateBorderWidth));
                }
            }
        }

        private string _templateName = string.Empty;
        public string TemplateName {
            get {
                return _templateName;
            }
            set {
                if (_templateName != value) {
                    _templateName = value;
                    OnPropertyChanged(nameof(TemplateName));
                    OnPropertyChanged(nameof(TemplateBorderWidth));
                }
            }
        }

        public Visibility ClearTemplateTextButtonVisibility {
            get {
                if(IsHovering) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility TemplateTextBlockVisibility {
            get {
                if (IsEditMode) {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }
        }

        public Visibility TemplateTextBoxVisibility {
            get {
                if (IsEditMode) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        private double _templateFontSize = 12;
        public double TemplateFontSize {
            get {
                return _templateFontSize;
            }
            set {
                if(_templateFontSize != value) {
                    _templateFontSize = value;
                    OnPropertyChanged(nameof(TemplateFontSize));
                    OnPropertyChanged(nameof(TemplateBorderWidth));
                }
            }
        }

        public double TemplateBorderWidth {
            get {
                if(IsEditMode) {
                    return (TemplateText.Length * TemplateFontSize) * 0.65;
                }
                return (TemplateName.Length * TemplateFontSize) * 0.65;
            }
        }

        public double TemplateTextBlockWidth {
            get {
                return TemplateBorderWidth - 5;
            }
        }

        private bool _isHovering = false;
        public bool IsHovering {
            get {
                return _isHovering;
            }
            set {
                if (_isHovering != value) {
                    _isHovering = value;
                    OnPropertyChanged(nameof(IsHovering));
                    OnPropertyChanged(nameof(TemplateBorderBrush));
                    OnPropertyChanged(nameof(ClearTemplateTextButtonVisibility));
                }
            }
        }

        private bool _isFocused = false;
        public bool IsFocused {
            get {
                return _isFocused;
            }
            set {
                if (_isFocused != value) {
                    _isFocused = value;
                    OnPropertyChanged(nameof(IsFocused));
                }
            }
        }

        private bool _isEditMode = false;
        public bool IsEditMode {
            get {
                return _isEditMode;
            }
            set {
                if (_isEditMode != value) {
                    _isEditMode = value;
                    OnPropertyChanged(nameof(IsEditMode));
                    OnPropertyChanged(nameof(TemplateBorderBrush));
                    OnPropertyChanged(nameof(TemplateBorderWidth));
                    OnPropertyChanged(nameof(TemplateTextBlockVisibility));
                    OnPropertyChanged(nameof(TemplateTextBoxVisibility));
                }
            }
        }
        #endregion

        #region Public Methods
        public MpTemplateHyperlinkViewModel() : this(null) { }

        public MpTemplateHyperlinkViewModel(MpClipTileViewModel ctvm) : this(ctvm, "Template Name", MpHelpers.GetRandomBrushColor(), 0) { }

        public MpTemplateHyperlinkViewModel(
            MpClipTileViewModel ctvm,
            string templateName,
            Brush templateColor,
            double fontSize) {
            ClipTileViewModel = ctvm;
            TemplateName = templateName;
            TemplateBackgroundBrush = templateColor;
            TemplateFontSize = Math.Max(fontSize, 12);

            //ClipTileViewModel.TemplateTokens.Add(this);
        }
        #endregion

        #region Commands

        #endregion

        #region Overrides
        public override string ToString() {
            return string.Format(
                @"{0}{1}{0}{2}{0}",
                Properties.Settings.Default.TemplateTokenMarker,
                TemplateName,
                TemplateBackgroundBrush.ToString());
        }
        #endregion
    }
}
