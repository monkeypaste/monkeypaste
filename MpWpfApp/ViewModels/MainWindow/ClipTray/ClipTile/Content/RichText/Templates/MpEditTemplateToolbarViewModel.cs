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
using System.Windows.Threading;

namespace MpWpfApp {
    public class MpEditTemplateToolbarViewModel : MpViewModelBase {
        #region Private Variables
        private Hyperlink _selectedTemplateHyperlink = null;
        private string _originalText = string.Empty;
        private string _originalTemplateName = string.Empty;
        private TextRange _originalSelection = null;
        private Brush _originalTemplateColor = Brushes.Pink;

        private Grid _borderGrid = null;

        private bool _wasEdited = false;
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
            //        ClipTileViewModel.RichTextBoxViewModels.SubSelectedRtbvm.TemplateHyperlinkCollectionViewModel == null || 
            //        ClipTileViewModel.RichTextBoxViewModels.SubSelectedRtbvm.TemplateHyperlinkCollectionViewModel.Count == 0) {
            //        return null;
            //    }
            //    return ClipTileViewModel.RichTextBoxViewModels.SubSelectedRtbvm.TemplateHyperlinkCollectionViewModel.SelectedTemplateHyperlinkViewModel;
            //}
            //set {
            //    if (ClipTileViewModel != null && 
            //        ClipTileViewModel.RichTextBoxViewModels.SubSelectedRtbvm.TemplateHyperlinkCollectionViewModel != null && 
            //        SelectedTemplateHyperlinkViewModel != value) {
            //        ClipTileViewModel.RichTextBoxViewModels.SubSelectedRtbvm.TemplateHyperlinkCollectionViewModel.SelectedTemplateHyperlinkViewModel = value;

            //        OnPropertyChanged(nameof(SelectedTemplateHyperlinkViewModel));
            //    }
            //}
        }
        #endregion

        #region Properties       

