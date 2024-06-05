using System.Collections.Generic;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public enum MpMenuItemType {
        None = 0,
        Default,
        Checkable,
        CheckableWithIcon,
        ColorPalette,
        ColorPaletteItem,
    }

    public interface MpAvIMenuItemViewModel : MpIHoverableViewModel, MpIViewModel {
        ICommand Command { get; }
        object CommandParameter { get; }
        string Header { get; }
        object IconSourceObj { get; }
        string InputGestureText { get; }
        bool StaysOpenOnClick { get; }
        bool HasLeadingSeparator { get; }
        bool IsVisible { get; }
        bool? IsChecked { get; }
        bool IsThreeState { get; }
        bool IsSubMenuOpen { get; set; }
        string IconBorderHexColor { get; }
        string IconTintHexStr { get; }
        MpMenuItemType MenuItemType { get; }
        IEnumerable<MpAvIMenuItemViewModel> SubItems { get; }
    }

}