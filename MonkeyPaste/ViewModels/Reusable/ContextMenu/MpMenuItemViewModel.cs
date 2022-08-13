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
        MpMenuItemViewModel ContextMenuViewModel { get; }
    }

    public interface MpIPopupMenuViewModel : MpIMenuItemViewModelBase {
        MpMenuItemViewModel PopupMenuViewModel { get; }
    }




    public class MpSetColorArguments {
        public event EventHandler<string> SetColorEventCallback;
        public string OriginalColor { get; }
    }

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

        #endregion

        #region Header

        public int HeaderIndentLevel { get; set; }

        public double HeaderIndentSize { get; set; } = 20;

        public string HeaderedSeparatorLabel { get; set; }

        public string Header { get; set; }

        #endregion

        #region State

        public bool IsEnabled { get; set; }

        public bool IsChecked { get; set; } = false;

        public bool IsPartiallySelected { get; set; } = false; // for multi-select tag ischecked overlay

        public bool IsHovering { get; set; }

        public bool IsCustomColorButton { get; set; }

        public int SortOrderIdx { get; set; }
        #endregion

        #region Appearance

        public bool CanHide { get; set; } // for eye button on paste to path

        public bool IsVisible { get; set; } = true;

        #endregion

        #region Commands

        public ICommand Command { get; set; }

        public object CommandParameter { get; set; }

        #endregion

        #region InputGesture

        private string _inputGestureText = string.Empty;
        public string InputGestureText {
            get {
                if (ShortcutObjId > 0 || ShortcutType != MpShortcutType.None) {
                    return MpDataModelProvider.GetShortcutKeystring(ShortcutType.ToString(), ShortcutObjId.ToString());
                }
                return _inputGestureText;
            }
            set {
                if(InputGestureText != value) {
                    _inputGestureText = value;
                    OnPropertyChanged(nameof(InputGestureText));
                }
            }
        }

        public int ShortcutObjId { get; set; } = 0;

        public MpShortcutType ShortcutType { get; set; } = MpShortcutType.None;

        #endregion

        #region Icon

        public string BorderHexColor {
            get {
                if (IsChecked) {
                    return MpSystemColors.IsSelectedBorderColor;
                } else if (IsHovering) {
                    return MpSystemColors.IsHoveringBorderColor;
                }
                return MpSystemColors.DarkGray;
            }
        }

        public bool IsIconHidden => IconSourceObj == null;

        public int IconId { get; set; } = 0;

        public string IconResourceKey { get; set; } = string.Empty;

        public string IconHexStr { get; set; } = string.Empty;

        public Uri IconSourceUri { get; set; }

        public object IconSourceObj {
            get {
                if(IconId > 0) {
                    return IconId;
                }
                if(IconHexStr.IsStringHexColor()) {
                    return IconHexStr;
                }
                if(IconResourceKey.IsStringResourcePath()) {
                    return IconResourceKey;
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
