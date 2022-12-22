using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Linq;
using MonkeyPaste.Common;

namespace MonkeyPaste {

    public interface MpIMenuItemViewModelBase { }

    public interface MpIMenuItemViewModel : MpIMenuItemViewModelBase {
        MpMenuItemViewModel ContextMenuItemViewModel { get; }
    }

    
    public interface MpIContextMenuViewModel : MpIMenuItemViewModelBase {
        bool IsContextMenuOpen { get; set; }
        MpMenuItemViewModel ContextMenuViewModel { get; }
    }

    public interface MpIPopupMenuViewModel : MpIMenuItemViewModelBase {
        bool IsPopupMenuOpen { get; set; }
        MpMenuItemViewModel PopupMenuViewModel { get; }
    }




    //public class MpSetColorArguments {
    //    public event EventHandler<string> SetColorEventCallback;
    //    public string OriginalColor { get; }
    //}

    public enum MpMenuItemType {
        None = 0,
        HeaderSeparator,
        Separator,
        PasteToPathRuntimeItem,
        ColorPallete,
        NewTableSelector,
        lorIconMenuItem,

    }
    public class MpMenuItemViewModel : MpViewModelBase {
        #region Statics


        #endregion

        #region Constants

        public const string DEFAULT_TEMPLATE_NAME = "DefaultMenuItemTemplate";
        public const string CHECKABLE_TEMPLATE_NAME = "CheckableMenuItemTemplate";
        public const string SEPERATOR_TEMPLATE_NAME = "SeperatorMenuItemTemplate";
        public const string COLOR_PALETTE_TEMPLATE_NAME = "ColorPaletteMenuItemTemplate";
        public const string COLOR_PALETTE_ITEM_TEMPLATE_NAME = "ColorPaletteItemMenuItemTemplate";
        public const string HEADERED_ITEM_TEMPLATE_NAME = "HeaderedSeperatorMenuItemTemplate";
        public const string PASTE_TO_PATH_ITEM_TEMPLATE_NAME = "PasteToPathRuntimeMenuItemTemplate";
        public const string NEW_TABLE_ITEM_TEMPLATE_NAME = "NewTableSelectorMenuItem";

        #endregion

        #region Properties

        #region View Models

        public IList<MpMenuItemViewModel> SubItems { get; set; }

        #endregion

        #region Data Template Helpers

        public bool IsPasteToPathRuntimeItem { get; set; }

        public bool IsSeparator { get; set; }

        public bool IsHeaderedSeparator { get; set; }

        public bool IsColorPallete { get; set; }
        public bool IsColorPalleteItem { get; set; }

        public bool IsNewTableSelector { get; set; }

        public string ContentTemplateName {
            get {
                if (IsHeaderedSeparator) {
                    return HEADERED_ITEM_TEMPLATE_NAME;
                } else if (IsSeparator) {
                    return SEPERATOR_TEMPLATE_NAME;
                } else if (IsPasteToPathRuntimeItem) {
                    return PASTE_TO_PATH_ITEM_TEMPLATE_NAME;
                } else if (IsColorPallete) {
                    return COLOR_PALETTE_TEMPLATE_NAME;
                } else if (IsColorPalleteItem) {
                    return COLOR_PALETTE_ITEM_TEMPLATE_NAME;
                } else if (IsNewTableSelector) {
                    return NEW_TABLE_ITEM_TEMPLATE_NAME;
                } else if (!string.IsNullOrEmpty(IconResourceKey)) {
                    return DEFAULT_TEMPLATE_NAME;
                } else if (!string.IsNullOrEmpty(IconHexStr)) {
                    return CHECKABLE_TEMPLATE_NAME;
                } else if (IconId > 0) {
                    return DEFAULT_TEMPLATE_NAME;
                }
                return DEFAULT_TEMPLATE_NAME;
            }
        }

        #endregion

        #region Header

        public int HeaderIndentLevel { get; set; }

        public double HeaderIndentSize { get; set; } = 20;

        public string HeaderedSeparatorLabel { get; set; }

        private string _header;
        public string Header {
            get => _header.EscapeMenuItemHeader(AltNavIdx);
            set {
                if(Header != value && Header != value.EscapeMenuItemHeader(AltNavIdx)) {
                    _header = value;
                    OnPropertyChanged(nameof(Header));
                }
            }
        }

        public int AltNavIdx { get; set; } = -1;

        #endregion

        #region State

        public bool IsEnabled {
            get {
                if(Command == null) {
                    return true;
                }
                return Command.CanExecute(CommandParameter);
            }
        }

        public bool? IsChecked { get; set; } = false;

        public string ToggleType { get; set; } = "None";

        public bool IsHovering { get; set; }

        public bool IsCustomColorButton { get; set; }

        public int SortOrderIdx { get; set; }
        #endregion

        #region Appearance

        public bool CanHide { get; set; } // for eye button on paste to path

        public bool IsVisible { get; set; } = true;

        public string CheckResourceKey { 
            get {
                if(IsChecked.IsTrue()) {
                    return "CheckSvg";
                }
                if (IsChecked.IsNull()) {
                    return "DotSvg";
                }
                return null;
            } 
        }
        #endregion

        #region Bindings
        public object IconSrcBindingObj { get; set; }
        public string IconPropPath { get; set; }
        public object HeaderSrcObj { get; set; }
        public string HeaderPropPath { get; set; }

