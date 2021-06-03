using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MonkeyPaste.Messages {
    public class MpMessage {
        public Type TypeInfo { get; set; }
        public string Id { get; set; }
        public string Username { get; set; }
        public DateTime Timestamp { get; set; }

        public MpMessage() { }
        public MpMessage(string username) {
            Id = Guid.NewGuid().ToString();
            TypeInfo = GetType();
            Username = username;
            Timestamp = DateTime.Now;
        }
    }
}
