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
                if (_selectedTemplateHyperlinkViewModel != value) {
                    _selectedTemplateHyperlinkViewModel = value;
                    OnPropertyChanged(nameof(SelectedTemplateHyperlinkViewModel));
                }
            }
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
                        SetTemplate(ClipTileViewModel.TemplateHyperlinkCollectionViewModel.SelectedTemplateHyperlinkViewModel,true);
                        break;
                }
            };
        }

        public void EditTemplateToolbarBorder_Loaded(object sender, RoutedEventArgs args) {
            var editTemplateToolbarBorderGrid = (Grid)sender;
            var editTemplateToolbarBorder = editTemplateToolbarBorderGrid.GetVisualAncestor<Border>();
            var templateColorButton = (Button)editTemplateToolbarBorder.FindName("TemplateColorButton");
            var rtb = ClipTileViewModel.GetRtb();
            var cb = (MpClipBorder)editTemplateToolbarBorder.GetVisualAncestor<MpClipBorder>();
            var rtbc = (Canvas)cb.FindName("ClipTileRichTextBoxCanvas");
            var titleIconImageButton = (Button)cb.FindName("ClipTileAppIconImageButton");
            var titleSwirl = (Image)cb.FindName("TitleSwirl");
            var tb = (TextBox)editTemplateToolbarBorder.FindName("TemplateNameEditorTextBox");

            templateColorButton.Click += (s, e) => {
                var colorMenuItem = new MenuItem();
                var colorContextMenu = new ContextMenu();
                colorContextMenu.Items.Add(colorMenuItem);
                MpHelpers.SetColorChooserMenuItem(
                    colorContextMenu,
                    colorMenuItem,
                    (s1, e1) => {
                        SelectedTemplateHyperlinkViewModel.TemplateBrush = ((Border)s1).Background;
                    },
                    MpHelpers.GetColorColumn(SelectedTemplateHyperlinkViewModel.TemplateBrush),
                    MpHelpers.GetColorRow(SelectedTemplateHyperlinkViewModel.TemplateBrush)
                );
                templateColorButton.ContextMenu = colorContextMenu;
                colorContextMenu.PlacementTarget = templateColorButton;
                colorContextMenu.IsOpen = true;
            };

            ClipTileViewModel.PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(ClipTileViewModel.IsEditingTemplate):
                        double rtbBottomMax = ClipTileViewModel.TileContentHeight;
                        double rtbBottomMin = ClipTileViewModel.TileContentHeight - ClipTileViewModel.EditTemplateToolbarHeight;

                        double editTemplateToolbarTopMax = ClipTileViewModel.TileContentHeight;
                        double editTemplateToolbarTopMin = ClipTileViewModel.TileContentHeight - ClipTileViewModel.EditTemplateToolbarHeight;
                        
                        if (ClipTileViewModel.IsEditingTemplate) {
                            ClipTileViewModel.EditTemplateToolbarVisibility = Visibility.Visible;
                        }

                        MpHelpers.AnimateDoubleProperty(
                            ClipTileViewModel.IsEditingTemplate ? rtbBottomMax : rtbBottomMin,
                            ClipTileViewModel.IsEditingTemplate ? rtbBottomMin : rtbBottomMax,
                            Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
                            rtb,
                            Canvas.BottomProperty,
                            (s1, e44) => {

                            });

                        MpHelpers.AnimateDoubleProperty(
                            ClipTileViewModel.IsEditingTemplate ? editTemplateToolbarTopMax : editTemplateToolbarTopMin,
                            ClipTileViewModel.IsEditingTemplate ? editTemplateToolbarTopMin : editTemplateToolbarTopMax,
                            Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
                            editTemplateToolbarBorder,
                            Canvas.TopProperty,
                            (s1, e44) => {
                                if (!ClipTileViewModel.IsEditingTemplate) {
                                    ClipTileViewModel.EditTemplateToolbarVisibility = Visibility.Collapsed;
                                } else {
                                    tb.Focus();
                                    if(tb.Text.Length > 2) {
                                        tb.Select(1, tb.Text.Length - 2);
                                    }
                                }
                            });
                        break;
                }
            };

            rtb.PreviewMouseLeftButtonDown += (s1, e1) => {
                if(ClipTileViewModel.IsEditingTemplate) {
                    OkCommand.Execute(null);
                }
            };
        }


        public void SetTemplate(MpTemplateHyperlinkViewModel ttcvm, bool isEditMode) {
            if (ttcvm == null) {
                //for new template create the vm but wait to add it in OkCommand
                SelectedTemplateHyperlinkViewModel = new MpTemplateHyperlinkViewModel(ClipTileViewModel,null);
                _originalText = ClipTileViewModel.GetRtb().Selection.Text;
                _selectedTemplateHyperlink = MpHelpers.CreateTemplateHyperlink(SelectedTemplateHyperlinkViewModel, ClipTileViewModel.GetRtb().Selection);
            } else {
                _originalTemplateName = ttcvm.TemplateName;
                _originalTemplateColor = ttcvm.TemplateBrush;
                SelectedTemplateHyperlinkViewModel = ttcvm;
            }
            ClipTileViewModel.IsEditingTemplate = isEditMode;

            if (!ClipTileViewModel.IsEditingTemplate) {
                MpHelpers.CreateTemplateHyperlink(SelectedTemplateHyperlinkViewModel, ClipTileViewModel.GetRtb().Selection);
                OkCommand.Execute(null);
            }
        }
        #endregion

        #region Private Methods
        private bool Validate() {
            if (SelectedTemplateHyperlinkViewModel == null) {
                return true;
            }
            if (string.IsNullOrEmpty(SelectedTemplateHyperlinkViewModel.TemplateName)) {
                ValidationText = "Name cannot be empty!";
                return false;
            }
            //if new name is a duplicate of another just delete this one and set it to the duplicate
            var dupTokenHyperlink = ClipTileViewModel.TemplateHyperlinkCollectionViewModel.Where(x => x.TemplateName == SelectedTemplateHyperlinkViewModel.TemplateName && x.CopyItemTemplateId != SelectedTemplateHyperlinkViewModel.CopyItemTemplateId).ToList();
            if (dupTokenHyperlink != null && dupTokenHyperlink.Count > 0) {
                ValidationText = SelectedTemplateHyperlinkViewModel.TemplateName + " already exists!";
                return false;
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
            if(IsSelectedNewTemplate) {
                ClipTileViewModel.GetRtb().Selection.Text = _originalText;
            } else {
                //restore original name/color to datacontext
                SelectedTemplateHyperlinkViewModel.TemplateName = _originalTemplateName;
                SelectedTemplateHyperlinkViewModel.TemplateBrush = _originalTemplateColor;
            }
            SelectedTemplateHyperlinkViewModel.IsSelected = false;
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
            SelectedTemplateHyperlinkViewModel.CopyItemTemplate.WriteToDatabase();

           // if(!ClipTileViewModel.TemplateHyperlinkCollectionViewModel.Contains(SelectedTemplateHyperlinkViewModel)) 
                {
                ClipTileViewModel.TemplateHyperlinkCollectionViewModel.Add(SelectedTemplateHyperlinkViewModel);
            }
            SelectedTemplateHyperlinkViewModel.IsSelected = false;
            ClipTileViewModel.IsEditingTemplate = false;
        }
        #endregion

    }
}
 