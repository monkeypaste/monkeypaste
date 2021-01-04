using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpTemplateTokenCollectionViewModel : MpObservableCollectionViewModel<MpTemplateHyperlinkViewModel> {
        #region View Models
        private MpPasteTemplateToolbarViewModel _pasteTemplateToolbarViewModel = new MpPasteTemplateToolbarViewModel();
        public MpPasteTemplateToolbarViewModel PasteTemplateToolbarViewModel {
            get {
                return _pasteTemplateToolbarViewModel;
            }
            set {
                if (_pasteTemplateToolbarViewModel != value) {
                    _pasteTemplateToolbarViewModel = value;
                    OnPropertyChanged(nameof(PasteTemplateToolbarViewModel));
                }
            }
        }
        #endregion

        #region Properties

        #region Business Logic Properties
        private string _templateText = string.Empty;
        public string TemplateText {
            get {
                return _templateText;
            }
            set {
                if (_templateText != value) {
                    _templateText = value;
                    OnPropertyChanged(nameof(TemplateText));
                    foreach(var thvm in this) {
                        thvm.OnPropertyChanged(nameof(thvm.TemplateDisplayValue));
                        thvm.OnPropertyChanged(nameof(thvm.TemplateBorderWidth));
                        thvm.OnPropertyChanged(nameof(thvm.TemplateBorderHeight));
                        thvm.OnPropertyChanged(nameof(thvm.TemplateTextBlockHeight));
                        thvm.OnPropertyChanged(nameof(thvm.TemplateTextBlockWidth));
                    }
                    
                }
            }
        }

        private bool _isSelected = false;
        public bool IsSelected {
            get {
                return _isSelected;
            }
            set {
                if (_isSelected != value) {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                    foreach (var thvm in this) {
                        thvm.OnPropertyChanged(nameof(thvm.TemplateBorderBrush));
                        thvm.OnPropertyChanged(nameof(thvm.TemplateBorderWidth));
                        thvm.OnPropertyChanged(nameof(thvm.TemplateBorderHeight));
                        thvm.OnPropertyChanged(nameof(thvm.TemplateTextBlockHeight));
                        thvm.OnPropertyChanged(nameof(thvm.TemplateTextBlockWidth));
                    }
                    if(!IsSelected) {
                        FocusedTemplateHyperlinkViewModel = null;
                    }
                }
            }
        }

        private MpTemplateHyperlinkViewModel _focusedTemplateHyperlinkViewModel = null;
        public MpTemplateHyperlinkViewModel FocusedTemplateHyperlinkViewModel {
            get {
                return _focusedTemplateHyperlinkViewModel;
            }
            set {
                if (_focusedTemplateHyperlinkViewModel != value) {
                    _focusedTemplateHyperlinkViewModel = value;
                    OnPropertyChanged(nameof(FocusedTemplateHyperlinkViewModel));
                }
            }
        }
        #endregion

        #region Model Properties
        public int CopyItemTemplateId {
            get {
                return CopyItemTemplate.CopyItemTemplateId;
            }
        }

        public int CopyItemId {
            get {
                return CopyItemTemplate.CopyItemId;
            }
        }

        public string TemplateName {
            get {
                return CopyItemTemplate.TemplateName;
            }
            set {
                if(CopyItemTemplate.TemplateName != value) {
                    CopyItemTemplate.TemplateName = value;
                    OnPropertyChanged(nameof(TemplateName));
                }
            }
        }

        public Brush TemplateBrush {
            get {
                return CopyItemTemplate.TemplateColor;
            }
            set {
                if(CopyItemTemplate.TemplateColor != value) {
                    CopyItemTemplate.TemplateColor = value;
                    OnPropertyChanged(nameof(TemplateBrush));
                }
            }
        }
        private MpCopyItemTemplate _copyItemTemplate = new MpCopyItemTemplate();
        public MpCopyItemTemplate CopyItemTemplate {
            get {
                return _copyItemTemplate;
            }
            set {
                if(_copyItemTemplate != value) {
                    _copyItemTemplate = value;
                    OnPropertyChanged(nameof(CopyItemTemplate));
                    OnPropertyChanged(nameof(TemplateBrush));
                    OnPropertyChanged(nameof(TemplateName));
                    OnPropertyChanged(nameof(CopyItemTemplateId));
                    OnPropertyChanged(nameof(CopyItemId));
                }
            }
        }
        #endregion
        #endregion

        #region Public Methods
        public MpTemplateTokenCollectionViewModel() : this(new MpPasteTemplateToolbarViewModel(),new MpCopyItemTemplate()) { }

        public MpTemplateTokenCollectionViewModel(MpPasteTemplateToolbarViewModel pttbvm, MpCopyItemTemplate cit) : base() {
            PasteTemplateToolbarViewModel = pttbvm;
            foreach(var ttr in cit.TemplateTextRangeList) {
                this.Add(new MpTemplateHyperlinkViewModel(this,ttr));
            }
        }
        #endregion

        #region Commands
        private RelayCommand _editTemplateCommand;
        public ICommand EditTemplateCommand {
            get {
                if (_editTemplateCommand == null) {
                    _editTemplateCommand = new RelayCommand(EditTemplate, CanEditTemplate);
                }
                return _editTemplateCommand;
            }
        }
        private bool CanEditTemplate() {
            return PasteTemplateToolbarViewModel.ClipTileViewModel.IsEditingTile;
        }
        private void EditTemplate() {
            // _rtb.Selection.Select(hl.ElementStart, hl.ElementEnd);
            //MpTemplateTokenEditModalWindowViewModel.ShowTemplateTokenEditModalWindow(_rtb, hl, true);
        }
        #endregion

        #region Overrides
        public new void Add(MpTemplateHyperlinkViewModel thlvm) {
            base.Add(thlvm);
            thlvm.TemplateTextRange.WriteToDatabase();
            CopyItemTemplate.TemplateTextRangeList.Add(thlvm.TemplateTextRange);
            CopyItemTemplate.WriteToDatabase();

            var rtb = PasteTemplateToolbarViewModel.ClipTileViewModel.GetRtb();
            var thlb = new MpTemplateHyperlinkBorder(thlvm);
            var container = new InlineUIContainer(thlb);
            new Hyperlink(container, thlvm.TemplateStart);            
        }

        public new void Remove(MpTemplateHyperlinkViewModel thlvm) {
            base.Remove(thlvm);
            CopyItemTemplate.TemplateTextRangeList.Remove(thlvm.TemplateTextRange);
            thlvm.TemplateTextRange.DeleteFromDatabase();
        }
        #endregion
    }
}
