using GalaSoft.MvvmLight.CommandWpf;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpSubTextTokenViewModel : MpViewModelBase {
        #region Statics
        private static List<Brush> _TokenGroupColorLookupList = new List<Brush>() {
            Brushes.Purple,
            Brushes.Magenta,
            Brushes.OrangeRed,
            Brushes.Maroon,
            Brushes.Moccasin
        };
        #endregion

        #region Properties
        private Brush _templateBorderBackgroundBrush = Brushes.Yellow;
        public Brush TemplateBorderBackgroundBrush {
            get {
                return _templateBorderBackgroundBrush;
            }
            set {
                if (_templateBorderBackgroundBrush != value) {
                    _templateBorderBackgroundBrush = value;
                    OnPropertyChanged(nameof(TemplateBorderBackgroundBrush));
                }
            }
        }

        private int _subTextTokenId = 0;
        public int SubTextTokenId {
            get {
                return _subTextTokenId;
            }
            set {
                if (_subTextTokenId != value) {
                    _subTextTokenId = value;
                    OnPropertyChanged(nameof(SubTextTokenId));
                }
            }
        }

        private int _copyItemId = 0;
        public int CopyItemId {
            get {
                return _copyItemId;
            }
            set {
                if (_copyItemId != value) {
                    _copyItemId = value;
                    OnPropertyChanged(nameof(CopyItemId));
                }
            }
        }

        private string _tokenText = string.Empty;
        public string TokenText {
            get {
                return _tokenText;
            }
            set {
                if (_tokenText != value) {
                    _tokenText = value;
                    OnPropertyChanged(nameof(TokenText));
                }
            }
        }

        private string _tokenName = string.Empty;
        public string TokenName {
            get {
                return _tokenName;
            }
            set {
                if (_tokenName != value) {
                    _tokenName = value;
                    OnPropertyChanged(nameof(TokenName));
                }
            }
        }

        private MpSubTextTokenType _tokenType = MpSubTextTokenType.None;
        public MpSubTextTokenType TokenType {
            get {
                return _tokenType;
            }
            set {
                if (_tokenType != value) {
                    _tokenType = value;
                    OnPropertyChanged(nameof(TokenType));
                }
            }
        }

        private int _startIdx = 0;
        public int StartIdx {
            get {
                return _startIdx;
            }
            set {
                if (_startIdx != value) {
                    _startIdx = value;
                    OnPropertyChanged(nameof(StartIdx));
                }
            }
        }

        private int _endIdx = 0;
        public int EndIdx {
            get {
                return _endIdx;
            }
            set {
                if (_endIdx != value) {
                    _endIdx = value;
                    OnPropertyChanged(nameof(EndIdx));
                }
            }
        }

        private int _blockIdx = 0;
        public int BlockIdx {
            get {
                return _blockIdx;
            }
            set {
                if (_blockIdx != value) {
                    _blockIdx = value;
                    OnPropertyChanged(nameof(CopyItemId));
                }
            }
        }

        private int _inlineIdx = 0;
        public int InlineIdx {
            get {
                return _inlineIdx;
            }
            set {
                if (_inlineIdx != value) {
                    _inlineIdx = value;
                    OnPropertyChanged(nameof(CopyItemId));
                }
            }
        }

        //private bool _isSelected = false;
        //public bool IsSelected {
        //    get {
        //        return _isSelected;
        //    }
        //    set {
        //        if (_isSelected != value) {
        //            _isSelected = value;
        //            OnPropertyChanged(nameof(IsSelected));
        //        }
        //    }
        //}

        private bool _isDefined = false;
        public bool IsDefined {
            get {
                return _isDefined;
            }
            set {
                if (_isDefined != value) {
                    _isDefined = value;
                    OnPropertyChanged(nameof(IsDefined));
                }
            }
        }

        private bool _isEditing = false;
        public bool IsEditing {
            get {
                return _isEditing;
            }
            set {
                if (_isEditing != value) {
                    _isEditing = value;
                    OnPropertyChanged(nameof(IsEditing));
                }
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
                }
            }
        }

        private Brush _templateTextColor = Brushes.White;
        public Brush TemplateTextColor {
            get {
                return _templateTextColor;
            }
            set {
                if (_templateTextColor != value) {
                    _templateTextColor = value;
                    OnPropertyChanged(nameof(TemplateTextColor));
                }
            }
        }

        private Visibility _textBoxVisibility = Visibility.Collapsed;
        public Visibility TextBoxVisibility {
            get {
                return _textBoxVisibility;
            }
            set {
                if (_textBoxVisibility != value) {
                    _textBoxVisibility = value;
                    OnPropertyChanged(nameof(TextBoxVisibility));
                }
            }
        }

        private Visibility _textBlockVisibility = Visibility.Visible;
        public Visibility TextBlockVisibility {
            get {
                return _textBlockVisibility;
            }
            set {
                if (_textBlockVisibility != value) {
                    _textBlockVisibility = value;
                    OnPropertyChanged(nameof(TextBlockVisibility));
                }
            }
        }

        public double TemplateBorderHeight {
            get {
                //assumes Tag Margin is 5
                return MpMeasurements.Instance.FilterMenuHeight - (5 * 2);
            }
        }

        public double TemplateFontSize {
            get {
                return 16;
            }
        }

        private MpSubTextToken _subTextToken = null;
        public MpSubTextToken SubTextToken {
            get {
                return _subTextToken;
            }
            set {
                if (_subTextToken != value) {
                    _subTextToken = value;
                    OnPropertyChanged(nameof(SubTextToken));
                }
            }
        }
        #endregion

        #region Public Methods
        public MpSubTextTokenViewModel() { }

        public MpSubTextTokenViewModel(MpSubTextToken stt) {
            PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(SubTextToken):
                        SubTextTokenId = SubTextToken.SubTextTokenId;
                        CopyItemId = SubTextToken.CopyItemId;
                        TokenText = SubTextToken.TokenText;
                        TokenName = SubTextToken.TokenName;
                        TokenType = SubTextToken.TokenType;
                        StartIdx = SubTextToken.StartIdx;
                        EndIdx = SubTextToken.EndIdx;
                        BlockIdx = SubTextToken.BlockIdx;
                        InlineIdx = SubTextToken.InlineIdx;
                        TemplateBorderBackgroundBrush = _TokenGroupColorLookupList[InlineIdx];
                        break;
                    case nameof(SubTextTokenId):
                        SubTextToken.SubTextTokenId = SubTextTokenId;
                        break;
                    case nameof(CopyItemId):
                        SubTextToken.CopyItemId = CopyItemId;
                        break;
                    case nameof(TokenText):
                        SubTextToken.TokenText = TokenText;
                        break;
                    case nameof(TokenName):
                        SubTextToken.TokenName = TokenName;
                        break;
                    case nameof(TokenType):
                        SubTextToken.TokenType = TokenType;
                        break;
                    case nameof(StartIdx):
                        SubTextToken.StartIdx = StartIdx;
                        break;
                    case nameof(EndIdx):
                        SubTextToken.EndIdx = EndIdx;
                        break;
                    case nameof(BlockIdx):
                        SubTextToken.BlockIdx = BlockIdx;
                        break;
                    case nameof(InlineIdx):
                        SubTextToken.InlineIdx = InlineIdx;
                        break;
                    case nameof(IsFocused):
                        return;
                        break;
                    case nameof(IsEditing):
                        if (IsEditing) {
                            //show textbox and select all text
                            TextBoxVisibility = Visibility.Visible;
                            TextBlockVisibility = Visibility.Collapsed;
                            //IsFocused = true;
                            //TagTextBox?.SelectAll();
                        } else {
                            TextBoxVisibility = Visibility.Collapsed;
                            TextBlockVisibility = Visibility.Visible;
                            SubTextToken.TokenText = TokenText;
                            //SubTextToken.WriteToDatabase();
                            //IsFocused = false;
                        }
                        break;
                    //case nameof(IsSelected):
                    //    if (IsSelected) {
                    //        //TagTextColor = Brushes.White;
                    //        TemplateBorderBackgroundBrush = Brushes.DimGray;
                    //    } else {
                    //        TemplateBorderBackgroundBrush = Brushes.Transparent;
                    //        //TagTextColor = Brushes.LightGray;
                    //    }
                    //    break;
                    case nameof(IsHovering):
                        if (IsHovering) {
                            TemplateBorderBackgroundBrush = Brushes.LightGray;//MpHelpers.GetLighterBrush(_TokenGroupColorLookupList[InlineIdx]);
                                                                              //TagTextColor = Brushes.Black;
                        } else {
                            TemplateBorderBackgroundBrush = _TokenGroupColorLookupList[InlineIdx];
                            //TagTextColor = Brushes.White;
                        }
                        break;
                }
            };

            SubTextToken = stt;
            IsEditing = false;
        }
        
        public void TemplateTextTokenEditableButton_Loaded(object sender, RoutedEventArgs e) {
            var templateTokenBorder = (Border)sender;
            templateTokenBorder.MouseEnter += (s, e1) => {
                IsHovering = true;
            };
            templateTokenBorder.MouseLeave += (s, e1) => {
                IsHovering = false;
            };
            templateTokenBorder.MouseDown += (s, e7) => {
                return;
            };
            templateTokenBorder.LostFocus += (s, e4) => {
                //if (!IsSelected) {
                //    IsEditing = false;
                //}
            };
            templateTokenBorder.PreviewMouseLeftButtonDown += (s, e7) => {
                if (e7.ClickCount == 2) {
                    //RenametemplateTokenCommand.Execute(null);
                }
            };

            var templateTokenTextBox = (TextBox)templateTokenBorder.FindName("TemplateTextBox");
            //this is called 
            templateTokenTextBox.GotFocus += (s, e1) => {
                //templateTokenTextBox.SelectAll();
            };
            templateTokenTextBox.LostFocus += (s, e2) => {
                IsEditing = false;
            };
            //templateTokenTextBox.PreviewKeyDown += MainWindowViewModel.MainWindow_PreviewKeyDown;
            //if tag is created at runtime show tbox w/ all selected
            if (!MainWindowViewModel.IsLoading) {
                //RenameTagCommand.Execute(null);
            } else {
                foreach (var ctvm in MainWindowViewModel.ClipTrayViewModel) {
                    //if (Tag.IsLinkedWithCopyItem(ctvm.CopyItem)) {
                    //    TagClipCount++;
                    //}
                }
            }
        }
        #endregion

        #region Commands
        private RelayCommand _renameTemplateCommand;
        public ICommand RenameTemplateCommand {
            get {
                if (_renameTemplateCommand == null) {
                    _renameTemplateCommand = new RelayCommand(RenameTemplate, CanRenameTemplate);
                }
                return _renameTemplateCommand;
            }
        }
        private bool CanRenameTemplate() {
            return true;
        }
        private void RenameTemplate() {
            //IsSelected = true;
            //IsFocused = true;
            //IsEditing = true;
        }

        private RelayCommand _selectTemplateCommand;
        public ICommand SelectTemplateCommand {
            get {
                if (_selectTemplateCommand == null) {
                    _selectTemplateCommand = new RelayCommand(SelectTemplate);
                }
                return _selectTemplateCommand;
            }
        }
        private void SelectTemplate() {
            //IsSelected = true;
            //IsFocused = true;
        }
        #endregion
    }
}
