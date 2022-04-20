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
        IsReadOnly, //has context (tile)
        IsEditable,   //has context (tile)
        ResizingMainWindowComplete,
        //UnexpandComplete,
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
        ContentItemsChanged, //has context (tile)
        ResizingContent,
        ResizeContentCompleted,
        SelectNextMatch,
        SelectPreviousMatch,
        SearchCriteriaItemsChanged,
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

        public static void Register<T>(object sender, Action<T> receiverAction, object context) {
            var key = new MessengerKey(sender, typeof(T), context);
            if(_recipientDictionary.ContainsKey(key)) {
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


        public static void UnregisterAll() {
            _recipientDictionary.Clear();
           // _globalRecipientDictionary.Clear();
        }

        public static void SendGlobal<T>(T message) {
            Send(message, null);
        }

        public static void Send<T>(T message, object context) {
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

            foreach(var result in results) {
                foreach(var receiverAction in result.Value.ToList()) {
                    // NOTE enumerating over .ToList() to avoid colleciton changed exception
                    (receiverAction as Action<T>)?.Invoke(message);
                }
            }
        }

        #region (Internal class) Message Key

        internal class MessengerKey {
            public object Recipient { get; private  set; }
            public Type MessageType { get; private  set; }
            public object Context { get; private  set; }

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
