using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpTemplateTokenCollectionViewModel : MpObservableCollectionViewModel<MpTemplateHyperlinkViewModel> {
        #region Properties

        private bool _isTemplateReadyToPaste = false;
        public bool IsTemplateReadyToPaste {
            get {
                return _isTemplateReadyToPaste;
            }
            set {
                if (_isTemplateReadyToPaste != value) {
                    _isTemplateReadyToPaste = value;
                    OnPropertyChanged(nameof(IsTemplateReadyToPaste));
                }
            }
        }

        private bool _isCurrentTemplateTextBoxFocused = false;
        public bool IsCurrentTemplateTextBoxFocused {
            get {
                return _isCurrentTemplateTextBoxFocused;
            }
            set {
                if (_isCurrentTemplateTextBoxFocused != value) {
                    _isCurrentTemplateTextBoxFocused = value;
                    OnPropertyChanged(nameof(IsCurrentTemplateTextBoxFocused));
                }
            }
        }
        private bool _isPastingTemplateTile = false;
        public bool IsPastingTemplateTile {
            get {
                return _isPastingTemplateTile;
            }
            set {
                if(_isPastingTemplateTile != value) {
                    _isPastingTemplateTile = value;
                    OnPropertyChanged(nameof(IsPastingTemplateTile));
                }
            }
        }
        public Visibility PasteTemplateToolbarVisibility {
            get {
                if (IsPastingTemplateTile) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility ClearAllTemplateToolbarButtonVisibility {
            get {
                foreach (var templateLookup in TemplateTokenLookupDictionary) {
                    if (!string.IsNullOrEmpty(templateLookup.Value)) {
                        return Visibility.Visible;
                    }
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility ClearCurrentTemplateTextboxButtonVisibility {
            get {
                if (CurrentTemplateText.Length > 0 &&
                    CurrentTemplateText != CurrentTemplateTextBoxPlaceHolderText) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        public Visibility TemplateNavigationButtonStackVisibility {
            get {
                if (TemplateTokenLookupDictionary.Count > 0) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        private int _currentTemplateLookupIdx = 0;
        public int CurrentTemplateLookupIdx {
            get {
                return _currentTemplateLookupIdx;
            }
            set {
                if (_currentTemplateLookupIdx != value) {
                    _currentTemplateLookupIdx = value;
                    OnPropertyChanged(nameof(CurrentTemplateLookupIdx));
                    OnPropertyChanged(nameof(CurrentTemplateText));
                    OnPropertyChanged(nameof(CurrentTemplateTextBoxFontStyle));
                    OnPropertyChanged(nameof(CurrentTemplateTextBrush));
                    OnPropertyChanged(nameof(ClearCurrentTemplateTextboxButtonVisibility));
                    OnPropertyChanged(nameof(CurrentTemplateTextBoxPlaceHolderText));
                }
            }
        }

        public string CurrentTemplateText {
            get {
                if (TemplateTokenLookupDictionary.Count == 0) {
                    return string.Empty;
                }
                var curTemplateText = TemplateTokenLookupDictionary.ElementAt(CurrentTemplateLookupIdx).Value;
                if (string.IsNullOrEmpty(curTemplateText) && !IsCurrentTemplateTextBoxFocused) {
                    return CurrentTemplateTextBoxPlaceHolderText;   
                }
                return curTemplateText;
            }
            set {
                if (!string.IsNullOrEmpty(value) && TemplateTokenLookupDictionary.Count > 0 && TemplateTokenLookupDictionary.ElementAt(CurrentTemplateLookupIdx).Value != value) {
                    var templateName = TemplateTokenLookupDictionary.ElementAt(CurrentTemplateLookupIdx).Key;
                    TemplateTokenLookupDictionary[templateName] = value;

                    bool canPaste = true;
                    foreach (var thlvm in this) {
                        if (thlvm.TemplateName == templateName) {
                            thlvm.IsEditMode = true;
                            thlvm.IsSelected = true;
                            thlvm.WasTypeViewed = true;
                            thlvm.TemplateText = TemplateTokenLookupDictionary[templateName];
                        } else {
                            thlvm.IsEditMode = false;
                            thlvm.IsSelected = false;
                        }
                        if (thlvm.WasTypeViewed == false) {
                            canPaste = false;
                        }
                    }
                    if (canPaste) {
                        IsTemplateReadyToPaste = true;
                    }
                    OnPropertyChanged(nameof(CurrentTemplateText));
                    //OnPropertyChanged(nameof(TemplateTokens));
                    OnPropertyChanged(nameof(CurrentTemplateTextBoxFontStyle));
                    OnPropertyChanged(nameof(CurrentTemplateTextBrush));
                    OnPropertyChanged(nameof(ClearCurrentTemplateTextboxButtonVisibility));
                    OnPropertyChanged(nameof(ClearAllTemplateToolbarButtonVisibility));
                }
            }
        }

        public string CurrentTemplateTextBoxPlaceHolderText {
            get {
                if (TemplateTokenLookupDictionary.Count == 0) {
                    return string.Empty;
                }
                return TemplateTokenLookupDictionary.ElementAt(CurrentTemplateLookupIdx).Key + "...";
            }
        }

        public Brush CurrentTemplateTextBrush {
            get {
                if (CurrentTemplateText != CurrentTemplateTextBoxPlaceHolderText) {
                    return Brushes.Black;
                }
                return Brushes.DimGray;
            }
        }

        public FontStyle CurrentTemplateTextBoxFontStyle {
            get {
                if (CurrentTemplateText == CurrentTemplateTextBoxPlaceHolderText) {
                    return FontStyles.Italic;
                }
                return FontStyles.Normal;
            }
        }

        private Dictionary<string, string> _templateTokenLookupDictionary = new Dictionary<string, string>();
        public Dictionary<string, string> TemplateTokenLookupDictionary {
            get {
                return _templateTokenLookupDictionary;
            }
            set {
                if (_templateTokenLookupDictionary != value) {
                    _templateTokenLookupDictionary = value;
                    OnPropertyChanged(nameof(TemplateTokenLookupDictionary));
                    OnPropertyChanged(nameof(TemplateNavigationButtonStackVisibility));
                }
            }
        }

        public bool IsPasteToolbarAnimating { get; set; } = false;
        #endregion
        #region Public Methods

        #endregion
    }
}