        public object TooltipSrcObj { get; set; }
        public string TooltipPropPath { get; set; }

        public object CommandSrcObj { get; set; }
        public string CommandPath { get; set; }
        public object CommandParamSrcObj { get; set; }
        public string CommandParamPropPath { get; set; }

        public object IsCheckedSrcObj { get; set; }
        public string IsCheckedPropPath { get; set; }
        #endregion

        #region Commands


        public ICommand Command { get; set; }

        public object CommandParameter { get; set; }

        #endregion

        #region InputGesture

        public string InputGestureText {
            get {
                if(MpShortcutRef.Create(ShortcutArgs) is MpShortcutRef sr) {
                    return MpDataModelProvider.GetShortcutKeystring(sr.ShortcutType.ToString(), sr.CommandParameter);
                }
                return null;
            }
        }

        public object ShortcutArgs { get; set; }

        #endregion

        #region Icon

        private string _borderHexColor;
        public string BorderHexColor {
            get {
                if(!IsEnabled) {
                    return MpSystemColors.dimgray;
                }
                if(!string.IsNullOrEmpty(_borderHexColor)) {
                    return _borderHexColor;
                }
                //if (IsChecked.HasValue && IsChecked.Value) {
                if (IsChecked.IsTrueOrNull()) {
                    return MpSystemColors.IsSelectedBorderColor;
                } else if (IsHovering) {
                    return MpSystemColors.IsHoveringBorderColor;
                }
                return MpSystemColors.DarkGray;
            }
            set {
                if(BorderHexColor != value) {
                    _borderHexColor = value;
                    OnPropertyChanged(nameof(BorderHexColor));
                }
            }
        }

        public bool IsIconHidden => IconSourceObj == null;

        public int IconId { get; set; } = 0;

        public string IconResourceKey { get; set; } = string.Empty;

        private string _iconHexStr;
        public string IconHexStr { 
            get {
                if(IsEnabled) {
                    return _iconHexStr;
                }
                return MpSystemColors.gray;
            }
            set {
                if(_iconHexStr != value) {
                    _iconHexStr = value;
                    OnPropertyChanged(nameof(IconHexStr));
                }
            } 
        }

        public Uri IconSourceUri { get; set; }

        public object IconSourceObj {
            get {
                if(IconId > 0) {
                    return IconId;
                }
                if (!string.IsNullOrWhiteSpace(IconResourceKey)) {
                    return IconResourceKey;
                }
                if (IconHexStr.IsStringHexColor()) {
                    return IconHexStr;
                }
                if(IconSourceUri != null) {
                    return IconSourceUri;
                }
                return null;
            }
        }

        #endregion

        #region Tooltip

        public object Tooltip { get; set; }

        public bool HasTooltip => Tooltip != null;

        #endregion

        #endregion

        #region Constructors

        public MpMenuItemViewModel() : base(null) { }


        #endregion

        #region Public Methods
        public static MpMenuItemViewModel GetColorPalleteMenuItemViewModel(MpIUserColorViewModel ucvm) {
            bool isAnySelected = false;
            var colors = new List<MpMenuItemViewModel>();
            string selectedHexStr = ucvm.UserHexColor == null ? string.Empty : ucvm.UserHexColor;
            for (int i = 0; i < MpSystemColors.ContentColors.Count; i++) {
                string cc = MpSystemColors.ContentColors[i].ToUpper();
                bool isCustom = i == MpSystemColors.ContentColors.Count - 1;
                bool isSelected = selectedHexStr.ToUpper() == cc;
                if (isSelected) {
                    isAnySelected = true;
                }
                ICommand command;
                object commandArg;
                string header = cc;
                if (isCustom) {
                    if (!isAnySelected) {
                        isSelected = true;
                        // if selected color is custom make background of custom icon that color (default white)
                        header = selectedHexStr;
                    }
                    command = MpPlatformWrapper.Services.CustomColorChooserMenu.SelectCustomColorCommand;
                    commandArg = ucvm;
                } else {
                    command = SetColorCommand;
                    commandArg = new object[] { ucvm, cc };
                }

                colors.Add(new MpMenuItemViewModel() {
                    IsChecked = isSelected,
                    Header = header,
                    Command = command,
                    CommandParameter = commandArg,
                    IsVisible = isCustom,
                    IsCustomColorButton = isCustom,
                    IsColorPalleteItem = true,
                    SortOrderIdx = i
                });
            }

            return new MpMenuItemViewModel() {
                IsColorPallete = true,
                SubItems = colors.OrderBy(x => x.SortOrderIdx).ToList()
            };
        }

        public void ClearCommands() {
            Command = null;
            if(SubItems != null) {
                SubItems.ForEach(x => x.ClearCommands());
            }
        }
        #endregion

        #region Private Methods

        #endregion

        #region Commands

        public static ICommand SetColorCommand => new MpCommand<object>(
            (args) => {
                if (args == null) {
                    throw new Exception("Args must be color and color interface");
                }
                var argParts = args as object[];
                MpIUserColorViewModel ucvm = argParts[0] as MpIUserColorViewModel;
                string hexColor = argParts[1] as string;
                ucvm.UserHexColor = hexColor;
                MpPlatformWrapper.Services.ContextMenuCloser.CloseMenu();
            });

        #endregion
    }
}
