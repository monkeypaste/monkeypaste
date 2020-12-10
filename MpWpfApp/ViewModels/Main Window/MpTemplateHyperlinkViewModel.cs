using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpTemplateHyperlinkViewModel : MpViewModelBase {
        #region Private Variables

        #endregion

        #region Properties
        private int _templateIdx = -1;
        public int TemplateIdx {
            get { 
                return _templateIdx; 
            }
            set {
                if (_templateIdx != value) {
                    _templateIdx = value;
                    OnPropertyChanged(nameof(TemplateIdx));
                }
            }
        }

        public Brush TemplateForegroundColor { 
            get {
                if(MpHelpers.IsBright(((SolidColorBrush)TemplateBackgroundColor).Color)) {
                    return Brushes.Black;
                }
                return Brushes.White;
            }
        }

        private Brush _templateBackgroundColor = Brushes.Pink;
        public Brush TemplateBackgroundColor {
            get {
                return _templateBackgroundColor;
            }
            set {
                if (_templateBackgroundColor != value) {
                    _templateBackgroundColor = value;
                    OnPropertyChanged(nameof(TemplateBackgroundColor));
                    OnPropertyChanged(nameof(TemplateForegroundColor));
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
                }
            }
        }
        #endregion

        #region Public Methods

        #endregion

        #region Commands

        #endregion

        #region Overrides
        public override string ToString() {
            return string.Format(
                @"{0}{1}{0}{2}{0}",
                Properties.Settings.Default.TemplateTokenMarker,
                TemplateName,
                TemplateBackgroundColor.ToString());
        }
        #endregion
    }
}