        #region Controls
        public TextBox SelectedTemplateNameTextBox;
        #endregion
        #region Layout 
        private double _editTemplateBorderCanvasTop = MpMeasurements.Instance.ClipTileContentHeight;
        public double EditTemplateBorderCanvasTop {
            get {
                return _editTemplateBorderCanvasTop;
            }
            set {
                if (_editTemplateBorderCanvasTop != value) {
                    _editTemplateBorderCanvasTop = value;
                    OnPropertyChanged(nameof(EditTemplateBorderCanvasTop));
                }
            }
        }

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
        }

        public void EditTemplateToolbarBorderGrid_Loaded(object sender, RoutedEventArgs args) {
            if (ClipTileViewModel.CopyItemType != MpCopyItemType.RichText && ClipTileViewModel.CopyItemType != MpCopyItemType.Composite) {
                return;
            }
            _borderGrid = (Grid)sender;                 
        }

        public void InitWithRichTextBox(RichTextBox rtb) {
            ClipTileViewModel.RichTextBoxViewModelCollection.SubSelectedRtbvm.TemplateHyperlinkCollectionViewModel.PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(ClipTileViewModel.RichTextBoxViewModelCollection.SubSelectedRtbvm.TemplateHyperlinkCollectionViewModel.SelectedTemplateHyperlinkViewModel):
                        //thlvm.IsSelected is triggered on mouse click, this brings up edit toolbar
                        SetTemplate(ClipTileViewModel.RichTextBoxViewModelCollection.SubSelectedRtbvm.TemplateHyperlinkCollectionViewModel.SelectedTemplateHyperlinkViewModel, true);
                        break;
                }
            };
            var editTemplateToolbarBorder = _borderGrid.GetVisualAncestor<Border>();
            var templateColorButton = (Button)editTemplateToolbarBorder.FindName("TemplateColorButton");
            var cb = (MpClipBorder)editTemplateToolbarBorder.GetVisualAncestor<MpClipBorder>();
            var rtbc = (Canvas)cb.FindName("ClipTileRichTextBoxListBoxGridContainerCanvas");
            var rtblb = (ListBox)cb.FindName("ClipTileRichTextBoxListBox");
            var ctttg = (Grid)cb.FindName("ClipTileTitleTextGrid");
            //var rtb = rtbc.FindName("ClipTileRichTextBox") as RichTextBox;
            var titleIconImageButton = (Button)cb.FindName("ClipTileAppIconImageButton");
            var titleSwirl = (Image)cb.FindName("TitleSwirl");

            SelectedTemplateNameTextBox = (TextBox)editTemplateToolbarBorder.FindName("TemplateNameEditorTextBox");

            templateColorButton.Click += (s, e) => {
                var colorMenuItem = new MenuItem();
                var colorContextMenu = new ContextMenu();
                colorContextMenu.Items.Add(colorMenuItem);
                MpHelpers.Instance.SetColorChooserMenuItem(
                    colorContextMenu,
                    colorMenuItem,
                    (s1, e1) => {
                        if(IsSelectedNewTemplate) {
                            SelectedTemplateHyperlinkViewModel.TemplateBrush = (Brush)((Border)s1).Tag;
                        } else {
                            foreach (var thlvm in ClipTileViewModel.RichTextBoxViewModelCollection.SubSelectedRtbvm.TemplateHyperlinkCollectionViewModel) {
                                if (thlvm.TemplateName == SelectedTemplateHyperlinkViewModel.TemplateName) {
                                    thlvm.TemplateBrush = (Brush)((Border)s1).Tag;
                                }
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

            SelectedTemplateNameTextBox.TextChanged += (s, e) => {
                _wasEdited = true;
                if (SelectedTemplateHyperlinkViewModel != null) {
                    foreach (var thlvm in ClipTileViewModel.RichTextBoxViewModelCollection.SubSelectedRtbvm.TemplateHyperlinkCollectionViewModel) {
                        if (thlvm.CopyItemTemplateId == SelectedTemplateHyperlinkViewModel.CopyItemTemplateId) {
                            thlvm.TemplateName = SelectedTemplateNameTextBox.Text;
                        }
                    }
                    Validate();
                }
            };

            //ClipTileViewModel.PropertyChanged += (s, e) => {
            //    switch (e.PropertyName) {
            //        case nameof(ClipTileViewModel.IsEditingTemplate):
            //            double rtbBottomMax = ClipTileViewModel.TileContentHeight;
            //            double rtbBottomMin = ClipTileViewModel.TileContentHeight - ClipTileViewModel.EditTemplateToolbarHeight;

            //            double editTemplateToolbarTopMax = ClipTileViewModel.TileContentHeight;
            //            double editTemplateToolbarTopMin = ClipTileViewModel.TileContentHeight - ClipTileViewModel.EditTemplateToolbarHeight + 5;

            //            if (ClipTileViewModel.IsEditingTemplate) {
            //                ClipTileViewModel.EditTemplateToolbarVisibility = Visibility.Visible;
            //            } else if (!Validate()) {
            //                //occurs if template name is invalid and user clicks away from app or tile
            //                CancelCommand.Execute(null);
            //            } else {
            //                OkCommand.Execute(null);
            //            }

            //            MpHelpers.Instance.AnimateDoubleProperty(
            //                ClipTileViewModel.IsEditingTemplate ? rtbBottomMax : rtbBottomMin,
            //                ClipTileViewModel.IsEditingTemplate ? rtbBottomMin : rtbBottomMax,
            //                Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
            //                new List<FrameworkElement> { rtb, rtblb },
            //                Canvas.BottomProperty,
            //                (s1, e44) => {

            //                });

            //            MpHelpers.Instance.AnimateDoubleProperty(
            //                ClipTileViewModel.IsEditingTemplate ? editTemplateToolbarTopMax : editTemplateToolbarTopMin,
            //                ClipTileViewModel.IsEditingTemplate ? editTemplateToolbarTopMin : editTemplateToolbarTopMax,
            //                Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
            //                editTemplateToolbarBorder,
            //                Canvas.TopProperty,
            //                (s1, e44) => {
            //                    if (!ClipTileViewModel.IsEditingTemplate) {
            //                        ClipTileViewModel.EditTemplateToolbarVisibility = Visibility.Collapsed;
            //                        ResetState();
            //                    } else {
            //                        tb.Focus();
            //                        tb.SelectAll();
            //                    }
            //                });
            //            break;
            //    }
            //};

            rtb.PreviewMouseLeftButtonDown += (s1, e1) => {
                if (ClipTileViewModel.IsEditingTemplate) {
                    //clicking out of edit template toolbar performs Ok Command (save template & hide toolbar)
                    OkCommand.Execute(null);
                }
            };
        }
        public void Resize(double deltaTemplateTop) {
            EditTemplateBorderCanvasTop += deltaTemplateTop;

            if (ClipTileViewModel.IsEditingTemplate) {
                SelectedTemplateNameTextBox.Focus();
                SelectedTemplateNameTextBox.SelectAll();
            } else if (_wasEdited) {
                ResetState();
                if (!Validate()) {
                    CancelCommand.Execute(null);
                } else {
                    OkCommand.Execute(null);
                }
            }
        }

        public void Animate(
            double deltaTop,
            double tt,
            EventHandler onCompleted,
            double fps = 30,
            DispatcherPriority priority = DispatcherPriority.Render) {
            double fromTop = EditTemplateBorderCanvasTop;
            double toTop = fromTop + deltaTop;
            double dt = (deltaTop / tt) / fps;

            var timer = new DispatcherTimer(priority);
            timer.Interval = TimeSpan.FromMilliseconds(fps);
            timer.Tick += (s, e32) => {
                if (MpHelpers.Instance.DistanceBetweenValues(EditTemplateBorderCanvasTop, toTop) > 0.5) {
                    EditTemplateBorderCanvasTop += dt;
                } else {
                    timer.Stop();
                    if (ClipTileViewModel.IsEditingTemplate) {
                        SelectedTemplateNameTextBox.Focus();
                        SelectedTemplateNameTextBox.SelectAll();
                    } else if(_wasEdited) {
                        ResetState();
                        if (!Validate()) {
                            CancelCommand.Execute(null);
                        } else {
                            OkCommand.Execute(null);
                        }
                    }
                    if (onCompleted != null) {
                        onCompleted.BeginInvoke(this, new EventArgs(), null, null);
                    }
                }
            };
            timer.Start();
        }

        public void SetTemplate(MpTemplateHyperlinkViewModel ttcvm, bool isEditMode) {
            //cases
            //1. a new template is being created (null,true)
            //2. an existing template is being referenced (tvm,false)
            //3. an existing template is being edited (tvm, true)
            _originalText = _originalTemplateName = string.Empty;
            _originalTemplateColor = null;
            _selectedTemplateHyperlink = null;
            _originalSelection = ClipTileViewModel.RichTextBoxViewModelCollection.SubSelectedRtb.Selection;
            SelectedTemplateHyperlinkViewModel = null;
            if (ttcvm == null) {
                //for new template create the vm but wait to add it in OkCommand
                //SelectedTemplateHyperlinkViewModel = new MpTemplateHyperlinkViewModel(ClipTileViewModel, null);
                _originalText = ClipTileViewModel.RichTextBoxViewModelCollection.SubSelectedRtb.Selection.Text;
                //_selectedTemplateHyperlink = MpHelpers.Instance.CreateTemplateHyperlink(SelectedTemplateHyperlinkViewModel, ClipTileViewModel.GetRtb().Selection);
                
                _selectedTemplateHyperlink = MpTemplateHyperlinkViewModel.CreateTemplateHyperlink(ClipTileViewModel, null, ClipTileViewModel.RichTextBoxViewModelCollection.SubSelectedRtb.Selection);
                SelectedTemplateHyperlinkViewModel = (MpTemplateHyperlinkViewModel)_selectedTemplateHyperlink.DataContext;
                ClipTileViewModel.RichTextBoxViewModelCollection.SubSelectedRtb.Selection.Select(_selectedTemplateHyperlink.ElementStart, _selectedTemplateHyperlink.ElementEnd);

            } else {
                _originalTemplateName = ttcvm.TemplateName;
                _originalTemplateColor = ttcvm.TemplateBrush;
                if(isEditMode) {
                    SelectedTemplateHyperlinkViewModel = ttcvm;
                } else {
                    //SelectedTemplateHyperlinkViewModel = new MpTemplateHyperlinkViewModel(ClipTileViewModel, ttcvm.CopyItemTemplate);
                    //_selectedTemplateHyperlink = MpHelpers.Instance.CreateTemplateHyperlink(SelectedTemplateHyperlinkViewModel, ClipTileViewModel.GetRtb().Selection);
                    _selectedTemplateHyperlink = MpTemplateHyperlinkViewModel.CreateTemplateHyperlink(ClipTileViewModel, ttcvm.CopyItemTemplate, ClipTileViewModel.RichTextBoxViewModelCollection.SubSelectedRtb.Selection);
                    SelectedTemplateHyperlinkViewModel = (MpTemplateHyperlinkViewModel)_selectedTemplateHyperlink.DataContext;
                    ClipTileViewModel.RichTextBoxViewModelCollection.SubSelectedRtb.Selection.Select(_selectedTemplateHyperlink.ElementStart, _selectedTemplateHyperlink.ElementEnd);
                }
                
            }
            ClipTileViewModel.IsEditingTemplate = isEditMode;

            if (!ClipTileViewModel.IsEditingTemplate) {
                //MpHelpers.Instance.CreateTemplateHyperlink(SelectedTemplateHyperlinkViewModel, ClipTileViewModel.GetRtb().Selection);
                MpTemplateHyperlinkViewModel.CreateTemplateHyperlink(ClipTileViewModel, SelectedTemplateHyperlinkViewModel.CopyItemTemplate, ClipTileViewModel.RichTextBoxViewModelCollection.SubSelectedRtb.Selection);
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
            //var dupTokenHyperlink = ClipTileViewModel.RichTextBoxViewModels.SubSelectedRtbvm.TemplateHyperlinkCollectionViewModel.Where(x => x.TemplateName == SelectedTemplateHyperlinkViewModel.TemplateName && x.CopyItemTemplateId != SelectedTemplateHyperlinkViewModel.CopyItemTemplateId).ToList();
            MpTemplateHyperlinkViewModel dthlvm = null;
            foreach(var thlvm in ClipTileViewModel.RichTextBoxViewModelCollection.SubSelectedRtbvm.TemplateHyperlinkCollectionViewModel.UniqueTemplateHyperlinkViewModelListByDocOrder) {
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
            var templateNameRanges = MpHelpers.Instance.FindStringRangesFromPosition(ClipTileViewModel.RichTextBoxViewModelCollection.SubSelectedRtb.Document.ContentStart, SelectedTemplateHyperlinkViewModel.TemplateName, true);
            foreach (var tnr in templateNameRanges) {
                //check if templatename exists in the document
                var thlr = (Hyperlink)MpHelpers.Instance.FindParentOfType(tnr.Start.Parent, typeof(Hyperlink));
                if(thlr == null) {
                    continue;
                }
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
            ClipTileViewModel.RichTextBoxViewModelCollection.SubSelectedRtb.Focus();
            if (IsSelectedNewTemplate) {
                var selectionStart = SelectedTemplateHyperlinkViewModel.TemplateHyperlinkRange.Start;
                SelectedTemplateHyperlinkViewModel.Dispose(false);
                _originalSelection.Text = _originalText;
                var sr = MpHelpers.Instance.FindStringRangeFromPosition(selectionStart, _originalText, true);
                ClipTileViewModel.RichTextBoxViewModelCollection.SubSelectedRtb.Selection.Select(sr.Start, sr.End);                
            } else {
                foreach (var thlvm in ClipTileViewModel.RichTextBoxViewModelCollection.SubSelectedRtbvm.TemplateHyperlinkCollectionViewModel) {
                    if (thlvm.TemplateName == SelectedTemplateHyperlinkViewModel.TemplateName) {
                        //restore original name/color to datacontext
                        thlvm.TemplateName = _originalTemplateName;
                        thlvm.TemplateBrush = _originalTemplateColor;
                    }
                }
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
            if (SelectedTemplateHyperlinkViewModel == null) {
                return;
            }
            if(IsSelectedNewTemplate) {
                ClipTileViewModel.RichTextBoxViewModelCollection.SubSelectedRtbvm.TemplateHyperlinkCollectionViewModel.Add(SelectedTemplateHyperlinkViewModel);
            } else {
                foreach(var thlvm in ClipTileViewModel.RichTextBoxViewModelCollection.SubSelectedRtbvm.TemplateHyperlinkCollectionViewModel) {
                    if(thlvm.TemplateName == SelectedTemplateHyperlinkViewModel.TemplateName) {
                        thlvm.CopyItemTemplate.WriteToDatabase();
                    }
                }
            }

            SelectedTemplateHyperlinkViewModel.IsSelected = true;
            ClipTileViewModel.IsEditingTemplate = false;

            ClipTileViewModel.RichTextBoxViewModelCollection.SubSelectedRtb.Focus();
        }
        #endregion

    }
}
 