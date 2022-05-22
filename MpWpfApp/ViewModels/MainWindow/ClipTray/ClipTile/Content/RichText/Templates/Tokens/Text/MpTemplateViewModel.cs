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
        MpIMenuItemViewModel,
        MpIUserColorViewModel {
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

        #region MpIUserColorViewModel 

        public string UserHexColor {
            get => TemplateHexColor;
            set => TemplateHexColor = value;
        }
        #endregion

        #region Layout Properties        
        #endregion

        #region Appearance Properties
        #endregion

        #region Visibility Properties
        #endregion

        #region Validation
        public string ValidationText { get; set; }

        public bool IsValid => string.IsNullOrEmpty(ValidationText);
        #endregion

        #region Brush Properties
        public Brush TemplateNameTextBoxBorderBrush {
            get {
                return IsValid ? Brushes.Transparent : Brushes.Red;
            }
        }        


        public Brush TemplateForegroundHexColor {
            get {
                if (MpColorHelpers.IsBright(TemplateBackgroundHexColor)) {
                    return Brushes.Black;
                }
                return Brushes.White;
            }
        }

        public string TemplateBackgroundHexColor {
            get {
                if(HostClipTileViewModel == null) {
                    return string.Empty;
                }
                if(!HostClipTileViewModel.IsContentReadOnly) {
                    if (IsHovering) {
                        return MpColorHelpers.GetDarkerHexColor(TemplateHexColor);
                    }
                    if (IsSelected) {
                        return MpColorHelpers.GetLighterHexColor(TemplateHexColor);
                    }
                }
                return TemplateHexColor;
            }
        }
        #endregion

        #region State Properties
        public bool IsHovering { get; set; }

        public bool IsSelected { get; set; }
        public DateTime LastSelectedDateTime { get; set; }

        public bool HasText => !string.IsNullOrEmpty(TemplateText);

        public bool IsEditingTemplate { get; set; }

        public bool IsPastingTemplate { get; set; }

        public bool WasVisited { get; set; }

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
                return TemplateName;
            }
        }

        public string TemplateText { get; set; }

        #endregion

        #region Model Properties
        public bool IsNew {
            get {
                if(TextTemplate == null) {
                    return false;
                }
                return TextTemplate.Id == 0;
            }
        }

        public bool WasNewOnEdit { get; set; } = false;

        public int TextTemplateId {
            get {
                if (TextTemplate == null) {
                    return 0;
                }
                return TextTemplate.Id;
            }
        }

        public string TextTemplateGuid {
            get {
                if (TextTemplate == null) {
                    return string.Empty;
                }
                return TextTemplate.Guid;
            }
        }

        public int CopyItemId {
            get {
                if(TextTemplate == null) {
                    return 0;
                }
                return 0;// TextToken.CopyItemId;
            }
        }

        public string TemplateName {
            get {
                if (TextTemplate == null) {
                    return string.Empty;
                }
                
                return TextTemplate.TemplateName;
            }
            set {
                if(TemplateName != value) {
                    TextTemplate.TemplateName = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(TemplateName));
                    OnPropertyChanged(nameof(TemplateDisplayValue));
                    OnPropertyChanged(nameof(TextTemplate));
                }
            }
        }

        public string TemplateHexColor {
            get {
                if(TextTemplate == null) {
                    return string.Empty;
                }
                return TextTemplate.HexColor;
            }
            set {
                if(TemplateHexColor != value) {
                    TextTemplate.HexColor = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(TemplateHexColor));
                }
            }
        }


        public MpTextTemplate TextTemplate { get; set; }
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
            TextTemplate = cit;

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
                        LastSelectedDateTime = DateTime.Now;
                        OnTemplateSelected?.Invoke(this, null);
                    } else {
                        if(IsEditingTemplate) {
                            OkCommand.Execute(null);
                        }
                        //IsEditingTemplate = false;
                        Parent.Parent.Parent.OnPropertyChanged(nameof(Parent.Parent.Parent.IsDetailGridVisibile));
                        Parent.OnPropertyChanged(nameof(Parent.IsAnyEditingTemplate));
                    }
                    Parent.OnPropertyChanged(nameof(Parent.SelectedItem));
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
                    if(IsEditingTemplate && !IsSelected) {
                        IsSelected = true;
                    }
                    Parent.Parent.Parent.OnPropertyChanged(nameof(Parent.Parent.Parent.IsDetailGridVisibile));
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyEditingTemplate));
                    break;
                case nameof(HasModelChanged):
                    if(HasModelChanged) {
                        Task.Run(async () => {
                            await TextTemplate.WriteToDatabaseAsync();
                            HasModelChanged = false;
                        });
                    }
                    break;
            }
            Parent.UpdateCommandsCanExecute();
        }

        #endregion

        #region Commands

        public ICommand EditTemplateCommand => new RelayCommand(
            async() => {
                _originalModel = await TextTemplate.CloneDbModel();
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
                    await Parent.RemoveItem(TextTemplate, false);
                }
                TextTemplate = _originalModel;
                IsEditingTemplate = false;

            });

        public ICommand OkCommand => new RelayCommand(
            async () => {
                await TextTemplate.WriteToDatabaseAsync();
                //Parent.Parent.RequestSyncModels();
                WasNewOnEdit = false;
                IsEditingTemplate = false;
                IsSelected = false;
            },
            Validate());

        #endregion

        #region Overrides
        public override string ToString() {
            return string.Format(
                @"Name:{0} Text:{1} Count:{2} IsEditing:{3} IsSelected:{4} WasNew:{5} IsNew:{6}",
                TemplateName,TemplateText,InstanceCount,IsEditingTemplate?"T":"F",IsSelected?"T":"F",WasNewOnEdit ? "T" : "F",IsNew ? "T" : "F");
        }

        #endregion
    }
}
