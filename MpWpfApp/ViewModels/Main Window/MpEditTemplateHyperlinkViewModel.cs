using GalaSoft.MvvmLight.CommandWpf;
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
    public class MpEditTemplateHyperlinkViewModel : MpViewModelBase {
        #region Static Variables
        public static bool IsOpen = false;
        #endregion

        #region Private Variables
        //private Window _windowRef = null;
        private Button _colorButtonRef = null;

        #endregion

        #region View Models
        private MpTemplateToolbarViewModel _pasteTemplateToolbarViewModel = null;
        public MpTemplateToolbarViewModel PasteTemplateToolbarViewModel {
            get {
                return _pasteTemplateToolbarViewModel;
            }
            set {
                if(_pasteTemplateToolbarViewModel != value) {
                    _pasteTemplateToolbarViewModel = value;
                    OnPropertyChanged(nameof(PasteTemplateToolbarViewModel));
                }
            }
        }
        #endregion

        #region Properties
        public string OriginalTemplateName = string.Empty;
        public Brush OriginalTemplateColor = Brushes.Pink;

        public Visibility EditTemplateToolbarVisibility {
            get {
                if (PasteTemplateToolbarViewModel != null && PasteTemplateToolbarViewModel.IsEditingTemplate) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }

        private string _windowTitle = "Template Editor";
        public string WindowTitle {
            get {
                return _windowTitle;
            }
            set {
                if (_windowTitle != value) {
                    _windowTitle = value;
                    OnPropertyChanged(nameof(WindowTitle));
                }
            }
        }

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
                    OnPropertyChanged(nameof(TemplateNameTextBoxBorderBrush));
                    OnPropertyChanged(nameof(TemplateNameTextBoxBorderBrushThickness)); ;
                }
            }
        }

        public Visibility ValidationVisibility {
            get {
                if(string.IsNullOrEmpty(ValidationText)) {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }
        }

        public double TemplateNameTextBoxBorderBrushThickness {
            get {
                if (string.IsNullOrEmpty(ValidationText)) {
                    return 1;
                }
                return 3;
            }
        }

        public Brush TemplateNameTextBoxBorderBrush {
            get {
                if (string.IsNullOrEmpty(ValidationText)) {
                    return Brushes.Black;
                }
                return Brushes.Red;
            }
        }

        private MpTemplateHyperlinkViewModel _templateHyperlinkViewModel = null;
        public MpTemplateHyperlinkViewModel TemplateHyperlinkViewModel {
            get {
                return _templateHyperlinkViewModel;
            }
            set {
                if (_templateHyperlinkViewModel != value) {
                    _templateHyperlinkViewModel = value;
                    OnPropertyChanged(nameof(TemplateHyperlinkViewModel));
                }
            }
        }

        #endregion

        #region Static Methods
        //public static bool ShowTemplateTokenEditModalWindow(
        //    MpTemplateToolbarViewModel pttbvm,
        //    MpTemplateHyperlinkViewModel ttcvm,
        //    bool isEditMode) {
        //    var ttmw = new MpTemplateTokenEditModalWindow(pttbvm,ttcvm,isEditMode);

        //    if(isEditMode) {
        //        ((MpMainWindowViewModel)Application.Current.MainWindow.DataContext).IsShowingDialog = true;
        //        var result = ttmw.ShowDialog();
        //        ((MpMainWindowViewModel)Application.Current.MainWindow.DataContext).IsShowingDialog = false;
        //        if (result == null || result.Value == true) {
        //            return true;
        //        } else {
        //            return false;
        //        }
        //    } else {
        //        return true;
        //    }
        //}
        #endregion

        #region Public Methods

        public MpEditTemplateHyperlinkViewModel(MpTemplateToolbarViewModel pttbvm) : base() {
            PasteTemplateToolbarViewModel = pttbvm;
            PasteTemplateToolbarViewModel.PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(PasteTemplateToolbarViewModel.IsEditingTemplate):
                        OnPropertyChanged(nameof(EditTemplateToolbarVisibility));
                        break;
                }
            };
        }

        public void SetTemplate(MpTemplateHyperlinkViewModel ttcvm, bool isEditMode) {
            if (ttcvm == null) {
                //for new template create the vm but wait to add it in OkCommand
                TemplateHyperlinkViewModel = new MpTemplateHyperlinkViewModel(
                    PasteTemplateToolbarViewModel,
                    new MpCopyItemTemplate(
                        PasteTemplateToolbarViewModel.ClipTileViewModel.CopyItemId,
                        GetUniqueTemplateColor(),
                        GetUniqueTemplateName()));
            } else {
                OriginalTemplateName = ttcvm.TemplateName;
                OriginalTemplateColor = ttcvm.TemplateBrush;
                TemplateHyperlinkViewModel = ttcvm;
            }

            if (!isEditMode) {
                OkCommand.Execute(null);
                return;
            }
        }

        public void EditTemplateToolbarGrid_Loaded(object sender, RoutedEventArgs e) {
            //_windowRef = (Window)sender;
            _colorButtonRef = (Button)((FrameworkElement)sender).FindName("TemplateColorButton");
            IsOpen = true;
        }
        public string GetUniqueTemplateName() {
            int uniqueIdx = 1;
            string namePrefix = "Template #";
            while (PasteTemplateToolbarViewModel.Where(x => x.TemplateName == namePrefix + uniqueIdx && x != TemplateHyperlinkViewModel).ToList().Count > 0) {
                uniqueIdx++;
            }
            return namePrefix + uniqueIdx;
        }

        public Brush GetUniqueTemplateColor() {
            Brush randColor = (Brush)new SolidColorBrush(MpHelpers.GetRandomColor());
            while (PasteTemplateToolbarViewModel.Where(x => x.TemplateBrush == randColor).ToList().Count > 0) {
                randColor = (Brush)new SolidColorBrush(MpHelpers.GetRandomColor());
            }
            return randColor;
        }
        #endregion

        #region Private Methods
        private bool Validate() {
            if (TemplateHyperlinkViewModel == null) {
                return true;
            }
            if (string.IsNullOrEmpty(TemplateHyperlinkViewModel.TemplateName)) {
                ValidationText = "Name cannot be empty!";
                return false;
            }
            //if new name is a duplicate of another just delete this one and set it to the duplicate
            var dupTokenHyperlink = TemplateHyperlinkViewModel.PasteTemplateToolbarViewModel.Where(x => x.TemplateName == TemplateHyperlinkViewModel.TemplateName && !string.IsNullOrEmpty(OriginalTemplateName) && x != TemplateHyperlinkViewModel).ToList();
            if (dupTokenHyperlink != null && dupTokenHyperlink.Count > 0) {
                ValidationText = TemplateHyperlinkViewModel.TemplateName + " already exists!";
                return false;
            }
            ValidationText = string.Empty;
            return true;
        }
        #endregion

        #region Commands
        private RelayCommand _changeTemplateColorCommand;
        public ICommand ChangeTemplateColorCommand {
            get {
                if (_changeTemplateColorCommand == null) {
                    _changeTemplateColorCommand = new RelayCommand(ChangeTemplateColor);
                }
                return _changeTemplateColorCommand;
            }
        }
        private void ChangeTemplateColor() {
            var colorMenuItem = new MenuItem();
            var colorContextMenu = new ContextMenu();
            colorContextMenu.Items.Add(colorMenuItem);
            MpHelpers.SetColorChooserMenuItem(
                colorContextMenu,
                colorMenuItem,
                (s, e1) => {
                    TemplateHyperlinkViewModel.TemplateBrush = ((Border)s).Background;
                },
                MpHelpers.GetColorColumn(TemplateHyperlinkViewModel.TemplateBrush),
                MpHelpers.GetColorRow(TemplateHyperlinkViewModel.TemplateBrush)
            );
            _colorButtonRef.ContextMenu = colorContextMenu;
            colorContextMenu.PlacementTarget = _colorButtonRef;
            colorContextMenu.IsOpen = true;
        }

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
            if(!string.IsNullOrEmpty(OriginalTemplateName)) {
                //var rtb = TemplateHyperlinkViewModel.PasteTemplateToolbarViewModel.ClipTileViewModel.GetRtb();

                if(!string.IsNullOrEmpty(OriginalTemplateName)) {
                    //restore original name/color to datacontext
                    TemplateHyperlinkViewModel.TemplateName = OriginalTemplateName;
                    TemplateHyperlinkViewModel.TemplateBrush = OriginalTemplateColor;
                } 

                //rtb.ClearHyperlinks();
                //var ctvm = (MpClipTileViewModel)rtb.DataContext;
                //ctvm.CopyItemRichText = MpHelpers.ConvertFlowDocumentToRichText(rtb.Document);
                //rtb.CreateHyperlinks();
            }
            PasteTemplateToolbarViewModel.IsEditingTemplate = false;
            //_windowRef.DialogResult = false;
            //_windowRef.Close();
            //IsOpen = false;
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
            var rtb = TemplateHyperlinkViewModel.PasteTemplateToolbarViewModel.ClipTileViewModel.GetRtb();
            TemplateHyperlinkViewModel.CopyItemTemplate.WriteToDatabase();
            TemplateHyperlinkViewModel.PasteTemplateToolbarViewModel.Add(TemplateHyperlinkViewModel, rtb.Selection);
            //rtb.Selection.Text = TemplateHyperlinkViewModel.TemplateName;
            //rtb.ClearHyperlinks();
            //var ctvm = (MpClipTileViewModel)rtb.DataContext;
            //ctvm.CopyItemRichText = MpHelpers.ConvertFlowDocumentToRichText(rtb.Document);
            //if (_windowRef != null) {
            //    _windowRef.DialogResult = true;
            //    _windowRef.Close();
            //}
            PasteTemplateToolbarViewModel.IsEditingTemplate = false;
            IsOpen = false;
        }
        #endregion

    }
}
 