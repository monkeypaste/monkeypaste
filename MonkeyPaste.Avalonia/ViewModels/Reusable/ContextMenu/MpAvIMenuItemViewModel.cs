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
    public interface MpAvIMenuItemCollectionViewModel : MpIViewModel {
        bool IsMenuOpen { get; set; }
        IEnumerable<MpAvIMenuItemViewModel> Items { get; }
    }
    public interface MpAvIAnchoredMenuItemCollectionViewModel : MpAvIMenuItemCollectionViewModel {
        object MenuAnchorObj { get; }
    }
    public interface MpAvIMenuItemViewModel : MpIViewModel {
        ICommand Command { get; }
        object CommandParameter { get; }
        string Header { get; }
        object IconSourceObj { get; }
        string InputGestureText { get; }
        bool StaysOpenOnClick { get; }
        bool HasLeadingSeparator { get; }
        bool IsVisible { get; }
        bool? IsChecked { get; }
        MpMenuItemType MenuItemType { get; }
        IEnumerable<MpAvIMenuItemViewModel> SubItems { get; }
    }

}