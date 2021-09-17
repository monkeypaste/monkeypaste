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
using MonkeyPaste;

namespace MpWpfApp {
    public class MpEditTemplateToolbarViewModel : MpUndoableViewModelBase<MpEditTemplateToolbarViewModel>, IDisposable {
        #region Private Variables
        private Hyperlink _selectedTemplateHyperlink = null;
        private string _originalText = string.Empty;
        private string _originalTemplateName = string.Empty;
        private TextRange _originalSelection = null;
        private Brush _originalTemplateColor = Brushes.Pink;

        //private Grid _borderGrid = null;

        
        #endregion

        #region Properties       

        #region View Models        

        public MpClipTileViewModel HostClipTileViewModel {
            get {
                if (RtbItemCollectionViewModel == null) {
                    return null;
                }

                return RtbItemCollectionViewModel.HostClipTileViewModel;
            }
        }

        private MpRtbItemCollectionViewModel _rtbItemCollectionViewModel;
        public MpRtbItemCollectionViewModel RtbItemCollectionViewModel {
            get {
                return _rtbItemCollectionViewModel;
            }
            private set {
                if (_rtbItemCollectionViewModel != value) {
                    _rtbItemCollectionViewModel = value;
                    OnPropertyChanged(nameof(RtbItemCollectionViewModel));
                    OnPropertyChanged(nameof(HostClipTileViewModel));
                }
            }
        }

        public MpRtbItemViewModel SubSelectedRtbViewModel {
            get {
                if (HostClipTileViewModel == null) {
                    return null;
                }
                if (HostClipTileViewModel.ContentContainerViewModel.Count == 0 ||
                   HostClipTileViewModel.ContentContainerViewModel.SubSelectedContentItems.Count != 1) {
                    return null;
                }
                return HostClipTileViewModel.ContentContainerViewModel.SubSelectedContentItems[0] as MpRtbItemViewModel;
            }
        }

        private MpTemplateHyperlinkViewModel _selectedTemplateHyperlinkViewModel = null;
        public MpTemplateHyperlinkViewModel SelectedTemplateHyperlinkViewModel {
            get {
                return _selectedTemplateHyperlinkViewModel;
            }
            set {
                if (_selectedTemplateHyperlinkViewModel != value) {
                    _selectedTemplateHyperlinkViewModel = value;
                    OnPropertyChanged(nameof(SelectedTemplateHyperlinkViewModel));
                }
            }
        }
        #endregion

        #region Controls
        // public TextBox SelectedTemplateNameTextBox {get; set;}
        #endregion

        #region Layout 
        public double EditTemplateToolbarHeight {
            get {
                return MpMeasurements.Instance.ClipTileEditTemplateToolbarHeight;
            }
        }

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

        //public double SelectedTemplateNameTextBoxBorderBrushThickness {
        //    get {
        //        if (string.IsNullOrEmpty(ValidationText)) {
        //            return 1;
        //        }
        //        return 3;
        //    }
        //}
        #endregion
         
        #region Visibility Properties        

        private Visibility _editTemplateToolbarVisibility = Visibility.Collapsed;
        public Visibility EditTemplateToolbarVisibility {
            get {
                return _editTemplateToolbarVisibility;
            }
            set {
                if (_editTemplateToolbarVisibility != value) {
                    _editTemplateToolbarVisibility = value;
                    OnPropertyChanged(nameof(EditTemplateToolbarVisibility));
                }
            }
        }
        #endregion

        #region Brush Properties
        public Brush SelectedTemplateNameTextBoxBorderBrush {
            get {
                //if (string.IsNullOrEmpty(ValidationText)) {
                //    return Brushes.Black;
                //}
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

        public bool WasEdited { get; set; } = false;
        #endregion


        #region Model Properties
        public MpCopyItemTemplate CopyItemTemplate { get; set; }
        #endregion

        #endregion

        #region Public Methods
        public MpEditTemplateToolbarViewModel() : base() {
            PropertyChanged += MpEditTemplateToolbarViewModel_PropertyChanged;
        }


        public MpEditTemplateToolbarViewModel(MpRtbItemCollectionViewModel rtbicvm) : this() {
            RtbItemCollectionViewModel = rtbicvm;
        }

        private void MpEditTemplateToolbarViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(CopyItemTemplate):

                    break;
            }
        }

