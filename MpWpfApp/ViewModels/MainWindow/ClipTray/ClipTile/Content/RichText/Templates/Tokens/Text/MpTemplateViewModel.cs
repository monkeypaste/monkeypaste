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
    public class MpTemplateViewModel : MpViewModelBase<MpTemplateCollectionViewModel>, ICloneable {
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

        #endregion

        #region Layout Properties        
        #endregion

        #region Appearance Properties
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
                return string.IsNullOrEmpty(ValidationText);
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
                if (MpWpfColorHelpers.IsBright(((SolidColorBrush)TemplateBackgroundBrush).Color)) {
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
                        return MpWpfColorHelpers.GetDarkerBrush(TemplateBrush);
                    }
                    if (IsSelected) {
                        return MpWpfColorHelpers.GetLighterBrush(TemplateBrush);
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
                    if(_isSelected && value) {
                        WasVisited = true;
                    }
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                    OnPropertyChanged(nameof(TemplateForegroundBrush));
                    OnPropertyChanged(nameof(TemplateBorderBrush));
                    OnPropertyChanged(nameof(TemplateBackgroundBrush));
                }
               
            }
        }

        public bool HasText => !string.IsNullOrEmpty(TemplateText);

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

        private bool _isPastingTemplate = false;
        public bool IsPastingTemplate {
            get {
                return _isPastingTemplate;
            }
            set {
                if (_isPastingTemplate != value) {
                    _isPastingTemplate = value;
                    OnPropertyChanged(nameof(IsPastingTemplate));
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

        public int InstanceCount { get; set; }
        #endregion

        #region Business Logic Properties

        public string TemplateDisplayValue {
            get {
                if(Parent == null) {
                    return string.Empty;
                }
                if(Parent.Parent.IsPastingTemplate &&
                    HasText) {
                    return TemplateText;
                }
                return CopyItemTemplate.TemplateToken;
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

        public bool WasNewOnEdit { get; set; } = false;

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
                }

                OnPropertyChanged(nameof(TemplateName));
                OnPropertyChanged(nameof(TemplateDisplayValue));
                OnPropertyChanged(nameof(CopyItemTemplate));
            }
        }

        public Brush TemplateBrush {
            get {
                if (CopyItemTemplate == null || string.IsNullOrEmpty(CopyItemTemplate.HexColor)) {
                    return Brushes.Pink;
                }
                return new SolidColorBrush(CopyItemTemplate.HexColor.ToWinMediaColor());
            }
            set {
                if (CopyItemTemplate != null) {
                    CopyItemTemplate.HexColor = (value as SolidColorBrush).Color.ToHex();
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
                    OnPropertyChanged(nameof(TemplateDisplayValue));
                    OnPropertyChanged(nameof(CopyItemTemplateId));
                    OnPropertyChanged(nameof(CopyItemId));
                }
            }
        }
        #endregion

        #endregion

        #region Events

        public event EventHandler OnTemplateSelected;

        #endregion

        #region Constructors

        public MpTemplateViewModel() : base(null) { }

        public MpTemplateViewModel(MpTemplateCollectionViewModel thlcvm, MpCopyItemTemplate cit) : base(thlcvm) {
             PropertyChanged += MpTemplateViewModel_PropertyChanged;
            CopyItemTemplate = cit;
        }

        #endregion

        #region Public Methods

        public bool Validate() {
            if (Parent.Templates.Any(x => x.TemplateName.ToLower() == TemplateName.ToLower() && x != this)) {
                ValidationText = $"'{Parent.Parent.CopyItemTitle}' already contains a '{TemplateName}' template";
                MpConsole.WriteLine($"Template invalidated: {ValidationText}");
                return false;
            }

            if (string.IsNullOrWhiteSpace(TemplateName)) {
                ValidationText = "Name cannot be empty!";
                MpConsole.WriteLine($"Template invalidated: {ValidationText}");
                return false;
            }

            ValidationText = string.Empty;
            return true;
        }

        public void Reset() {
            IsSelected = false;            
            TemplateText = string.Empty;
            IsEditingTemplate = false;
            WasVisited = false;
        }

        public override void Dispose() {
            PropertyChanged -= MpTemplateViewModel_PropertyChanged;
            Reset();
            base.Dispose();
        }

        #endregion

        #region Private Methods

        private void MpTemplateViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    if (IsSelected) {
                        OnTemplateSelected?.Invoke(this, null);
                    } else {
                        IsEditingTemplate = false;
                        Parent.Parent.Parent.OnPropertyChanged(nameof(Parent.Parent.Parent.IsDetailGridVisibile));
                        Parent.OnPropertyChanged(nameof(Parent.IsAnyEditingTemplate));
                    }
                    Parent.OnPropertyChanged(nameof(Parent.SelectedTemplateIdx));
                    break;
                case nameof(TemplateName):
                    Validate();
                    break;
                case nameof(ValidationText):
                    if (!string.IsNullOrEmpty(ValidationText)) {
                        MpConsole.WriteLine("Validation text changed to: " + ValidationText);
                    }
                    break;
                case nameof(IsEditingTemplate):
                    Parent.Parent.Parent.OnPropertyChanged(nameof(Parent.Parent.Parent.IsDetailGridVisibile));
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyEditingTemplate));
                    break;
            }
            Parent.UpdateCommandsCanExecute();
        }

        #endregion

        #region Commands

        public ICommand EditTemplateCommand {
            get {
                return new RelayCommand(
                    () => {
                        _originalModel = CopyItemTemplate.Clone() as MpCopyItemTemplate;

                        //Parent.ClearAllEditing();
                        //Parent.ClearSelection();

                        IsSelected = true;
                        IsEditingTemplate = true;
                        Parent.OnPropertyChanged(nameof(Parent.IsAnyEditingTemplate));
                    },
                    () => {
                        if (HostClipTileViewModel == null) {
                            return false;
                        }
                        return HostClipTileViewModel.IsExpanded;
                    });
            }
        }

        public ICommand DeleteTemplateCommand => new RelayCommand(
            async() => {
                await CopyItemTemplate.DeleteFromDatabaseAsync();
            });

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

        public ICommand CancelCommand => new RelayCommand(
            async() => {
                IsSelected = false;
                if (WasNewOnEdit) {
                    await Parent.RemoveItem(CopyItemTemplate, false);
                }
                CopyItemTemplate = _originalModel;
                IsEditingTemplate = false;

            });

        public ICommand OkCommand => new RelayCommand(
            async () => {
                await CopyItemTemplate.WriteToDatabaseAsync();
                //Parent.Parent.RequestSyncModels();
                WasNewOnEdit = false;
                IsEditingTemplate = false;
                IsSelected = false;
            },
            Validate());

        public ICommand ChangeTemplateColorCommand => new RelayCommand<object>(
        (args) => {
            var templateColorButton = args as Button;
            var colorMenuItem = new MenuItem();
            var colorContextMenu = new ContextMenu();
            colorContextMenu.Items.Add(colorMenuItem);
            MpHelpers.SetColorChooserMenuItem(
                colorContextMenu,
                colorMenuItem,
                (s1, e1) => {
                    TemplateBrush = (Brush)((Border)s1).Tag;
                }
            );
            templateColorButton.ContextMenu = colorContextMenu;
            colorContextMenu.PlacementTarget = templateColorButton;
            //colorContextMenu.Width = 200;
            //colorContextMenu.Height = 100;
            colorContextMenu.IsOpen = true;
        });
        #endregion

        #region Overrides
        public override string ToString() {
            return string.Format(
                @"Name:{0} Text:{1} Count:{2} IsEditing:{3} IsSelected:{4} WasNew:{5} IsNew:{6}",
                TemplateName,TemplateText,InstanceCount,IsEditingTemplate?"T":"F",IsSelected?"T":"F",WasNewOnEdit ? "T" : "F",IsNew ? "T" : "F");
        }

        public object Clone() {
            var nthlvm = new MpTemplateViewModel(Parent, CopyItemTemplate);
            nthlvm.TemplateText = TemplateText;
            return nthlvm;
        }
        #endregion
    }
}
