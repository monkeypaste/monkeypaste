using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste {
    public enum MpMessageType {
        None,

        QueryCompleted,
        RequeryCompleted,
        QueryChanged,
        SubQueryChanged,
        JumpToIdxCompleted,
        TotalQueryCountChanged,

        IsReadOnly, //has context (tile)
        IsEditable,   //has context (tile)
        ResizingMainWindowComplete,

        //UnexpandComplete,
        MainWindowOpening,
        MainWindowOpened,
        MainWindowClosing,
        MainWindowClosed,

        MainWindowActivated,
        MainWindowDeactivated,

        MainWindowLocked,
        MainWindowUnlocked,

        MainWindowOrientationChangeBegin,
        MainWindowOrientationChangeEnd,

        MainWindowSizeChangeBegin,
        MainWindowSizeChanged,
        MainWindowSizeChangeEnd,

        MainWindowLoadComplete,

        ShortcutAssignmentStarted,
        ShortcutAssignmentEnded,

        ItemInitialized,

        ItemDragBegin,
        ItemDragEnd,
        ItemDragCanceled,

        DropOverTraysBegin,
        DropOverTraysEnd,

        //ExternalDragBegin, 
        //ExternalDragEnd,

        ClipboardPresetsChanged,

        TrayScrollChanged,

        TraySelectionChanged,

        TrayLayoutChanged,

        SidebarItemSizeChanged,
        SelectedSidebarItemChanged,

        PinTrayEmptyOrHasTile,

        TrayZoomFactorChangeBegin,
        TrayZoomFactorChanged,
        TrayZoomFactorChangeEnd,

        ContentListScrollChanged, //has context (tile)
        ContentItemsChanged, //has context (tile)

        // START Sound ntf messages 

        ContentAdded,
        ContentPasted,

        AppError,

        AppendModeActivated,
        AppendModeDeactivated,
        AppendBufferChanged,

        RightClickPasteEnabled,
        RightClickPasteDisabled,

        AutoCopyEnabled,
        AutoCopyDisabled,

        // END Sound ntf messages 

        SettingsFilterTextChanged,

        ContentResized,
        ResizeContentCompleted,

        ChildWindowClosed,

        SettingsWindowOpened,

        AdvancedSearchExpandedChanged,

        TagSelectionChanged,

        SelectNextMatch,
        SelectPreviousMatch,

        SearchCriteriaItemsChanged,
        QuerySortChanged,

        TagTileNotificationAdded,
        TagTileNotificationRemoved,

        Loaded, //has context (object)
        Busy,
        NotBusy
    }

    public static partial class MpMessenger {
        //private static readonly List<object> _globalRecipientDictionary = new ConcurrentDictionary<MessengerKey, List<object>>();
        private static readonly ConcurrentDictionary<MessengerKey, List<object>> _recipientDictionary = new ConcurrentDictionary<MessengerKey, List<object>>();

        public static void Register<T>(object sender, Action<T> receiverAction) {
            Register(sender, receiverAction, null);
        }

        public static void RegisterGlobal(Action<MpMessageType> receiverAction) {
            Register<MpMessageType>(null, receiverAction, null);
        }

        public static void Register<T>(object sender, Action<T> receiverAction, object context) {
            var key = new MessengerKey(sender, typeof(T), context);
            if (_recipientDictionary.ContainsKey(key)) {
                if (_recipientDictionary[key].Contains(receiverAction)) {
                    // this probably shouldn't happen, needs to be unregistered or remove old entry

                    //Debugger.Break();

                    //MpConsole.WriteLine("Warning, re-registering message receipient " + receiverAction.Target + " there are " + _recipientDictionary[key].Count + " instances for this receiver type");
                }
                _recipientDictionary[key].Add(receiverAction);
            } else {
                _recipientDictionary.TryAdd(key, new List<object> { receiverAction });
            }
        }

        public static void Unregister<T>(object sender, Action<T> receiverAction) {
            Unregister(sender, receiverAction, null);
        }

        public static void Unregister<T>(object sender, Action<T> receiverAction, object context) {
            var key = new MessengerKey(sender, typeof(T), context);
            //RecipientDictionary.TryRemove(key, out removeAction);
            if (_recipientDictionary.ContainsKey(key)) {
                _recipientDictionary[key].Remove(receiverAction);
            }
        }
        public static void UnregisterGlobal(Action<MpMessageType> receiverAction) {
            Unregister<MpMessageType>(null, receiverAction, null);
        }

        public static void UnregisterAll() {
            _recipientDictionary.Clear();
            // _globalRecipientDictionary.Clear();
        }

        public static void SendGlobal<T>(T message) {
            Send(message, null);
        }

        public static void Send<T>(T message, object context) {
            //MpConsole.WriteLine("Messenger sending: " + message.ToString());

            IEnumerable<KeyValuePair<MessengerKey, List<object>>> results;

            if (context == null) {
                // Get all recipients where the context is null.
                results = _recipientDictionary.Where(x => x.Key.Context == null).Select(x => x);
                //results = from r in _recipientDictionary where r.Key.Context == null select r;
            } else {
                // Get all recipients where the context is matching.
                results = _recipientDictionary.Where(x => x.Key.Context != null && x.Key.Context.Equals(context)).Select(x => x);
                //results = from r in _recipientDictionary where r.Key.Context != null && r.Key.Context.Equals(context) select r;
            }

            foreach (var result in results) {
                foreach (var receiverAction in result.Value.ToList()) {
                    // NOTE enumerating over .ToList() to avoid colleciton changed exception
                    (receiverAction as Action<T>)?.Invoke(message);
                }
            }
        }

        #region (Internal class) Message Key

        internal class MessengerKey {
            public object Recipient { get; private set; }
            public Type MessageType { get; private set; }
            public object Context { get; private set; }

            public MessengerKey(object recipient, Type messageType, object context) {
                Recipient = recipient;
                MessageType = messageType;
                Context = context;
            }

            protected bool Equals(MessengerKey other) {
                return Equals(Recipient, other.Recipient)
                    && Equals(MessageType, other.MessageType)
                    && Equals(Context, other.Context);
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;

                return Equals((MessengerKey)obj);
            }

            public override int GetHashCode() {
                unchecked {
                    return ((Recipient != null ? Recipient.GetHashCode() : 0) * 397)
                        ^ ((MessageType != null ? MessageType.GetHashCode() : 0) * 397)
                        ^ (Context != null ? Context.GetHashCode() : 0);
                }
            }
        }

        #endregion
    }
}
