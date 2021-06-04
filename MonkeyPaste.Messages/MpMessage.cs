using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace MonkeyPaste.Messages {
    public class MpMessage {
        public Type TypeInfo { get; set; }
        public string Id { get; set; }
        public string Username { get; set; }
        public DateTime Timestamp { get; set; }
        public string Ip4Address { get; set; }

        public MpMessage() { 
            var ipAddress = Dns.GetHostAddresses(Dns.GetHostName()).FirstOrDefault();
            if (ipAddress != null) {
                Ip4Address = ipAddress.ToString();
            }
        }
        public MpMessage(string username) : this() {
            Id = Guid.NewGuid().ToString();
            TypeInfo = GetType();
            Username = username;
            Timestamp = DateTime.Now;
        }
    }
}
