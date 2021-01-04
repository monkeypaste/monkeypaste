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
using Windows.UI.Xaml.Controls;

namespace MpWpfApp {
    public class MpTemplateTokenEditModalWindowViewModel : MpViewModelBase {
        #region Static Variables
        public static bool IsOpen = false;
        #endregion

        #region Private Variables
        private bool _isEditMode = false;
        private Window _windowRef = null;
        //private RichTextBox _rtb = null;
        //private Hyperlink _originalLink = null;
        private string _originalTemplateName = string.Empty;
        private Brush _originalTemplateColor = Brushes.Pink;
        public TextPointer _selectionEnd = null;
        #endregion

        #region Properties
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

        //private Hyperlink _selectedTokenHyperlink = null;
        //public Hyperlink SelectedTokenHyperlink {
        //    get {
        //        return _selectedTokenHyperlink;
        //    }
        //    set {
        //        if (_selectedTokenHyperlink != value) {
        //            _selectedTokenHyperlink = value;
        //            OnPropertyChanged(nameof(SelectedTokenHyperlink));
        //            OnPropertyChanged(nameof(SelectedTokenName));
        //            OnPropertyChanged(nameof(SelectedTokenBrush));
        //            OnPropertyChanged(nameof(SelectedTemplateTokenViewModel));
        //        }
        //    }
        //}

        //public string SelectedTokenName {
        //    get {
        //        return SelectedTokenHyperlink.TargetName;
        //    }
        //    set {
        //        if(string.IsNullOrEmpty(value)) {
        //            return;
        //        }
        //        SelectedTokenHyperlink.TargetName = value;
        //        Validate();
        //        OnPropertyChanged(nameof(SelectedTokenName));
        //        OnPropertyChanged(nameof(SelectedTokenBrush));
        //        OnPropertyChanged(nameof(SelectedTokenHyperlink));

        //        foreach(var thlvm in ((MpClipTileViewModel)_rtb.DataContext).TemplateTokens) {
        //            if(thlvm.TemplateName == _originalLink.TargetName) {
        //                thlvm.TemplateName = SelectedTokenName;
        //            }
        //        }
        //    }
        //}

        //public Brush SelectedTokenBrush {
        //    get {
        //        return SelectedTokenHyperlink.Background;
        //    }
        //    set {
        //        SelectedTokenHyperlink.Background = value;
        //        OnPropertyChanged(nameof(SelectedTokenBrush));
        //        OnPropertyChanged(nameof(SelectedTokenHyperlink));

        //        foreach (var thlvm in ((MpClipTileViewModel)_rtb.DataContext).TemplateTokens) {
        //            if (thlvm.TemplateName == _originalLink.TargetName) {
        //                thlvm.TemplateName = SelectedTokenName;
        //            }
        //        }
        //    }
        //}

        private MpTemplateTokenCollectionViewModel _templateTokenCollectionViewModel = null;
        public MpTemplateTokenCollectionViewModel TemplateTokenCollectionViewModel {
            get {
                return _templateTokenCollectionViewModel;
            }
            set {
                if (_templateTokenCollectionViewModel != value) {
                    _templateTokenCollectionViewModel = value;
                    OnPropertyChanged(nameof(TemplateTokenCollectionViewModel));
                }
            }
        }

        #endregion

        #region Static Methods
        public static bool ShowTemplateTokenEditModalWindow(
            MpPasteTemplateToolbarViewModel pttbvm,
            MpTemplateTokenCollectionViewModel ttcvm,
            bool isEditMode) {
            var ttmw = new MpTemplateTokenEditModalWindow(pttbvm, ttcvm, isEditMode);

            if(isEditMode) {
                ((MpMainWindowViewModel)Application.Current.MainWindow.DataContext).IsShowingDialog = true;
                var result = ttmw.ShowDialog();
                ((MpMainWindowViewModel)Application.Current.MainWindow.DataContext).IsShowingDialog = false;
                if (result == null || result.Value == true) {
                    return true;
                } else {
                    return false;
                }
            } else {
                return true;
            }
        }
        #endregion

        #region Public Methods
        public MpTemplateTokenEditModalWindowViewModel(
            MpPasteTemplateToolbarViewModel pttbvm,
            MpTemplateTokenCollectionViewModel ttcvm) : base() {

           if(ttcvm == null) {
                TemplateTokenCollectionViewModel = new MpTemplateTokenCollectionViewModel(
                    pttbvm, 
                    new MpCopyItemTemplate(
                        pttbvm.ClipTileViewModel.CopyItemId, 
                        GetUniqueTemplateColor(), 
                        GetUniqueTemplateName()));
                //templateLink = new Hyperlink().ConvertToTemplateHyperlink(
                //    _rtb, 
                //    GetUniqueTemplateName(), 
                //    GetUniqueTemplateColor());                
            } else {
                _originalTemplateName = ttcvm.TemplateName;
                _originalTemplateColor = ttcvm.TemplateBrush;
                TemplateTokenCollectionViewModel = ttcvm;
            }

            //SelectedTemplateTokenViewModel = (MpTemplateHyperlinkViewModel)templateLink.DataContext;
            if (!_isEditMode) {
                OkCommand.Execute(null);
                return;
            }          
        }

