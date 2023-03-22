using System.Collections.ObjectModel;

namespace MonkeyPaste.Avalonia {
    public static class MpAvDesignData {
        public static void Init() { }
        public static MpAvSettingsViewModel Design { get; } = new MpAvSettingsViewModel {
            // View Model initialization here.
        };

        public static MpAvShortcutCollectionViewModel ShortcutCollection { get; } = new MpAvShortcutCollectionViewModel {
            Items = new ObservableCollection<MpAvShortcutViewModel>() {
                new MpAvShortcutViewModel(null) {
                    Shortcut = new MpShortcut() {
                        DefaultKeyString = "Control+S",
                        KeyString = "Control+S",
                        RoutingType = MpRoutingType.Bubble,
                        ShortcutType = MpShortcutType.ToggleMainWindow
                    }
                },
                new MpAvShortcutViewModel(null) {
                    Shortcut = new MpShortcut() {
                        DefaultKeyString = "Control+F5|Control+F4",
                        KeyString = "Control+F5|Control+F4",
                        RoutingType = MpRoutingType.Bubble,
                        ShortcutType = MpShortcutType.ToggleMainWindowLocked
                    }
                }
            }
        };

        public static MpAvAssignShortcutViewModel AssignShortcutViewModel { get; } = new MpAvAssignShortcutViewModel() {
            ShortcutDisplayName = "This is a test title",
            KeyString = "Control+T|Control+L",
            WarningString = "THis is shortcut warning 1",
            WarningString2 = "THis is shortcut warning 2",
            IconResourceObj = "KeyboardImage"
        };
    }
}
