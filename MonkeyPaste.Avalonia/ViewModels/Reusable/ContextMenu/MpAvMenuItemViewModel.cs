using Avalonia.Media;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
//using Xamarin.Forms;

namespace MonkeyPaste.Avalonia {

    public interface MpIMenuItemViewModelBase { }

    public interface MpIMenuItemViewModel : MpIMenuItemViewModelBase {
        MpAvMenuItemViewModel ContextMenuItemViewModel { get; }
    }


    public interface MpIContextMenuViewModel : MpIMenuItemViewModelBase {
        bool IsContextMenuOpen { get; set; }
        MpAvMenuItemViewModel ContextMenuViewModel { get; }
    }

    public interface MpIPopupMenuViewModel : MpIMenuItemViewModelBase {
        bool IsPopupMenuOpen { get; set; }
        MpAvMenuItemViewModel PopupMenuViewModel { get; }
    }

    public interface MpIPopupMenuPicker {
        MpAvMenuItemViewModel GetMenu(ICommand cmd, object cmdArg, IEnumerable<int> selectedIds, bool recursive);
    }

    public class MpAvMenuItemViewModel : MpAvViewModelBase,
        //MpITreeItemViewModel, 
        MpAvIMenuItemViewModel {
        #region Constants

        public const string DEFAULT_TEMPLATE_NAME = "DefaultMenuItemTemplate";
        public const string CHECKABLE_TEMPLATE_NAME = "CheckableMenuItemTemplate";
        public const string SEPERATOR_TEMPLATE_NAME = "SeperatorMenuItemTemplate";
        public const string COLOR_PALETTE_TEMPLATE_NAME = "ColorPaletteMenuItemTemplate";
        public const string COLOR_PALETTE_ITEM_TEMPLATE_NAME = "ColorPaletteItemMenuItemTemplate";
        public const string HEADERED_ITEM_TEMPLATE_NAME = "HeaderedSeperatorMenuItemTemplate";
        public const string PASTE_TO_PATH_ITEM_TEMPLATE_NAME = "PasteToPathRuntimeMenuItemTemplate";
        public const string NEW_TABLE_ITEM_TEMPLATE_NAME = "NewTableSelectorMenuItem";

        public const double DEFAULT_ICON_BORDER_THICKNESS_LENGTH = 1.0d;
        public const double DEFAULT_ICON_CORNER_RADIUS_LENGTH = 2.5d;

        public const double DEFAULT_ICON_SIZE = 20.0d;
        public const double DEFAULT_ICON_WIDTH = DEFAULT_ICON_SIZE;
        public const double DEFAULT_ICON_HEIGHT = DEFAULT_ICON_SIZE;

        #endregion

        #region Statics

        #endregion

        #region Properties

        #region View Models

        public IEnumerable<MpAvIMenuItemViewModel> SubItems { get; set; }

        #endregion

        #region Data Template Helpers

        public bool IsThreeState { get; set; }
        public bool StaysOpenOnClick { get; set; }
        public bool HasLeadingSeparator { get; set; }

        private MpMenuItemType _menuItemType = MpMenuItemType.None;
        public MpMenuItemType MenuItemType {
            get {
                if (_menuItemType != MpMenuItemType.None) {
                    return _menuItemType;
                }

                switch (ContentTemplateName) {
                    case CHECKABLE_TEMPLATE_NAME:
                        return MpMenuItemType.Checkable;
                    case COLOR_PALETTE_TEMPLATE_NAME:
                        return MpMenuItemType.ColorPalette;
                    case COLOR_PALETTE_ITEM_TEMPLATE_NAME:
                        return MpMenuItemType.ColorPaletteItem;
                    default:
                        return MpMenuItemType.Default;
                }
            }
            set {
                if (_menuItemType != value) {
                    _menuItemType = value;
                    OnPropertyChanged(nameof(MenuItemType));
                }
            }
        }

        public bool IsPasteToPathRuntimeItem { get; set; }

        public bool IsSeparator { get; set; }

        public bool IsCheckable { get; set; }
        public bool IsHeaderedSeparator { get; set; }

        public bool IsColorPallete { get; set; }
        public bool IsColorPalleteItem { get; set; }

        public bool IsNewTableSelector { get; set; }

        public string ContentTemplateName {
            get {

                if (IsHeaderedSeparator) {
                    return HEADERED_ITEM_TEMPLATE_NAME;
                } else if (IsCheckable) {
                    return CHECKABLE_TEMPLATE_NAME;
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

        #region Identifier

        private object _identifier;
        public object Identifier {
            get {
                if (_identifier == null) {
                    return CommandParameter;
                }
                return _identifier;
            }
            set {
                if (Identifier != value) {
                    _identifier = value;
                    OnPropertyChanged(nameof(Identifier));
                }
            }
        }
        #endregion

        #region Header

        public int HeaderIndentLevel { get; set; }

        public double HeaderIndentSize { get; set; } = 20;

        public string HeaderedSeparatorLabel { get; set; }

        //private string _header;
        //public string Header {
        //    get => _header.EscapeMenuItemHeader(AltNavIdx);
        //    set {
        //        if (Header != value && Header != value.EscapeMenuItemHeader(AltNavIdx)) {
        //            _header = value;
        //            OnPropertyChanged(nameof(Header));
        //        }
        //    }
        //}
        public string Header { get; set; }

        public int AltNavIdx { get; set; } = -1;

        #endregion

        #region State

        public int MenuItemId { get; set; }
        public bool IsEnabled {
            get {
                if (Command == null) {
                    return true;
                }
                return Command.CanExecute(CommandParameter);
            }
        }

        public bool? NullCheckToggleResult { get; set; } = false;
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
                if (IsChecked.IsTrue()) {
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

        public object CheckedResourceSrcObj { get; set; }
        public string CheckedResourcePropPath { get; set; }

        public object IsEnabledSrcObj { get; set; }
        public string IsEnabledPropPath { get; set; }

        public object InputGestureSrcObj { get; set; }
        public string InputGesturePropPath { get; set; }
        #endregion

        #region Commands

        private ICommand _command;
        public ICommand Command {
            get {
                if (CommandSrcObj != null) {
                    return CommandParamSrcObj.GetPropertyValue(CommandPath) as ICommand;
                }
                return _command;
            }
            set {
                if (Command != value) {
                    _command = value;
                    OnPropertyChanged(nameof(Command));
                }
            }
        }

        private object _commandParameter;
        public object CommandParameter {
            get {
                if (CommandParamSrcObj != null) {
                    return CommandParamSrcObj.GetPropertyValue(CommandParamPropPath);
                }
                return _commandParameter;
            }
            set {
                if (CommandParameter != value) {
                    _commandParameter = value;
                    OnPropertyChanged(nameof(CommandParameter));
                }
            }
        }

        #endregion

        #region InputGesture

        public string InputGestureText {
            get {
                if (!Mp.Services.PlatformInfo.IsDesktop ||
                    Mp.Services.ShortcutGestureLocator == null) {
                    return null;
                }
                //if (MpShortcutRef.Create(ShortcutArgs) is MpShortcutRef sr) {
                //    return MpDataModelProvider.GetShortcutKeystring(sr.ShortcutType.ToString(), sr.CommandParameter);
                //}
                if (ShortcutArgs is object[] scArgParts &&
                    scArgParts[0] is MpShortcutType sct) {
                    if (scArgParts.Length > 1 &&
                        scArgParts[1] is MpIShortcutCommandViewModel scvm) {
                        return Mp.Services.ShortcutGestureLocator.LocateByCommand(scvm);
                    }
                    return Mp.Services.ShortcutGestureLocator.LocateByType(sct);
                }
                return null;
            }
        }

        public object ShortcutArgs { get; set; }

        #endregion

        #region Icon

        #region Model

        public MpShape IconShape { get; set; }
        public int IconId { get; set; } = 0;
        string IconBase64Str { get; set; }
        public string IconResourceKey { get; set; } = string.Empty;

        public object IconSourceObj {
            get {
                if (IconId > 0) {
                    return IconId;
                }
                if (!string.IsNullOrWhiteSpace(IconBase64Str)) {
                    return IconBase64Str;
                }
                if (!string.IsNullOrWhiteSpace(IconResourceKey)) {
                    if (IconHexStr.IsStringHexOrNamedColor()) {
                        return new object[] { IconResourceKey, IconHexStr };
                    }
                    return IconResourceKey;
                }
                if (IconHexStr.IsStringHexColor()) {
                    return IconHexStr;
                }
                return null;
            }
            set {
                if (value is int iconId) {
                    IconId = iconId;
                } else if (value is string valStr) {
                    if (valStr.IsStringHexColor()) {
                        IconHexStr = valStr;
                    } else if (valStr.IsStringImageResourcePathOrKey()) {
                        IconResourceKey = valStr;
                    } else if (valStr.IsStringBase64()) {
                        IconBase64Str = valStr;
                    } else {
                        IconId = 0;
                        IconHexStr = null;
                        IconResourceKey = null;
                    }
                } else {
                    IconId = 0;
                    IconHexStr = null;
                    IconResourceKey = null;
                }
                OnPropertyChanged(nameof(IconSourceObj));
            }
        }
        #endregion

        #region Appearance

        private string _iconHexStr;
        public string IconHexStr {
            get {
                if (IsEnabled) {
                    return _iconHexStr;
                }
                return Mp.Services.PlatformResource.GetResource<string>(MpThemeResourceKey.ThemeGrayAccent3Color.ToString());
            }
            set {
                if (_iconHexStr != value) {
                    _iconHexStr = value;
                    OnPropertyChanged(nameof(IconHexStr));
                }
            }
        }

        public string IconTintHexStr { get; set; }

        private string _iconBorderHexColor;
        public string IconBorderHexColor {
            get {
                if (!IsEnabled) {
                    return Mp.Services.PlatformResource.GetResource<string>(MpThemeResourceKey.ThemeGrayAccent1Color.ToString());
                }
                if (!string.IsNullOrEmpty(_iconBorderHexColor)) {
                    return _iconBorderHexColor;
                }
                //if (IsChecked.HasValue && IsChecked.Value) {
                if (IsChecked.IsTrueOrNull()) {
                    return Mp.Services.PlatformResource.GetResource<string>(MpThemeResourceKey.ThemeAccent2Color.ToString());
                } else if (IsHovering) {
                    return Mp.Services.PlatformResource.GetResource<string>(MpThemeResourceKey.ThemeAccent1Color.ToString());
                }
                return Mp.Services.PlatformResource.GetResource<string>(MpThemeResourceKey.ThemeGrayAccent1Color.ToString());
            }
            set {
                if (IconBorderHexColor != value) {
                    _iconBorderHexColor = value;
                    OnPropertyChanged(nameof(IconBorderHexColor));
                }
            }
        }

        public IBrush ItemBgColor { get; set; }
        public IBrush CheckedItemBgColor { get; set; }
        public IBrush UncheckedItemBgColor { get; set; }
        #endregion

        #region Layout


        public double IconCornerRadius { get; set; } = DEFAULT_ICON_CORNER_RADIUS_LENGTH;
        public double IconBorderThickness { get; set; } = DEFAULT_ICON_BORDER_THICKNESS_LENGTH;

        public double IconMinWidth { get; set; } = DEFAULT_ICON_WIDTH;
        public double IconMinHeight { get; set; } = DEFAULT_ICON_HEIGHT;

        private double[] _iconMargin;
        public double[] IconMargin {
            get {
                //if (_iconMargin == null) {
                //    return new double[] { 5, 0, IconMinWidth + 10, 0 };
                //}
                return _iconMargin;
            }
            set {
                _iconMargin = value;
            }
        }
        #endregion

        #region State

        public bool IsIconHidden => IconSourceObj == null;

        #endregion


        #endregion

        #region Tooltip

        public object Tooltip { get; set; }

        public bool HasTooltip => Tooltip != null;

        #endregion

        #endregion

        #region Constructors

        public MpAvMenuItemViewModel() : base(null) { }
        public MpAvMenuItemViewModel(object parentObj) : base(parentObj) { }


        #endregion

        #region Public Methods


        public static MpAvMenuItemViewModel GetColorPalleteMenuItemViewModel(MpIUserColorViewModel ucvm, bool has_leading_sep = false) {
            bool isAnySelected = false;
            var colors = new List<MpAvMenuItemViewModel>();
            string selectedHexStr = ucvm.UserHexColor == null ? string.Empty : ucvm.UserHexColor;
            if (selectedHexStr.Length == 7) {
                // add alpha for matching
                selectedHexStr = $"#FF{selectedHexStr.Substring(1)}";
            }
            for (int i = 0; i < MpSystemColors.ContentColors.Count; i++) {
                string cc = MpSystemColors.ContentColors[i].ToUpper();
                bool isCustom = i == MpSystemColors.ContentColors.Count - 1;
                bool isSelected = selectedHexStr.ToUpper() == cc;
                if (isSelected) {
                    isAnySelected = true;
                }
                ICommand command = null;
                object commandArg;
                string header = cc;
                bool is_palette_picker = false;
                if (ucvm is MpIColorPalettePickerViewModel cppvm) {
                    command = cppvm.PaletteColorPickedCommand;
                    is_palette_picker = true;
                }
                if (isCustom) {
                    if (!isAnySelected) {
                        isSelected = true;
                        // if selected color is custom make background of custom icon that color (default white)
                        header = selectedHexStr;
                    }
                    if (is_palette_picker) {
                        commandArg = "custom";
                    } else {
                        command = Mp.Services.CustomColorChooserMenuAsync.SelectCustomColorCommand;
                        commandArg = ucvm;
                    }
                } else {
                    if (is_palette_picker) {
                        commandArg = cc;
                    } else {
                        command = SetColorCommand;
                        commandArg = new object[] { ucvm, cc };
                    }
                }

                colors.Add(new MpAvMenuItemViewModel() {
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
            return new MpAvMenuItemViewModel() {
                HasLeadingSeparator = has_leading_sep,
                Header = "Color",
                IconResourceKey = "ColorsImage",
                SubItems = new[] {
                    new MpAvMenuItemViewModel() {
                        IsColorPallete = true,
                        SubItems =
                            colors
                            .OrderBy(x => x.SortOrderIdx)
                            .Cast<MpAvIMenuItemViewModel>()
                            .ToList()
                    }
                }
            };
        }


        public override string ToString() {
            return Header;
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
                Mp.Services.MainThreadMarshal.RunOnMainThread(() => {
                    var argParts = args as object[];
                    MpIUserColorViewModel ucvm = argParts[0] as MpIUserColorViewModel;
                    string hexColor = argParts[1] as string;
                    ucvm.UserHexColor = hexColor;
                    MpAvMenuView.CloseMenu();
                });
            });

        public bool IsSubMenuOpen { get; set; }



        #endregion
    }
}
