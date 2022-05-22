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
using MonkeyPaste.Plugin;
using System.Diagnostics;

namespace MpWpfApp {
    public enum MpCurrencyType {
        None = 0,
        Dollars,
        Pounds,
        Euros,
        Yen
    }    
    public class MpTemplateViewModel : 
        MpViewModelBase<MpTemplateCollectionViewModel>, 
        MpISelectableViewModel,
        MpIHoverableViewModel,
        MpIMenuItemViewModel {
        #region Private Variables
        private MpTextTemplate _originalModel;
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

        public MpMenuItemViewModel MenuItemViewModel {
            get {
                return new MpMenuItemViewModel() {
                    Header = "Delete All",
                    Command = DeleteAllTemplateInstancesCommand
                };
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
                if(HostClipTileViewModel == null || HostClipTileViewModel.IsContentReadOnly) {
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
                if(!HostClipTileViewModel.IsContentReadOnly) {
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
        public bool IsHovering { get; set; }

        public bool IsSelected { get; set; }
        public DateTime LastSelectedDateTime { get; set; }

        public bool HasText => !string.IsNullOrEmpty(MatchData);

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
                    return MatchData;
                }
                return TextToken.EncodedTemplate;
            }
        }

        private string _templateText = string.Empty;
        public string MatchData {
            get {
                return _templateText;
            }
            set {
                if (_templateText != value) {
                    _templateText = value;
                    OnPropertyChanged(nameof(MatchData));
                    OnPropertyChanged(nameof(TemplateDisplayValue));
                }
            }
        }

        #endregion

        #region Model Properties
        public bool IsNew {
            get {
                if(TextToken == null) {
                    return false;
                }
                return TextToken.Id == 0;
            }
        }

        public bool WasNewOnEdit { get; set; } = false;

        public int TextTokenId {
            get {
                if (TextToken == null) {
                    return 0;
                }
                return TextToken.Id;
            }
        }

        public string TextTokenGuid {
            get {
                if (TextToken == null) {
                    return string.Empty;
                }
                return TextToken.Guid;
            }
        }

        public int CopyItemId {
            get {
                if(TextToken == null) {
                    return 0;
                }
                return 0;// TextToken.CopyItemId;
            }
        }

        public string TemplateName {
            get {
                if (TextToken == null) {
                    return "TEMPLATE UNKNOWN";
                }
                
                return TextToken.TemplateName;
            }
            set {
                if (TextToken == null) {
                    return;
                }
                if (TextToken.TemplateName != value) {
                    TextToken.TemplateName = value;
                }

                OnPropertyChanged(nameof(TemplateName));
                OnPropertyChanged(nameof(TemplateDisplayValue));
                OnPropertyChanged(nameof(TextToken));
            }
        }

        public string TemplateHexColor {
            get {
                if(TextToken == null) {
                    return string.Empty;
                }
                return TextToken.HexColor;
            }
            set {
                if(TemplateHexColor != value) {
                    TextToken.HexColor = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(TemplateHexColor));
                }
            }
        }

        public Brush TemplateBrush {
            get {
                if (TextToken == null || string.IsNullOrEmpty(TextToken.HexColor)) {
                    return Brushes.Pink;
                }
                return new SolidColorBrush(TextToken.HexColor.ToWinMediaColor());
            }
            set {
                if (TextToken != null) {
                    TextToken.HexColor = (value as SolidColorBrush).Color.ToHex();
                    OnPropertyChanged(nameof(TemplateBrush));
                    OnPropertyChanged(nameof(TemplateForegroundBrush));
                    OnPropertyChanged(nameof(TemplateBackgroundBrush));
                    OnPropertyChanged(nameof(TextToken));
                }
            }
        }

        private MpTextTemplate _copyItemTemplate = null;
        public MpTextTemplate TextToken {
            get {
                return _copyItemTemplate;
            }
            set {
                if (_copyItemTemplate != value) {
                    _copyItemTemplate = value;
                    OnPropertyChanged(nameof(TextToken));
                    OnPropertyChanged(nameof(TemplateBrush));
                    OnPropertyChanged(nameof(TemplateName)); 
                    OnPropertyChanged(nameof(TemplateDisplayValue));
                    OnPropertyChanged(nameof(TextTokenId));
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

        public MpTemplateViewModel(MpTemplateCollectionViewModel thlcvm) : base(thlcvm) {
            PropertyChanged += MpTemplateViewModel_PropertyChanged;            
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpTextTemplate cit) {
            IsBusy = true;

            await Task.Delay(1);
            TextToken = cit;

            IsBusy = false;
        }

        public bool Validate() {
            if (Parent.Items.Any(x => x.TemplateName.ToLower() == TemplateName.ToLower() && x != this)) {
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
            MatchData = string.Empty;
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

        public ICommand EditTemplateCommand => new RelayCommand(
            async() => {
                _originalModel = await TextToken.CloneDbModel();
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
                return !HostClipTileViewModel.IsContentReadOnly;
            });

        public ICommand DeleteAllTemplateInstancesCommand => new RelayCommand(
            async() => {
                //await TextToken.DeleteFromDatabaseAsync();
                await Task.Delay(1);
                Debugger.Break();
            });

        public ICommand ClearTemplateCommand {
            get {
                return new RelayCommand(
                    () => {
                        MatchData = string.Empty;
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
                    await Parent.RemoveItem(TextToken, false);
                }
                TextToken = _originalModel;
                IsEditingTemplate = false;

            });

        public ICommand OkCommand => new RelayCommand(
            async () => {
                await TextToken.WriteToDatabaseAsync();
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
                TemplateName,MatchData,InstanceCount,IsEditingTemplate?"T":"F",IsSelected?"T":"F",WasNewOnEdit ? "T" : "F",IsNew ? "T" : "F");
        }

        #endregion
    }
}
