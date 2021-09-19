using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MonkeyPaste;
using System.Collections.ObjectModel;

namespace MpWpfApp {
    public enum MpCurrencyType {
        None = 0,
        Dollars,
        Pounds,
        Euros,
        Yen
    }    
    public class MpTokenViewModel : MpViewModelBase<MpTokenCollectionViewModel>, ICloneable {
        #region Private Variables
        private MpCopyItemTemplate _originalModel;
        #endregion

        #region Properties

        #region View Models
        public MpClipTileViewModel HostClipTileViewModel {
            get {
                if(Parent == null) {
                    return null;
                }
                return Parent.HostClipTileViewModel;
            }
        }
        private MpTokenCollectionViewModel _hostTemplateCollectionViewModel = null;
        public MpTokenCollectionViewModel Parent {
            get {
                return _hostTemplateCollectionViewModel;
            }
            set {
                if (_hostTemplateCollectionViewModel != value) {
                    _hostTemplateCollectionViewModel = value;
                    OnPropertyChanged(nameof(Parent));
                    OnPropertyChanged(nameof(HostClipTileViewModel));
                    OnPropertyChanged(nameof(TemplateDisplayValue));
                    OnPropertyChanged(nameof(TemplateTextBlockCursor));
                    OnPropertyChanged(nameof(TemplateName));
                    OnPropertyChanged(nameof(TemplateDisplayName));
                }
            }
        }
        #endregion

        #region Layout Properties        
        #endregion

        #region Appearance Properties
        public Cursor TemplateTextBlockCursor {
            get {
                if(Parent != null && 
                  (HostClipTileViewModel.IsEditingContent || HostClipTileViewModel.IsEditingTemplate)) {
                    return Cursors.Hand;
                }
                return Cursors.Arrow;
            }
        }
        #endregion

        #region Visibility Properties
        #endregion

        #region Validation
        private string _validationText = string.Empty;
        public string ValidationText {
            get {
                return _validationText;
            }
            set {
                if (_validationText != value) {
                    _validationText = value;
                    OnPropertyChanged(nameof(ValidationText));
                    OnPropertyChanged(nameof(TemplateNameTextBoxBorderBrush));
                }
            }
        }

        public bool IsValid {
            get {
                return !string.IsNullOrEmpty(ValidationText);
            }
        }
        #endregion

        #region Brush Properties
        public Brush TemplateNameTextBoxBorderBrush {
            get {
                return IsValid ? Brushes.Transparent : Brushes.Red;
            }
        }

        

        public Brush TemplateBorderBrush {
            get {
                if(HostClipTileViewModel == null || !HostClipTileViewModel.IsExpanded) {
                    return Brushes.Transparent;
                }
                if(IsSelected) {
                    return Brushes.Red;
                }
                if(IsHovering) {
                    return Brushes.Yellow;
                }
                return Brushes.Transparent;
            }
        }

        public Brush TemplateForegroundBrush {
            get {
                if (MpHelpers.Instance.IsBright(((SolidColorBrush)TemplateBackgroundBrush).Color)) {
                    return Brushes.Black;
                }
                return Brushes.White;
            }
        }

        public Brush TemplateBackgroundBrush {
            get {
                if(HostClipTileViewModel == null) {
                    return TemplateBrush;
                }
                if(HostClipTileViewModel.IsExpanded) {
                    if (IsHovering) {
                        return MpHelpers.Instance.GetDarkerBrush(TemplateBrush);
                    }
                    if (IsSelected) {
                        return MpHelpers.Instance.GetLighterBrush(TemplateBrush);
                    }
                }
                return TemplateBrush;
            }
        }
        #endregion

        #region State Properties
        private bool _isHovering = false;
        public bool IsHovering {
            get {
                return _isHovering;
            }
            set {
                if (_isHovering != value) {
                    _isHovering = value;
                    OnPropertyChanged(nameof(IsHovering));
                    OnPropertyChanged(nameof(TemplateForegroundBrush));
                    OnPropertyChanged(nameof(TemplateBorderBrush));
                    OnPropertyChanged(nameof(TemplateBackgroundBrush));
                    OnPropertyChanged(nameof(TemplateTextBlockCursor));
                }
            }
        }

        private bool _isSelected = false;
        public bool IsSelected {
            get {
                return _isSelected;
            }
            set {
                //if (_isSelected != value)  
                    {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                    OnPropertyChanged(nameof(TemplateForegroundBrush));
                    OnPropertyChanged(nameof(TemplateBorderBrush));
                    OnPropertyChanged(nameof(TemplateBackgroundBrush));
                    OnPropertyChanged(nameof(TemplateTextBlockCursor));
                }
               
            }
        }

        public bool HasText {
            get {
                return !string.IsNullOrEmpty(TemplateText);
            }
        }

