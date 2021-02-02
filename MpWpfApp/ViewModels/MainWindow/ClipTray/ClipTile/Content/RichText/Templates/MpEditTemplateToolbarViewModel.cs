using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpEditTemplateToolbarViewModel : MpViewModelBase {
        #region Private Variables
        private Hyperlink _selectedTemplateHyperlink = null;
        private string _originalText = string.Empty;
        private string _originalTemplateName = string.Empty;
        private TextRange _originalSelection = null;
        private Brush _originalTemplateColor = Brushes.Pink;
        #endregion

        #region View Models        
        private MpClipTileViewModel _clipTileViewModel = null;
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

        private MpTemplateHyperlinkViewModel _selectedTemplateHyperlinkViewModel = null;
        public MpTemplateHyperlinkViewModel SelectedTemplateHyperlinkViewModel {
            get {
                return _selectedTemplateHyperlinkViewModel;
            }
            set {
                if(_selectedTemplateHyperlinkViewModel != value) {
                    _selectedTemplateHyperlinkViewModel = value;
                    OnPropertyChanged(nameof(SelectedTemplateHyperlinkViewModel));
                }
            }
            //get {
            //    if(ClipTileViewModel == null ||
            //        ClipTileViewModel.TemplateHyperlinkCollectionViewModel == null || 
            //        ClipTileViewModel.TemplateHyperlinkCollectionViewModel.Count == 0) {
            //        return null;
            //    }
            //    return ClipTileViewModel.TemplateHyperlinkCollectionViewModel.SelectedTemplateHyperlinkViewModel;
            //}
            //set {
            //    if (ClipTileViewModel != null && 
            //        ClipTileViewModel.TemplateHyperlinkCollectionViewModel != null && 
            //        SelectedTemplateHyperlinkViewModel != value) {
            //        ClipTileViewModel.TemplateHyperlinkCollectionViewModel.SelectedTemplateHyperlinkViewModel = value;

            //        OnPropertyChanged(nameof(SelectedTemplateHyperlinkViewModel));
            //    }
            //}
        }
        #endregion

        #region Properties       

        #region Layout Properties

        public double SelectedTemplateNameTextBoxBorderBrushThickness {
            get {
                if (string.IsNullOrEmpty(ValidationText)) {
                    return 1;
                }
                return 3;
            }
        }
        #endregion
         
        #region Visibility Properties        

        public Visibility ValidationVisibility {
            get {
                if (string.IsNullOrEmpty(ValidationText)) {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }
        }
        #endregion

        #region Brush Properties
        public Brush SelectedTemplateNameTextBoxBorderBrush {
            get {
                if (string.IsNullOrEmpty(ValidationText)) {
                    return Brushes.Black;
                }
                return Brushes.Red;
            }
        }
        #endregion

        #region State Properties
        public bool IsSelectedNewTemplate {
            get {
                return string.IsNullOrEmpty(_originalTemplateName);
            }
        }        
        #endregion

        #region Business Logic Properties
        private string _validationText = string.Empty;
        public string ValidationText {
            get {
                return _validationText;
            }
            set {
                if (_validationText != value) {
                    _validationText = value;
                    OnPropertyChanged(nameof(ValidationText));
                    OnPropertyChanged(nameof(ValidationVisibility));
                    OnPropertyChanged(nameof(SelectedTemplateNameTextBoxBorderBrush));
                    OnPropertyChanged(nameof(SelectedTemplateNameTextBoxBorderBrushThickness)); ;
                }
            }
        }
        #endregion

        #region Model Properties

        #endregion

        #endregion

        #region Public Methods

        public MpEditTemplateToolbarViewModel(MpClipTileViewModel ctvm) : base() {
            ClipTileViewModel = ctvm;
            
            ClipTileViewModel.TemplateHyperlinkCollectionViewModel.PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(ClipTileViewModel.TemplateHyperlinkCollectionViewModel.SelectedTemplateHyperlinkViewModel):
                        //thlvm.IsSelected is triggered on mouse click, this brings up edit toolbar
                        SetTemplate(ClipTileViewModel.TemplateHyperlinkCollectionViewModel.SelectedTemplateHyperlinkViewModel,true);
                        break;
                }
            };
        }

        public void EditTemplateToolbarBorder_Loaded(object sender, RoutedEventArgs args) {
            var editTemplateToolbarBorderGrid = (Grid)sender;
            var editTemplateToolbarBorder = editTemplateToolbarBorderGrid.GetVisualAncestor<Border>();
            var templateColorButton = (Button)editTemplateToolbarBorder.FindName("TemplateColorButton");
            var cb = (MpClipBorder)editTemplateToolbarBorder.GetVisualAncestor<MpClipBorder>();
            var rtbc = (Canvas)cb.FindName("ClipTileRichTextBoxCanvas");
            var rtb = rtbc.FindName("ClipTileRichTextBox") as RichTextBox;
            var titleIconImageButton = (Button)cb.FindName("ClipTileAppIconImageButton");
            var titleSwirl = (Image)cb.FindName("TitleSwirl");
            var tb = (TextBox)editTemplateToolbarBorder.FindName("TemplateNameEditorTextBox");

            templateColorButton.Click += (s, e) => {
                var colorMenuItem = new MenuItem();
                var colorContextMenu = new ContextMenu();
                colorContextMenu.Items.Add(colorMenuItem);
                MpHelpers.Instance.SetColorChooserMenuItem(
                    colorContextMenu,
                    colorMenuItem,
                    (s1, e1) => {
                        foreach(var thlvm in ClipTileViewModel.TemplateHyperlinkCollectionViewModel) {
                            if(thlvm.TemplateName == SelectedTemplateHyperlinkViewModel.TemplateName) {
                                thlvm.TemplateBrush = ((Border)s1).Background;
                            }
                        }
                    },
                    MpHelpers.Instance.GetColorColumn(SelectedTemplateHyperlinkViewModel.TemplateBrush),
                    MpHelpers.Instance.GetColorRow(SelectedTemplateHyperlinkViewModel.TemplateBrush)
                );
                templateColorButton.ContextMenu = colorContextMenu;
                colorContextMenu.PlacementTarget = templateColorButton;
                colorContextMenu.IsOpen = true;
            };

            tb.TextChanged += (s, e) => {
                if(SelectedTemplateHyperlinkViewModel != null) {
                    foreach (var thlvm in ClipTileViewModel.TemplateHyperlinkCollectionViewModel) {
                        if (thlvm.CopyItemTemplateId == SelectedTemplateHyperlinkViewModel.CopyItemTemplateId) {
                            thlvm.TemplateName = tb.Text;
                        }
                    }
                    Validate();
                }
            };

            ClipTileViewModel.PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(ClipTileViewModel.IsEditingTemplate):
                        double rtbBottomMax = ClipTileViewModel.TileContentHeight;
                        double rtbBottomMin = ClipTileViewModel.TileContentHeight - ClipTileViewModel.EditTemplateToolbarHeight;

                        double editTemplateToolbarTopMax = ClipTileViewModel.TileContentHeight;
                        double editTemplateToolbarTopMin = ClipTileViewModel.TileContentHeight - ClipTileViewModel.EditTemplateToolbarHeight + 5;
                        
                        if (ClipTileViewModel.IsEditingTemplate) {
                            ClipTileViewModel.EditTemplateToolbarVisibility = Visibility.Visible;
                        } else if(!Validate()) {
                            //occurs if template name is invalid and user clicks away from app or tile
                            CancelCommand.Execute(null);
                        } else {
                            OkCommand.Execute(null);
                        }

                        MpHelpers.Instance.AnimateDoubleProperty(
                            ClipTileViewModel.IsEditingTemplate ? rtbBottomMax : rtbBottomMin,
                            ClipTileViewModel.IsEditingTemplate ? rtbBottomMin : rtbBottomMax,
                            Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
                            rtb,
                            Canvas.BottomProperty,
                            (s1, e44) => {

                            });

                        MpHelpers.Instance.AnimateDoubleProperty(
                            ClipTileViewModel.IsEditingTemplate ? editTemplateToolbarTopMax : editTemplateToolbarTopMin,
                            ClipTileViewModel.IsEditingTemplate ? editTemplateToolbarTopMin : editTemplateToolbarTopMax,
                            Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
                            editTemplateToolbarBorder,
                            Canvas.TopProperty,
                            (s1, e44) => {
                                if (!ClipTileViewModel.IsEditingTemplate) {
                                    ClipTileViewModel.EditTemplateToolbarVisibility = Visibility.Collapsed;
                                    ResetState();
                                } else {
                                    tb.Focus();
                                    tb.SelectAll();
                                }
                            });
                        break;
                }
            };

            rtb.PreviewMouseLeftButtonDown += (s1, e1) => {
                if(ClipTileViewModel.IsEditingTemplate) {
                    //clicking out of edit template toolbar performs Ok Command (save template & hide toolbar)
                    OkCommand.Execute(null);
                }
            };
        }

        public void SetTemplate(MpTemplateHyperlinkViewModel ttcvm, bool isEditMode) {
            //cases
            //1. a new template is being created (null,true)
            //2. an existing template is being referenced (tvm,false)
            //3. an existing template is being edited (tvm, true)
            _originalText = _originalTemplateName = string.Empty;
            _originalTemplateColor = null;
            _selectedTemplateHyperlink = null;
            _originalSelection = ClipTileViewModel.GetRtb().Selection;
            SelectedTemplateHyperlinkViewModel = null;
            if (ttcvm == null) {
                //for new template create the vm but wait to add it in OkCommand
                SelectedTemplateHyperlinkViewModel = new MpTemplateHyperlinkViewModel(ClipTileViewModel, null);
                _originalText = ClipTileViewModel.GetRtb().Selection.Text;
                _selectedTemplateHyperlink = MpHelpers.Instance.CreateTemplateHyperlink(SelectedTemplateHyperlinkViewModel, ClipTileViewModel.GetRtb().Selection);
                ClipTileViewModel.GetRtb().Selection.Select(_selectedTemplateHyperlink.ElementStart, _selectedTemplateHyperlink.ElementEnd);
            } else {
                _originalTemplateName = ttcvm.TemplateName;
                _originalTemplateColor = ttcvm.TemplateBrush;
                if(isEditMode) {
                    SelectedTemplateHyperlinkViewModel = ttcvm;
                } else {
                    SelectedTemplateHyperlinkViewModel = new MpTemplateHyperlinkViewModel(ClipTileViewModel, ttcvm.CopyItemTemplate);
                    _selectedTemplateHyperlink = MpHelpers.Instance.CreateTemplateHyperlink(SelectedTemplateHyperlinkViewModel, ClipTileViewModel.GetRtb().Selection);
                    ClipTileViewModel.GetRtb().Selection.Select(_selectedTemplateHyperlink.ElementStart, _selectedTemplateHyperlink.ElementEnd);
                }
                
            }
            ClipTileViewModel.IsEditingTemplate = isEditMode;

            if (!ClipTileViewModel.IsEditingTemplate) {
                MpHelpers.Instance.CreateTemplateHyperlink(SelectedTemplateHyperlinkViewModel, ClipTileViewModel.GetRtb().Selection);
                OkCommand.Execute(null);
            }
        }
        #endregion

        #region Private Methods
        private void ResetState() {
            SelectedTemplateHyperlinkViewModel = null;
            ValidationText = string.Empty;
            _selectedTemplateHyperlink = null;
            _originalText = string.Empty;
            _originalTemplateName = string.Empty;
            _originalSelection = null;
            _originalTemplateColor = Brushes.Pink;
        }

        private bool Validate() {
            if (SelectedTemplateHyperlinkViewModel == null) {
                return true;
            }
            if (string.IsNullOrEmpty(SelectedTemplateHyperlinkViewModel.TemplateName)) {
                ValidationText = "Name cannot be empty!";
                return false;
            }
            //if new name is a duplicate of another just delete this one and set it to the duplicate
            //var dupTokenHyperlink = ClipTileViewModel.TemplateHyperlinkCollectionViewModel.Where(x => x.TemplateName == SelectedTemplateHyperlinkViewModel.TemplateName && x.CopyItemTemplateId != SelectedTemplateHyperlinkViewModel.CopyItemTemplateId).ToList();
            MpTemplateHyperlinkViewModel dthlvm = null;
            foreach(var thlvm in ClipTileViewModel.TemplateHyperlinkCollectionViewModel.UniqueTemplateHyperlinkViewModelList) {
                //check if another template has this name
                if(thlvm != SelectedTemplateHyperlinkViewModel &&
                   thlvm.CopyItemTemplateId != SelectedTemplateHyperlinkViewModel.CopyItemTemplateId &&
                   thlvm.TemplateName == SelectedTemplateHyperlinkViewModel.TemplateName) {
                    dthlvm = thlvm;
                }
            }
            if (dthlvm != null) {
                ValidationText = SelectedTemplateHyperlinkViewModel.TemplateDisplayName + " already exists";
                return false;
            }
            var templateNameRanges = MpHelpers.Instance.FindStringRangesFromPosition(ClipTileViewModel.GetRtb().Document.ContentStart, SelectedTemplateHyperlinkViewModel.TemplateName, true);
            foreach (var tnr in templateNameRanges) {
                //check if templatename exists in the document
                var thlr = (Hyperlink)MpHelpers.Instance.FindParentOfType(tnr.Start.Parent, typeof(Hyperlink));
                if (SelectedTemplateHyperlinkViewModel.TemplateHyperlinkRange != null &&

                   (thlr == null || !thlr.ElementStart.IsInSameDocument(SelectedTemplateHyperlinkViewModel.TemplateHyperlinkRange.Start)) &&
                  !thlr.Equals(SelectedTemplateHyperlinkViewModel.TemplateHyperlinkRange)) {
                    //the first condition will occur when use is adding an already created template
                    //the second condition ensures the found range is not part of a template which the earlier loop would have been detected
                    //then the or ensures IF the first condition is non-null (a link with the template name) it won't be part of the selected thlvm
                    ValidationText = SelectedTemplateHyperlinkViewModel.TemplateName + " already exists in text";
                    return false;
                }
            }

            ValidationText = string.Empty;
            return true;
        }
        
        #endregion

        #region Commands        
        private RelayCommand _cancelCommand;
        public ICommand CancelCommand {
            get {
                if (_cancelCommand == null) {
                    _cancelCommand = new RelayCommand(Cancel);
                }
                return _cancelCommand;
            }
        }
        private void Cancel() {
            ClipTileViewModel.GetRtb().Focus();
            if (IsSelectedNewTemplate) {
                var selectionStart = SelectedTemplateHyperlinkViewModel.TemplateHyperlinkRange.Start;
                SelectedTemplateHyperlinkViewModel.Dispose(false);
                _originalSelection.Text = _originalText;
                var sr = MpHelpers.Instance.FindStringRangeFromPosition(selectionStart, _originalText, true);
                ClipTileViewModel.GetRtb().Selection.Select(sr.Start, sr.End);                
            } else {
                //restore original name/color to datacontext
                SelectedTemplateHyperlinkViewModel.TemplateName = _originalTemplateName;
                SelectedTemplateHyperlinkViewModel.TemplateBrush = _originalTemplateColor;
            }
            ClipTileViewModel.IsEditingTemplate = false;
        }

        private RelayCommand _okCommand;
        public ICommand OkCommand {
            get {
                if (_okCommand == null) {
                    _okCommand = new RelayCommand(Ok, CanOk);
                }
                return _okCommand;
            }
        }
        private bool CanOk() {
            return Validate();
        }
        private void Ok() {
            if(IsSelectedNewTemplate) {
                ClipTileViewModel.TemplateHyperlinkCollectionViewModel.Add(SelectedTemplateHyperlinkViewModel);
            } else {
                foreach(var thlvm in ClipTileViewModel.TemplateHyperlinkCollectionViewModel) {
                    if(thlvm.TemplateName == SelectedTemplateHyperlinkViewModel.TemplateName) {
                        thlvm.CopyItemTemplate.WriteToDatabase();
                    }
                }
            }

            SelectedTemplateHyperlinkViewModel.IsSelected = true;
            ClipTileViewModel.IsEditingTemplate = false;

            ClipTileViewModel.GetRtb().Focus();
        }
        #endregion

    }
}
 