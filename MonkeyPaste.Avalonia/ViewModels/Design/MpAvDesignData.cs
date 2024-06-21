using MonkeyPaste.Common;
using System.Collections.ObjectModel;

namespace MonkeyPaste.Avalonia {

    public static class MpAvDesignData {
        //public static void CheckEnumUiStrings() { }
        //public static MpAvSettingsViewModel Design { get; } = new MpAvSettingsViewModel {
        //    // View Model initialization here.
        //};

        //public static MpAvShortcutCollectionViewModel ShortcutCollection { get; } = new MpAvShortcutCollectionViewModel {
        //    Items = new ObservableCollection<MpAvShortcutViewModel>() {
        //        new MpAvShortcutViewModel(null) {
        //            Shortcut = new MpShortcut() {
        //                DefaultKeyString = "Control+S",
        //                KeyString = "Control+S",
        //                RoutingType = MpRoutingType.Bubble,
        //                ShortcutType = MpShortcutType.ToggleMainWindow
        //            }
        //        },
        //        new MpAvShortcutViewModel(null) {
        //            Shortcut = new MpShortcut() {
        //                DefaultKeyString = "Control+F5|Control+F4",
        //                KeyString = "Control+F5|Control+F4",
        //                RoutingType = MpRoutingType.Bubble,
        //                ShortcutType = MpShortcutType.ToggleMainWindowLocked
        //            }
        //        }
        //    }
        //};


        public static MpAvWelcomeNotificationViewModel WelcomeViewModel { get; } = new MpAvWelcomeNotificationViewModel {
            NotificationFormat = new MpNotificationFormat() {
                NotificationType = MpNotificationType.Welcome,
                Title = "Greeting",
                Body = "Body"
            }
        };

    }
    public class DesignTriggerCollectionViewModel : MpAvTriggerCollectionViewModel {
        public DesignTriggerCollectionViewModel() {
            var trigger = new MpAvContentAddTriggerViewModel(this) {
                Action = new MpAction() {
                    ActionType = MpActionType.Trigger,
                    Arg1 = "1,0,0,true", // scale, trans-x,trans-y,show grid
                    Arg2 = "true", // isEnabled
                    Arg3 = MpTriggerType.ClipAdded.ToString(),
                    Label = "Test Trigger",
                    Description = "Test Description"
                }
            };
            Items.Add(trigger);
        }
    }
}