        private bool _isEditingTemplate = false;
        public bool IsEditingTemplate {
            get {
                return _isEditingTemplate;
            }
            set {
                if (_isEditingTemplate != value) {
                    _isEditingTemplate = value;
                    OnPropertyChanged(nameof(IsEditingTemplate));
                }
            }
        }

        private bool _wasVisited = false;
        public bool WasVisited {
            get {
                return _wasVisited;
            }
            set {
                if (_wasVisited != value) {
                    _wasVisited = value;
                    OnPropertyChanged(nameof(WasVisited));
                }

            }
        }

        private int _instanceCount = 0;
        public int InstanceCount {
            get {
                return _instanceCount;            
            }
            set {
                if(_instanceCount != value) {
                    _instanceCount = value;
                    OnPropertyChanged(nameof(InstanceCount));
                }
            }
        }
        #endregion

        #region Business Logic Properties
        public int TemplateTokenTag {
            get {
                return (int)MpSubTextTokenType.TemplateSegment;
            }
        }

        public string TemplateDisplayValue {
            get {
                if (HostClipTileViewModel.IsPastingTemplate && 
                    HasText) {
                    return TemplateText;
                }
                return TemplateName;
            }
        }

        public string TemplateDisplayName {
            get {
                if(string.IsNullOrEmpty(TemplateName)) {
                    return string.Empty;
                }
                return TemplateName.Replace("<", String.Empty).Replace(">", string.Empty);
            }
        }

        private string _templateText = string.Empty;
        public string TemplateText {
            get {
                return _templateText;
            }
            set {
                if (_templateText != value) {
                    _templateText = value;
                    OnPropertyChanged(nameof(TemplateText));
                    OnPropertyChanged(nameof(TemplateDisplayValue));
                }
            }
        }
        #endregion

        #region Model Properties
        public bool IsNew {
            get {
                if(CopyItemTemplate == null) {
                    return false;
                }
                return CopyItemTemplate.Id == 0;
            }
        }

        public int CopyItemTemplateId {
            get {
                if (CopyItemTemplate == null) {
                    return 0;
                }
                return CopyItemTemplate.Id;
            }
        }

        public int CopyItemId {
            get {
                if(CopyItemTemplate == null) {
                    return 0;
                }
                return CopyItemTemplate.CopyItemId;
            }
        }

        public string TemplateName {
            get {
                if (CopyItemTemplate == null) {
                    return "TEMPLATE UNKNOWN";
                }
                return CopyItemTemplate.TemplateName;
            }
            set {
                if (CopyItemTemplate == null) {
                    return;
                }
                if (CopyItemTemplate.TemplateName != value) {
                    CopyItemTemplate.TemplateName = value;
                    if(CopyItemTemplate.TemplateName == null) {
                        CopyItemTemplate.TemplateName = string.Empty;
                    }
                    if(!CopyItemTemplate.TemplateName.StartsWith("<")) {
                        CopyItemTemplate.TemplateName = "<" + CopyItemTemplate.TemplateName;
                    }
                    if(!CopyItemTemplate.TemplateName.EndsWith(">")) {
                        CopyItemTemplate.TemplateName = CopyItemTemplate.TemplateName + ">";
                    }
                }

                OnPropertyChanged(nameof(TemplateName));
                OnPropertyChanged(nameof(TemplateDisplayValue));
                OnPropertyChanged(nameof(TemplateDisplayName));
                OnPropertyChanged(nameof(CopyItemTemplate));
            }
        }

        public Brush TemplateBrush {
            get {
                if (CopyItemTemplate == null) {
                    return Brushes.Pink;
                }
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(CopyItemTemplate.HexColor));
            }
            set {
                if (CopyItemTemplate != null) {
                    CopyItemTemplate.HexColor = value.ToString();                    
                    OnPropertyChanged(nameof(TemplateBrush));
                    OnPropertyChanged(nameof(TemplateForegroundBrush));
                    OnPropertyChanged(nameof(TemplateBackgroundBrush));
                    OnPropertyChanged(nameof(CopyItemTemplate));
                }
            }
        }

        private MpCopyItemTemplate _copyItemTemplate = null;
        public MpCopyItemTemplate CopyItemTemplate {
            get {
                return _copyItemTemplate;
            }
            set {
                if (_copyItemTemplate != value) {
                    _copyItemTemplate = value;
                    OnPropertyChanged(nameof(CopyItemTemplate));
                    OnPropertyChanged(nameof(TemplateBrush));
                    OnPropertyChanged(nameof(TemplateName)); 
                    OnPropertyChanged(nameof(TemplateDisplayName));
                    OnPropertyChanged(nameof(TemplateDisplayValue));
                    OnPropertyChanged(nameof(CopyItemTemplateId));
                    OnPropertyChanged(nameof(CopyItemId));
                }
            }
        }
        #endregion

