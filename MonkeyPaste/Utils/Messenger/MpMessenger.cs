using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public enum MpMessageType {
        None,
        RequeryCompleted,
        QueryChanged,
        SubQueryChanged, //sent when composite parent is moved or tile deleted
        JumpToIdxCompleted,
        TotalQueryCountChanged,
        Expand, //has context (tile)
        Unexpand,   //has context (tile)
        ExpandComplete,
        UnexpandComplete,
        MainWindowOpening,
        MainWindowOpened,
        MainWindowHiding,
        MainWindowHid,
        ItemInitialized,
        ItemDragBegin,
        ItemDragEnd,
        TrayScrollChanged,
        TraySelectionChanged,
        ContentListScrollChanged, //has context (tile)
        ContentListItemsChanged, //has context (tile)
        Resizing,
        ResizeCompleted,
        SelectNextMatch,
        SelectPreviousMatch,
        SearchCriteriaItemsChanged,
        TagTileNotificationAdded,
        TagTileNotificationRemoved,
        Loaded, //has context (object)
        Busy,
        NotBusy,
    }

    public static partial class MpMessenger {

        private static readonly ConcurrentDictionary<MessengerKey, List<object>> _recipientDictionary = new ConcurrentDictionary<MessengerKey, List<object>>();

        public static void Register(object sender, Action<MpMessageType> callback, object context = null) {
            Register<MpMessageType>(sender, callback, context);
        }

        public static void Register<T>(object sender, Action<T> callback) {
            Register(sender, callback, null);
        }

        public static void Register<T>(object sender, Action<T> action, object context) {
            var key = new MessengerKey(sender, typeof(T), context);
            if(_recipientDictionary.ContainsKey(key)) {
                _recipientDictionary[key].Add(action);
            } else {
                _recipientDictionary.TryAdd(key, new List<object> { action });
            }            
        }

        public static void Unegister(object sender, Action<MpMessageType> callback, object context = null) {
            Unregister<MpMessageType>(sender, callback, context);
        }

        public static void Unregister<T>(object sender, Action<T> callback) {
            Unregister(sender, callback, null);
        }

        public static void Unregister<T>(object recipient, Action<T> action, object context) {
            var key = new MessengerKey(recipient, typeof(T), context);
            //RecipientDictionary.TryRemove(key, out removeAction);
            if (_recipientDictionary.ContainsKey(key)) {
                _recipientDictionary[key].Remove(action);
            }
        }

        public static void UnregisterAll() {
            _recipientDictionary.Clear();
        }

        public static void Send(MpMessageType message, object context = null) {
            Send<MpMessageType>(message, context);
        }

        public static void Send<T>(T message) {
            Send(message, null);
        }

        public static void Send<T>(T message, object context) {
            IEnumerable<KeyValuePair<MessengerKey, List<object>>> results;

            if (context == null) {
                // Get all recipients where the context is null.
                results = from r in _recipientDictionary where r.Key.Context == null select r;
            } else {
                // Get all recipients where the context is matching.
                results = from r in _recipientDictionary where r.Key.Context != null && r.Key.Context.Equals(context) select r;
            }

            foreach(var result in results) {
                foreach(var action in result.Value.ToList()) {
                    // NOTE enumerating over .ToList() to avoid colleciton changed exception
                    (action as Action<T>).Invoke(message);
                }
            }
        }

        #region (Internal class) Message Key

        internal class MessengerKey {
            public  object Recipient { get; private  set; }
            public  Type MessageType { get; private  set; }
            public  object Context { get; private  set; }

            public  MessengerKey(object recipient, Type messageType, object context) {
                Recipient = recipient;
                MessageType = messageType;
                Context = context;
            }

            protected bool Equals(MessengerKey other) {
                return Equals(Recipient, other.Recipient)
                    && Equals(MessageType, other.MessageType)
                    && Equals(Context, other.Context);
            }

            public  override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;

                return Equals((MessengerKey)obj);
            }

            public  override int GetHashCode() {
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
