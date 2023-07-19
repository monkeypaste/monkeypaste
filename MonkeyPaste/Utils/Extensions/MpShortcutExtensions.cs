namespace MonkeyPaste {
    public static class MpShortcutExtensions {

        public static bool CanBeGlobal(this MpShortcutType st) {
            switch (st) {
                // APPLICATION
                case MpShortcutType.ToggleMainWindow:
                case MpShortcutType.ToggleAppendInsertMode:
                case MpShortcutType.ToggleAppendLineMode:
                case MpShortcutType.ToggleAutoCopyMode:
                case MpShortcutType.ToggleRightClickPasteMode:
                case MpShortcutType.ToggleListenToClipboard:
                case MpShortcutType.ToggleDropWidgetEnabled:
                // USER
                case MpShortcutType.PasteCopyItem:
                case MpShortcutType.InvokeTrigger:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsEditorShortcut(this MpShortcutType st) {
            switch (st) {
                case MpShortcutType.ToggleAppendInsertMode:
                case MpShortcutType.ToggleAppendLineMode:
                case MpShortcutType.ToggleAppendPreMode:
                case MpShortcutType.ToggleAppendPaused:
                case MpShortcutType.ToggleAppendManualMode:
                    return true;
                default:
                    return false;
            }
        }


        public static bool IsUserDefinedShortcut(this MpShortcutType stype) {
            return (int)stype > (int)MpShortcutType.MAX_APP_SHORTCUT;
        }

        public static MpRoutingType GetProfileBasedRoutingType(
            this MpShortcutRoutingProfileType profile,
            MpShortcutType st = MpShortcutType.None) {
            if (!st.CanBeGlobal()) {
                return MpRoutingType.Internal;
            }

            switch (profile) {
                case MpShortcutRoutingProfileType.Global:
                    if (st == MpShortcutType.ToggleMainWindow) {
                        return MpRoutingType.Override;
                    }
                    return MpRoutingType.Passive;
                default:
                case MpShortcutRoutingProfileType.Internal:
                    return MpRoutingType.Internal;
            }
        }
    }
}
