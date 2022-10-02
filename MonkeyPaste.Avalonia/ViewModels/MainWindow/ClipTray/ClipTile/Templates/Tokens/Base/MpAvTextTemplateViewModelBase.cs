using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MonkeyPaste;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;

using System.Diagnostics;

namespace MonkeyPaste.Avalonia {
    public enum MpCurrencyType {
        None = 0,
        Dollars,
        Pounds,
        Euros,
        Yen
    }    

    public abstract class MpAvTextTemplateViewModelBase : MpViewModelBase<MpAvTemplateCollectionViewModel>, 
        MpISelectableViewModel,
        MpIValidatableViewModel,
        MpIHoverableViewModel,
        MpIMenuItemViewModel,
        MpIUserColorViewModel {

        #region Private Variables
        private MpTextTemplate _originalModel;
        #endregion

        #region Properties

        #region View Models


        public MpAvClipTileViewModel HostClipTileViewModel {
            get {
                if(Parent == null) {
                    return null;
                }
                return Parent.Parent;
            }
        }

        public MpMenuItemViewModel ContextMenuItemViewModel {
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

        public string TemplateNameTextBoxBorderBrush {
            get {
                return IsValid ? MpSystemColors.Transparent : MpSystemColors.Red;
            }
        }
        #endregion

        #region Validation
        public string ValidationText { get; set; }

        public bool IsValid => string.IsNullOrEmpty(ValidationText);
        #endregion

        #region Brush Properties


        public string TemplateForegroundHexColor {
            get {
                if (MpColorHelpers.IsBright(TemplateBackgroundHexColor)) {
                    return MpSystemColors.black;
                }
                return MpSystemColors.White;
            }
        }

        public string TemplateBorderHexColor {
            get {
                if(HostClipTileViewModel == null ||
                   HostClipTileViewModel.IsContentReadOnly) {
                    return MpSystemColors.oldlace;
                }
                if(IsSelected) {
                    return MpSystemColors.Red;
                }
                if(IsHovering) {
                    return MpSystemColors.Yellow;
                }
                return MpSystemColors.oldlace;
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

        public bool IsEditTextBoxFocused { get; set; }

        public bool IsPasteTextBoxFocused { get; set; }

        public bool WasVisited { get; set; }

        public bool IsEnabled {
            get {
                if(Parent == null || Parent.Parent == null) {
                    return false;
                }
                if(Parent.Parent.IsPasting && 
                   Parent.PastableItems.Any(x=>x.TextTemplateId == TextTemplateId)) {
                    return true;
                }
                return false;
            }
        }


        #endregion

        #region Business Logic Properties

        public virtual string TemplateDisplayValue {
            get {
                if(Parent == null) {
                    return string.Empty;
                }
                if(Parent.Parent.IsPastingTemplate &&
                    IsEnabled &&
                    HasText) {
                    return TemplateText;
                }
                return TemplateName;
            }
        }

        public string TemplateText { get; set; }

        public bool IsInputRequiredForPaste {
            get {
                if(this is MpDynamicTextTemplateViewModel ||
                   this is MpContactTextTemplateViewModel) {
                    return true;
                }
                return false;
            }
        }

        #endregion

        #region Model Properties

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

        public MpRichTextFormatInfoFormat RichTextFormat {
            get {
                if (TextTemplate == null) {
                    return null;                    
                }

                return TextTemplate.RichTextFormat;
            }
            set {
                if (RichTextFormat != value) {
                    TextTemplate.RichTextFormat = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(RichTextFormat));
                }
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

        public string TemplateData {
            get {
                if (TextTemplate == null) {
                    return string.Empty;
                }

                return TextTemplate.TemplateData;
            }
            set {
                if (TemplateData != value) {
                    TextTemplate.TemplateData = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(TemplateData));
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
                    OnPropertyChanged(nameof(RichTextFormat));
                }
            }
        }

        public MpTextTemplateType TextTemplateType {
            get {
                if (TextTemplate == null) {
                    return MpTextTemplateType.None;
                }
                return TextTemplate.TemplateType;
            }
            set {
                if (TextTemplateType != value) {
                    TextTemplate.TemplateType = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(TextTemplateType));
                }
            }
        }


        public MpTextTemplate TextTemplate { get; set; }
        #endregion

        #endregion

        #region Events
        #endregion

        #region Constructors

        public MpAvTextTemplateViewModelBase() : base(null) { }

        public MpAvTextTemplateViewModelBase(MpAvTemplateCollectionViewModel thlcvm) : base(thlcvm) {
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

        public virtual void FillAutoTemplate() { }
        #endregion

        #region Protected Methods


        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpTextTemplate cit && TextTemplateId == cit.Id) {
                if(HasModelChanged) {
                    //change is from this template instance so ignore
                    return;
                }
                TextTemplate = cit;
            }
        }

        #endregion

        #region Private Methods

        private void MpTemplateViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    if (IsSelected) {
                        LastSelectedDateTime = DateTime.Now;

                        Parent.OnPropertyChanged(nameof(Parent.SelectedItem));

                        //if(HostClipTileViewModel.IsPasting) {
                        //    if (TextTemplateType == MpTextTemplateType.DateTime ||
                        //        TextTemplateType == MpTextTemplateType.Static) {
                        //        Parent.SelectNextTemplateCommand.Execute(null);
                        //    }
                        //}

                    } else {
                        if(IsEditingTemplate) {
                            FinishEditTemplateCommand.Execute(null);
                        }
                        //IsEditingTemplate = false;
                        Parent.Parent.OnPropertyChanged(nameof(Parent.Parent.IsDetailGridVisibile));
                        Parent.OnPropertyChanged(nameof(Parent.IsAnyEditingTemplate));

                        Parent.OnPropertyChanged(nameof(Parent.SelectedItem));
                    }
                    break;
                case nameof(TemplateName):
                    Validate();
                    break;
                case nameof(TemplateText):
                    OnPropertyChanged(nameof(TemplateDisplayValue));
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
                    Parent.Parent.OnPropertyChanged(nameof(Parent.Parent.IsDetailGridVisibile));
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyEditingTemplate));

