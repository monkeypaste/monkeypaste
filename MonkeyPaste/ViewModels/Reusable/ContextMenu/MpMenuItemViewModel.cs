using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Linq;
using MonkeyPaste.Common;

namespace MonkeyPaste {

    public interface MpIMenuItemViewModel {
        MpMenuItemViewModel MenuItemViewModel { get; }
    }

    public class MpMenuItemViewModel : MpViewModelBase {
        #region Properties

        #region View Models

        public IList<MpMenuItemViewModel> SubItems { get; set; }

        #endregion

        #region Data Template Helpers

        public bool IsPasteToPathRuntimeItem { get; set; }

        public bool IsSeparator { get; set; }

        public bool IsHeaderedSeparator { get; set; }

        public bool IsColorPallete { get; set; }

        #endregion

        #region Header

        public int HeaderIndentLevel { get; set; }

        public double HeaderIndentSize { get; set; } = 20;

        public string HeaderedSeparatorLabel { get; set; }

        public string Header { get; set; }

        #endregion

        #region State

        public bool IsSelected { get; set; } = false;

        public bool IsPartiallySelected { get; set; } = false; // for multi-select tag ischecked overlay

        public bool IsHovering { get; set; }

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
                    return MpDataModelProvider.GetShortcutKeystring(ShortcutType, ShortcutObjId);
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
                if (IsSelected) {
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
                return null;
            }
        }

        #endregion

        #region Tooltip

        public object Tooltip { get; set; }

        public bool HasTooltip => Tooltip != null;

        #endregion        

        #endregion

        #region Public Methods

        public static MpMenuItemViewModel GetColorPalleteMenuItemViewModel(MpIUserColorViewModel ucvm) {
            bool isAnySelected = false;
            var colors = new List<MpMenuItemViewModel>();
            string selectedHexStr = ucvm.UserHexColor == null ? string.Empty:ucvm.UserHexColor;
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
                    IsSelected = isSelected,
                    Header = header,
                    Command = command,
                    CommandParameter = commandArg,
                    IsVisible = isCustom
                });
            }
            
            return new MpMenuItemViewModel() {
                IsColorPallete = true,
                SubItems = colors
            };
        }

        public MpMenuItemViewModel() : base(null)  {
            //PropertyChanged += MpContextMenuItemViewModel_PropertyChanged;
            //IsSeparator = true;
        }
               
        #endregion
         
        #region Private Methods

        private void MpContextMenuItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            //switch (e.PropertyName) {
            //}
        }

        #endregion

        #region Commands

        public static ICommand SetColorCommand => new MpCommand<object>(
            (args) => {
                if(args == null) {
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