        //public void SetTemplate(MpTemplateHyperlinkViewModel ttcvm, bool isEditMode) {
        //    //cases
        //    //1. a new template is being created (null,true)
        //    //2. an existing template is being referenced (tvm,false)
        //    //3. an existing template is being edited (tvm, true)
        //    _originalText = _originalTemplateName = string.Empty;
        //    _originalTemplateColor = null;
        //    _selectedTemplateHyperlink = null;
        //    _originalSelection = SubSelectedRtbViewModel.Rtb.Selection;
        //    SelectedTemplateHyperlinkViewModel = null;
        //    if (ttcvm == null) {
        //        //for new template create the vm but wait to add it in OkCommand
        //        //SelectedTemplateHyperlinkViewModel = new MpTemplateHyperlinkViewModel(ClipTileViewModel, null);
        //        _originalText = SubSelectedRtbViewModel.Rtb.Selection.Text;
        //        //_selectedTemplateHyperlink = MpHelpers.Instance.CreateTemplateHyperlink(SelectedTemplateHyperlinkViewModel, ClipTileViewModel.GetRtb().Selection);
                
        //        _selectedTemplateHyperlink = MpTemplateHyperlinkViewModel.CreateTemplateHyperlink(SubSelectedRtbViewModel, null, SubSelectedRtbViewModel.Rtb.Selection);
        //        SelectedTemplateHyperlinkViewModel = (MpTemplateHyperlinkViewModel)_selectedTemplateHyperlink.DataContext;
        //        SubSelectedRtbViewModel.Rtb.Selection.Select(_selectedTemplateHyperlink.ElementStart, _selectedTemplateHyperlink.ElementEnd);

        //    } else {
        //        _originalTemplateName = ttcvm.TemplateName;
        //        _originalTemplateColor = ttcvm.TemplateBrush;
        //        if(isEditMode) {
        //            SelectedTemplateHyperlinkViewModel = ttcvm;
        //        } else {
        //            //SelectedTemplateHyperlinkViewModel = new MpTemplateHyperlinkViewModel(ClipTileViewModel, ttcvm.CopyItemTemplate);
        //            //_selectedTemplateHyperlink = MpHelpers.Instance.CreateTemplateHyperlink(SelectedTemplateHyperlinkViewModel, ClipTileViewModel.GetRtb().Selection);
        //            _selectedTemplateHyperlink = MpTemplateHyperlinkViewModel.CreateTemplateHyperlink(SubSelectedRtbViewModel, ttcvm.CopyItemTemplate, SubSelectedRtbViewModel.Rtb.Selection);
        //            SelectedTemplateHyperlinkViewModel = (MpTemplateHyperlinkViewModel)_selectedTemplateHyperlink.DataContext;
        //            SubSelectedRtbViewModel.Rtb.Selection.Select(_selectedTemplateHyperlink.ElementStart, _selectedTemplateHyperlink.ElementEnd);
        //        }
                
        //    }
        //    HostClipTileViewModel.IsEditingTemplate = isEditMode;

        //    if (!HostClipTileViewModel.IsEditingTemplate) {
        //        //MpHelpers.Instance.CreateTemplateHyperlink(SelectedTemplateHyperlinkViewModel, ClipTileViewModel.GetRtb().Selection);
        //        MpTemplateHyperlinkViewModel.CreateTemplateHyperlink(SubSelectedRtbViewModel, SelectedTemplateHyperlinkViewModel.CopyItemTemplate, SubSelectedRtbViewModel.Rtb.Selection);
        //        OkCommand.Execute(null);
        //    } else {
        //        //MonkeyPaste.MpConsole.WriteLine("SetTemplate Resize edit template toolbar deltaHeight: " + HostClipTileViewModel.EditTemplateToolbarHeight);
        //        ShowToolbar();
        //    }
        //}

        //public void SetTemplateName(string newName) {
        //    if (SelectedTemplateHyperlinkViewModel != null) {
        //        foreach (var thlvm in SubSelectedRtbViewModel.TemplateHyperlinkCollectionViewModel) {
        //            if (thlvm.CopyItemTemplateId == SelectedTemplateHyperlinkViewModel.CopyItemTemplateId) {
        //                thlvm.TemplateName = newName;
        //            }
        //        }
        //        Validate();
        //    }
        //}

        //public void ResetState() {
        //    SelectedTemplateHyperlinkViewModel = null;
        //    ValidationText = string.Empty;
        //    _selectedTemplateHyperlink = null;
        //    _originalText = string.Empty;
        //    _originalTemplateName = string.Empty;
        //    _originalSelection = null;
        //    _originalTemplateColor = Brushes.Pink;

