using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonkeyPaste {
    // from https://stackoverflow.com/a/68272972/105028
    public enum MpMessageType {
        None,
        Requery,
        QueryChanged,
        ItemsInitialized,
        Expand,
        Unexpand,
        MainWindowOpening,
        MainWindowOpened,
        MainWindowHiding,
        MainWindowHid
    }

    public class MpMessenger {
        private static readonly Lazy<MpMessenger> _Lazy = new Lazy<MpMessenger>(() => new MpMessenger());
        public static MpMessenger Instance { get { return _Lazy.Value; } }

        private readonly ConcurrentDictionary<MessengerKey, object> RecipientDictionary = new ConcurrentDictionary<MessengerKey, object>();

        public MpMessenger() { }

        public void Register<T>(object recipient, Action<T> action) {
            Register(recipient, action, null);
        }

        public void Register<T>(object recipient, Action<T> action, object context) {
            var key = new MessengerKey(recipient, typeof(T), context);
            RecipientDictionary.TryAdd(key, action);
        }

        public void Unregister<T>(object recipient, Action<T> action) {
            Unregister(recipient, action, null);
        }

        public void Unregister<T>(object recipient, Action<T> action, object context) {
            object removeAction;
            var key = new MessengerKey(recipient, typeof(T), context);
            RecipientDictionary.TryRemove(key, out removeAction);
        }

        public void UnregisterAll() {
            RecipientDictionary.Clear();
        }

        public void Send<T>(T message) {
            Send(message, null);
        }

        public void Send<T>(T message, object context) {
            IEnumerable<KeyValuePair<MessengerKey, object>> result;

            if (context == null) {
                // Get all recipients where the context is null.
                result = from r in RecipientDictionary where r.Key.Context == null select r;
            } else {
                // Get all recipients where the context is matching.
                result = from r in RecipientDictionary where r.Key.Context != null && r.Key.Context.Equals(context) select r;
            }

            foreach (var action in result.Select(x => x.Value).OfType<Action<T>>()) {
                // Send the message to all recipients.
                action(message);
            }
        }

        protected class MessengerKey {
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
    }
}
