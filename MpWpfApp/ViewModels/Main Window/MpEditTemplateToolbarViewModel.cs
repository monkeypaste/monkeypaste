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

            templateColorButton.Click += (s, e) => {
                var colorMenuItem = new MenuItem();
                var colorContextMenu = new ContextMenu();
                colorContextMenu.Items.Add(colorMenuItem);
                MpHelpers.SetColorChooserMenuItem(
                    colorContextMenu,
                    colorMenuItem,
                    (s1, e1) => {
                        SelectedTemplateHyperlinkViewModel.TemplateBrush = ((Button)s).Background;
                    },
                    MpHelpers.GetColorColumn(SelectedTemplateHyperlinkViewModel.TemplateBrush),
                    MpHelpers.GetColorRow(SelectedTemplateHyperlinkViewModel.TemplateBrush)
                );
                templateColorButton.ContextMenu = colorContextMenu;
                colorContextMenu.PlacementTarget = templateColorButton;
                colorContextMenu.IsOpen = true;
            };

            editTemplateToolbarBorder.IsVisibleChanged += (s, e1) => {
                double fromTopToolbar = 0;
                double toTopToolbar = 0;

                double fromBottomRtb = 0;
                double toBottomRtb = 0;

                if (editTemplateToolbarBorder.Visibility == Visibility.Visible) {
                    fromTopToolbar = ClipTileViewModel.TileContentHeight;
                    toTopToolbar = fromTopToolbar - ClipTileViewModel.EditTemplateToolbarHeight;

                    fromBottomRtb = ClipTileViewModel.TileContentHeight;
                    toBottomRtb = fromBottomRtb - ClipTileViewModel.EditTemplateToolbarHeight;
                } else {
                    fromTopToolbar = ClipTileViewModel.TileContentHeight - ClipTileViewModel.EditTemplateToolbarHeight;
                    toTopToolbar = ClipTileViewModel.TileContentHeight;

                    fromBottomRtb = ClipTileViewModel.TileContentHeight - ClipTileViewModel.EditTemplateToolbarHeight;
                    toBottomRtb = ClipTileViewModel.TileContentHeight;
                }

                MpHelpers.AnimateDoubleProperty(
                    fromBottomRtb,
                    toBottomRtb,
                    Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
                    rtb,
                    Canvas.BottomProperty,
                    (s1, e) => {

                    });

                MpHelpers.AnimateDoubleProperty(
                    fromTopToolbar,
                    toTopToolbar,
                    Properties.Settings.Default.ShowMainWindowAnimationMilliseconds,
                    editTemplateToolbarBorder,
                    Canvas.TopProperty,
                    (s1, e) => {

                    });

                
            };
        }


        public void SetTemplate(MpTemplateHyperlinkViewModel ttcvm, bool isEditMode) {
            if (ttcvm == null) {
                //for new template create the vm but wait to add it in OkCommand
                SelectedTemplateHyperlinkViewModel = new MpTemplateHyperlinkViewModel(ClipTileViewModel,null);
            } else {
                _originalTemplateName = ttcvm.TemplateName;
                _originalTemplateColor = ttcvm.TemplateBrush;
                SelectedTemplateHyperlinkViewModel = ttcvm;
            }
            ClipTileViewModel.IsEditingTemplate = isEditMode;

            if (!ClipTileViewModel.IsEditingTemplate) {
                OkCommand.Execute(null);
                return;
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
            var dupTokenHyperlink = ClipTileViewModel.TemplateHyperlinkCollectionViewModel.Where(x => x.TemplateName == SelectedTemplateHyperlinkViewModel.TemplateName/* && !string.IsNullOrEmpty(_originalTemplateName) */&& x != SelectedTemplateHyperlinkViewModel).ToList();
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
            if(!IsSelectedNewTemplate) {
                if(!string.IsNullOrEmpty(_originalTemplateName)) {
                    //restore original name/color to datacontext
                    SelectedTemplateHyperlinkViewModel.TemplateName = _originalTemplateName;
                    SelectedTemplateHyperlinkViewModel.TemplateBrush = _originalTemplateColor;
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
            SelectedTemplateHyperlinkViewModel.CopyItemTemplate.WriteToDatabase();

            //if(IsSelectedNewTemplate) 
                {
                ClipTileViewModel.TemplateHyperlinkCollectionViewModel.Add(SelectedTemplateHyperlinkViewModel, ClipTileViewModel.GetRtb().Selection);
            }
            ClipTileViewModel.IsEditingTemplate = false;
        }
        #endregion

    }
}
 