        //    if (!Validate()) {
        //        CancelCommand.Execute(null);
        //    } else {
        //        OkCommand.Execute(null);
        //    }
        //}
        #endregion

        #region Private Methods


        //private bool Validate() {
        //    if (SelectedTemplateHyperlinkViewModel == null || SubSelectedRtbViewModel == null) {
        //        return true;
        //    }
        //    if (string.IsNullOrEmpty(SelectedTemplateHyperlinkViewModel.TemplateName)) {
        //        ValidationText = "Name cannot be empty!";
        //        return false;
        //    }
        //    //if new name is a duplicate of another just delete this one and set it to the duplicate
        //    //var dupTokenHyperlink = ClipTileViewModel.RichTextBoxViewModels.SubSelectedRtbvm.TemplateHyperlinkCollectionViewModel.Where(x => x.TemplateName == SelectedTemplateHyperlinkViewModel.TemplateName && x.CopyItemTemplateId != SelectedTemplateHyperlinkViewModel.CopyItemTemplateId).ToList();
        //    MpTemplateHyperlinkViewModel dthlvm = null;
        //    foreach(var thlvm in SubSelectedRtbViewModel.TemplateHyperlinkCollectionViewModel.UniqueTemplateHyperlinkViewModelListByDocOrder) {
        //        //check if another template has this name
        //        if(thlvm != SelectedTemplateHyperlinkViewModel &&
        //           thlvm.CopyItemTemplateId != SelectedTemplateHyperlinkViewModel.CopyItemTemplateId &&
        //           thlvm.TemplateName == SelectedTemplateHyperlinkViewModel.TemplateName) {
        //            dthlvm = thlvm;
        //        }
        //    }
        //    if (dthlvm != null) {
        //        ValidationText = SelectedTemplateHyperlinkViewModel.TemplateDisplayName + " already exists";
        //        return false;
        //    }
        //    var templateNameRanges = MpHelpers.Instance.FindStringRangesFromPosition(SubSelectedRtbViewModel.Rtb.Document.ContentStart, SelectedTemplateHyperlinkViewModel.TemplateName, true);
        //    foreach (var tnr in templateNameRanges) {
        //        //check if templatename exists in the document
        //        var thlr = (Hyperlink)MpHelpers.Instance.FindParentOfType(tnr.Start.Parent, typeof(Hyperlink));
        //        if(thlr == null) {
        //            continue;
        //        }
        //        if (SelectedTemplateHyperlinkViewModel.TemplateHyperlinkRange != null &&

        //           (thlr == null || !thlr.ElementStart.IsInSameDocument(SelectedTemplateHyperlinkViewModel.TemplateHyperlinkRange.Start)) &&
        //          !thlr.Equals(SelectedTemplateHyperlinkViewModel.TemplateHyperlinkRange)) {
        //            //the first condition will occur when use is adding an already created template
        //            //the second condition ensures the found range is not part of a template which the earlier loop would have been detected
        //            //then the or ensures IF the first condition is non-null (a link with the template name) it won't be part of the selected thlvm
        //            ValidationText = SelectedTemplateHyperlinkViewModel.TemplateName + " already exists in text";
        //            return false;
        //        }
        //    }

        //    ValidationText = string.Empty;
        //    return true;
        //}

        private void ShowToolbar() {
            //EditTemplateBorderCanvasTop = HostClipTileViewModel.TileContentHeight - HostClipTileViewModel.EditTemplateToolbarHeight;
            EditTemplateToolbarVisibility = Visibility.Visible;
        }

        private void HideToolbar() {
            //EditTemplateBorderCanvasTop = HostClipTileViewModel.TileContentHeight + 20;
            EditTemplateToolbarVisibility = Visibility.Hidden;
        }

        #endregion