        #endregion

        public event EventHandler OnTemplateSelected;

        #region Public Methods
        public MpTokenViewModel() : base(null) { }
        public MpTokenViewModel(MpTokenCollectionViewModel thlcvm, MpCopyItemTemplate cit) : base(thlcvm) {
            PropertyChanged += (s, e) => {
                switch(e.PropertyName) {
                    case nameof(IsSelected):
                        if(IsSelected) {
                             OnTemplateSelected?.Invoke(this, null);
                        }
                        break;
                    case nameof(TemplateName):
                        Validate();
                        break;
                }
            };
            CopyItemTemplate = cit;         
        }

        public bool Validate() {
            Parent.Parent.SaveToDatabase();

            if (string.IsNullOrEmpty(TemplateName.Trim())) {
                ValidationText = "Name cannot be empty!";
                return false;
            }

            string pt = Parent.Parent.CopyItem.ItemData.ToPlainText();
            if (pt.Contains(TemplateName)) {
                ValidationText = $"{TemplateName} must have a unique name";
                return false;
            }

            ValidationText = string.Empty;
            return true;
        }

        public void Reset() {
            TemplateText = string.Empty;
            IsEditingTemplate = false;
        }

        #endregion

        #region Commands
        public ICommand EditTemplateCommand {
            get {
                return new RelayCommand(
                    () => {
                        _originalModel = CopyItemTemplate.Clone() as MpCopyItemTemplate;

                        Parent.ClearAllEditing();
                        Parent.ClearSelection();

                        IsSelected = true;
                        IsEditingTemplate = true;

                        HostClipTileViewModel.IsEditingTemplate = true;
                    },
                    () => {
                        if (HostClipTileViewModel == null) {
                            return false;
                        }
                        return HostClipTileViewModel.IsExpanded;
                    });
            }
        }

        public ICommand DeleteTemplateCommand {
            get {
                return new RelayCommand(
                    () => {
                        CopyItemTemplate.DeleteFromDatabase();
                    });
            }
        }

        public ICommand ClearTemplateCommand {
            get {
                return new RelayCommand(
                    () => {
                        TemplateText = string.Empty;
                    },
                    ()=> {
                        return HasText;
                    });
            }
        }

        public ICommand CancelCommand {
            get {
                return new RelayCommand(
                    () => {
                        CopyItemTemplate = _originalModel;
                        IsEditingTemplate = false;
                        IsSelected = false;
                    });
            }
        }

        public ICommand OkCommand {
            get {
                return new RelayCommand(
                    () => {
                        CopyItemTemplate.WriteToDatabase();
                        IsEditingTemplate = false;
                        IsSelected = false;
                    },
                    () => {
                        return Validate();
                    });
            }
        }

        public ICommand ChangeTemplateColorCommand {
            get {
                return new RelayCommand<object>(
                    (args) => {
                        var templateColorButton = args as Button;
                        var colorMenuItem = new MenuItem();
                        var colorContextMenu = new ContextMenu();
                        colorContextMenu.Items.Add(colorMenuItem);
                        MpHelpers.Instance.SetColorChooserMenuItem(
                            colorContextMenu,
                            colorMenuItem,
                            (s1, e1) => {
                                TemplateBrush = (Brush)((Border)s1).Tag;
                            },
                            MpHelpers.Instance.GetColorColumn(TemplateBrush),
                            MpHelpers.Instance.GetColorRow(TemplateBrush)
                        );
                        templateColorButton.ContextMenu = colorContextMenu;
                        colorContextMenu.PlacementTarget = templateColorButton;
                        colorContextMenu.Width = 200;
                        colorContextMenu.Height = 100;
                        colorContextMenu.IsOpen = true;
                    });
            }
        }
        #endregion

        #region Overrides
        public override string ToString() {
            return TemplateName;
        }

        //public void Dispose(bool fromContextMenu) {
        //    if(fromContextMenu) {
        //        HostTemplateCollectionViewModel.Rtb.Selection.Select(TemplateHyperlinkRange.Start, TemplateHyperlinkRange.End);
        //        HostTemplateCollectionViewModel.Rtb.Selection.Text = string.Empty;
        //    }
        //    //remove this individual token reference
        //    if(HostTemplateCollectionViewModel != null) {
        //        HostTemplateCollectionViewModel.TemplateHyperlinkCollectionViewModel.Templates.Remove(this);
        //    }
            
        //}

        public object Clone() {
            var nthlvm = new MpTokenViewModel(Parent, CopyItemTemplate);
            nthlvm.TemplateText = TemplateText;
            return nthlvm;
        }
        #endregion
    }
}