        public void TemplateTokenModalWindow_Loaded(object sender, RoutedEventArgs e) {
            _windowRef = (Window)sender;
            IsOpen = true;
        }
        #endregion

        #region Private Methods
        private string GetUniqueTemplateName() {
            int uniqueIdx = 1;
            string namePrefix = "Template #";
            while(TemplateTokenCollectionViewModel.PasteTemplateToolbarViewModel.Where(x => x.TemplateName == namePrefix + uniqueIdx && x != TemplateTokenCollectionViewModel).ToList().Count > 0) {
                uniqueIdx++;
            }
            return namePrefix + uniqueIdx;
        }

        private Brush GetUniqueTemplateColor() {
            Brush randColor = (Brush)new SolidColorBrush(MpHelpers.GetRandomColor());
            while (TemplateTokenCollectionViewModel.PasteTemplateToolbarViewModel.Where(x => x.TemplateBrush == randColor).ToList().Count > 0) {
                randColor = (Brush)new SolidColorBrush(MpHelpers.GetRandomColor());
            }
            return randColor;
        }

        private bool Validate() {
            //if (string.IsNullOrEmpty(SelectedTokenName)) {
            //    ValidationText = "Name cannot be empty!";
            //    return false;
            //}
            ////if new name is a duplicate of another just delete this one and set it to the duplicate
            //var dupTokenHyperlink = _rtb.GetTemplateHyperlinkList().Where(x => x.TargetName == SelectedTokenName && x != SelectedTokenHyperlink).ToList();
            //if (dupTokenHyperlink != null && dupTokenHyperlink.Count > 0) {
            //    ValidationText = SelectedTokenName + " already exists!";
            //    return false;
            //}
            if (string.IsNullOrEmpty(TemplateTokenCollectionViewModel.TemplateName)) {
                ValidationText = "Name cannot be empty!";
                return false;
            }
            //if new name is a duplicate of another just delete this one and set it to the duplicate
            var dupTokenHyperlink = TemplateTokenCollectionViewModel.PasteTemplateToolbarViewModel.Where(x => x.TemplateName == TemplateTokenCollectionViewModel.TemplateName && !string.IsNullOrEmpty(_originalTemplateName) && x != TemplateTokenCollectionViewModel).ToList();
            if (dupTokenHyperlink != null && dupTokenHyperlink.Count > 0) {
                ValidationText = TemplateTokenCollectionViewModel.TemplateName + " already exists!";
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
            //var result = MpHelpers.ShowColorDialog(SelectedTokenBrush);
            //if(result != null) {
            //    SelectedTokenBrush = result;
            //}
            var result = MpHelpers.ShowColorDialog(TemplateTokenCollectionViewModel.TemplateBrush);
            if (result != null) {
                TemplateTokenCollectionViewModel.TemplateBrush = result;
            }
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
            if(!string.IsNullOrEmpty(_originalTemplateName)) {
                var rtb = TemplateTokenCollectionViewModel.PasteTemplateToolbarViewModel.ClipTileViewModel.GetRtb();
                rtb.Selection.Text = String.Format(
                @"{0}{1}{0}{2}{0}",
                Properties.Settings.Default.TemplateTokenMarker,
                _originalTemplateName,
                ((SolidColorBrush)_originalTemplateColor).ToString());
                rtb.ClearHyperlinks();
                var ctvm = (MpClipTileViewModel)rtb.DataContext;
                ctvm.CopyItemRichText = MpHelpers.ConvertFlowDocumentToRichText(rtb.Document);
                rtb.CreateHyperlinks();
            }

            _windowRef.DialogResult = false;
            _windowRef.Close();
            IsOpen = false;
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
            if(string.IsNullOrEmpty(_originalTemplateName)) {
                var rtb = TemplateTokenCollectionViewModel.PasteTemplateToolbarViewModel.ClipTileViewModel.GetRtb();
                TemplateTokenCollectionViewModel.CopyItemTemplate.WriteToDatabase();

                var nthlvm = new MpTemplateHyperlinkViewModel(
                    TemplateTokenCollectionViewModel,
                    new MpTemplateTextRange(
                        TemplateTokenCollectionViewModel.CopyItemTemplateId,
                        rtb.Document.ContentStart.GetOffsetToPosition(rtb.Selection.Start),
                        rtb.Document.ContentStart.GetOffsetToPosition(rtb.Selection.End)));  
                
                TemplateTokenCollectionViewModel.Add(nthlvm);
            }
            //_rtb.Selection.Text = String.Format(
            //    @"{0}{1}{0}{2}{0}",
            //    Properties.Settings.Default.TemplateTokenMarker,
            //    SelectedTemplateTokenViewModel.TemplateName,
            //    ((SolidColorBrush)SelectedTemplateTokenViewModel.TemplateBackgroundBrush).ToString());
            //_rtb.ClearHyperlinks();
            //var ctvm = (MpClipTileViewModel)_rtb.DataContext;
            //ctvm.CopyItemRichText = MpHelpers.ConvertFlowDocumentToRichText(_rtb.Document);

            if (_windowRef != null) {
                _windowRef.DialogResult = true;
                _windowRef.Close();
            }
            IsOpen = false;
        }
        #endregion

    }
}
 