        #region Commands        
        //private RelayCommand _cancelCommand;
        //public ICommand CancelCommand {
        //    get {
        //        if (_cancelCommand == null) {
        //            _cancelCommand = new RelayCommand(Cancel);
        //        }
        //        return _cancelCommand;
        //    }
        //}
        //private void Cancel() {
        //    if(HostClipTileViewModel.IsPastingTemplate) {
        //        return;
        //    }
        //    SubSelectedRtbViewModel.Rtb.Focus();
        //    if (IsSelectedNewTemplate) {
        //        var selectionStart = SelectedTemplateHyperlinkViewModel.TemplateHyperlinkRange.Start;
        //        SelectedTemplateHyperlinkViewModel.Dispose(false);
        //        _originalSelection.Text = _originalText;
        //        var sr = MpHelpers.Instance.FindStringRangeFromPosition(selectionStart, _originalText, true);
        //        SubSelectedRtbViewModel.Rtb.Selection.Select(sr.Start, sr.End);                
        //    } else {
        //        foreach (var thlvm in SubSelectedRtbViewModel.TemplateHyperlinkCollectionViewModel) {
        //            if (thlvm.TemplateName == SelectedTemplateHyperlinkViewModel.TemplateName) {
        //                //restore original name/color to datacontext
        //                thlvm.TemplateName = _originalTemplateName;
        //                thlvm.TemplateBrush = _originalTemplateColor;
        //            }
        //        }
        //    }
        //    HostClipTileViewModel.IsEditingTemplate = false;
        //    HideToolbar();
        //}

        //private RelayCommand _okCommand;
        //public ICommand OkCommand {
        //    get {
        //        if (_okCommand == null) {
        //            _okCommand = new RelayCommand(Ok, CanOk);
        //        }
        //        return _okCommand;
        //    }
        //}
        //private bool CanOk() {
        //    if(HostClipTileViewModel == null || !HostClipTileViewModel.IsEditingTemplate) {
        //        return true;
        //    }
        //    return Validate();
        //}
        //private void Ok() {
        //    if (SelectedTemplateHyperlinkViewModel == null) {
        //        return;
        //    }
        //    if(IsSelectedNewTemplate) {
        //        SubSelectedRtbViewModel.TemplateHyperlinkCollectionViewModel.Add(SelectedTemplateHyperlinkViewModel);
        //    } else {
        //        foreach(var thlvm in SubSelectedRtbViewModel.TemplateHyperlinkCollectionViewModel) {
        //            if(thlvm.TemplateName == SelectedTemplateHyperlinkViewModel.TemplateName) {
        //                thlvm.CopyItemTemplate.WriteToDatabase();
        //            }
        //        }
        //    }

        //    SelectedTemplateHyperlinkViewModel.IsSelected = true;
        //    HostClipTileViewModel.IsEditingTemplate = false;
        //    HideToolbar();
        //    //SubSelectedRtbViewModel.Rtb.Focus();
        //}

        //private RelayCommand<object> _changeTemplateColorCommand;
        //public ICommand ChangeTemplateColorCommand {
        //    get {
        //        if(_changeTemplateColorCommand == null) {
        //            _changeTemplateColorCommand = new RelayCommand<object>(ChangeTemplateColor);
        //        }
        //        return _changeTemplateColorCommand;
        //    }
        //}
        //private void ChangeTemplateColor(object args) {
        //    var templateColorButton = args as Button;
        //    var colorMenuItem = new MenuItem();
        //    var colorContextMenu = new ContextMenu();
        //    colorContextMenu.Items.Add(colorMenuItem);
        //    MpHelpers.Instance.SetColorChooserMenuItem(
        //        colorContextMenu,
        //        colorMenuItem,
        //        (s1, e1) => {
        //            if (IsSelectedNewTemplate) {
        //                SelectedTemplateHyperlinkViewModel.TemplateBrush = (Brush)((Border)s1).Tag;
        //            } else {
        //                foreach (var thlvm in SubSelectedRtbViewModel.TemplateHyperlinkCollectionViewModel) {
        //                    if (thlvm.TemplateName == SelectedTemplateHyperlinkViewModel.TemplateName) {
        //                        thlvm.TemplateBrush = (Brush)((Border)s1).Tag;
        //                    }
        //                }
        //            }
        //        },
        //        MpHelpers.Instance.GetColorColumn(SelectedTemplateHyperlinkViewModel.TemplateBrush),
        //        MpHelpers.Instance.GetColorRow(SelectedTemplateHyperlinkViewModel.TemplateBrush)
        //    );
        //    templateColorButton.ContextMenu = colorContextMenu;
        //    colorContextMenu.PlacementTarget = templateColorButton;
        //    colorContextMenu.Width = 200;
        //    colorContextMenu.Height = 100;
        //    colorContextMenu.IsOpen = true;
        //}

        public void Dispose() {
            //SelectedTemplateNameTextBox = null;
        }
        #endregion

    }
}
 