                    break;
                case nameof(HasModelChanged):
                    if(HasModelChanged && IsValid) {
                        Task.Run(async () => {
                            await TextTemplate.WriteToDatabaseAsync();
                            HasModelChanged = false;
                        });
                    }
                    break;
            }
        }

        #endregion

        #region Commands

        public ICommand EditTemplateCommand => new MpAsyncCommand(
            async() => {
                _originalModel = await TextTemplate.CloneDbModelAsync(
                    deepClone: true,
                    suppressWrite: true);
                //Parent.ClearAllEditing();
                //Parent.ClearSelection();

                Parent.SelectedItem = this;
                IsEditingTemplate = true;
                
                Parent.OnPropertyChanged(nameof(Parent.IsAnyEditingTemplate));
                await Task.Delay(50);
                IsEditTextBoxFocused = true; 
            },
            () => {
                if (HostClipTileViewModel == null) {
                    return false;
                }
                return !HostClipTileViewModel.IsContentReadOnly;
            });

        public ICommand DeleteAllTemplateInstancesCommand => new MpAsyncCommand(
            async() => {
                IsEditingTemplate = false;
                await TextTemplate.DeleteFromDatabaseAsync();
            });

        public ICommand ClearTemplateCommand => new MpCommand(
            () => {
                TemplateText = string.Empty;
            }, () => HasText);
        //public ICommand ClearTemplateCommand {
        //    get {
        //        return new MpCommand(
        //            () => {
        //                TemplateText = string.Empty;
        //            },
        //            ()=> {
        //                return HasText;
        //            });
        //    }
        //}

        public ICommand CancelEditTemplateCommand => new MpCommand(
            () => {
                IsSelected = false;
                TextTemplate = _originalModel;
                IsEditingTemplate = false;

            });

        public ICommand FinishEditTemplateCommand => new MpAsyncCommand(
            async () => {
                await TextTemplate.WriteToDatabaseAsync();
                IsEditingTemplate = false;
                IsSelected = false;
            },
            ()=>Validate());

        #endregion

        #region Overrides
        public override string ToString() {
            return string.Format(
                @"Name:{0} Text:{1} Type:{2} Data:{3}",
                TemplateName,TemplateText,TextTemplateType,TemplateData);
        }

        #endregion
    }
}
