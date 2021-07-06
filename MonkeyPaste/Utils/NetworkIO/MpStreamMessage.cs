using Acr.UserDialogs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public enum MpSyncMesageType {
        None = 0,
        Connect,
        HandshakeStart,
        HandshakeBack,
        RequestLog,
        ResponseLog,
        RequestData,
        ResponseData,
        RequestComplete,
        FlipStart,
        FlipDone,
        DisconnectStart,
        DisconnectDone
    }

    public class MpStreamHeader {
        private string _headerStr;
        public const string HeaderParseToken = @"$$##@";

        public DateTime MessageDateTime { get; set; }
        public MpSyncMesageType MessageType { get; set; }
        public string ClientGuid { get; set; }

        public static MpStreamHeader Parse(string headerStr) {
            var headerParts = headerStr.Split(new string[] { HeaderParseToken }, StringSplitOptions.RemoveEmptyEntries);
            return new MpStreamHeader() {
                MessageDateTime = DateTime.Parse(headerParts[0]),
                MessageType = (MpSyncMesageType)Convert.ToInt32(headerParts[1]),
                ClientGuid = headerParts[2]
            };
        }
        public override string ToString() {
            //header string format: <SendDateTime><MessageTypeId><ClientGuid>
            return string.Format(
                 @"{1}{0}{2}{0}{3}",
                 HeaderParseToken,
                 MessageDateTime,
                 (int)MessageType,
                 ClientGuid);
        }
    }

    public class MpStreamMessage {
        public const string HeaderContentParseToken = @"#^$*&";

        public MpStreamHeader Header { get; set; }
        public string Content;

        public static MpStreamMessage Create(MpSyncMesageType msgType, string content, string thisGuid) {
            // Stream Message Format: <header><Content><eof>
            var sm = new MpStreamMessage() {
                Header = new MpStreamHeader() {
                    MessageType = msgType,
                    ClientGuid = thisGuid,
                    MessageDateTime = DateTime.Now,
                },
                Content = content
            };
            return sm;
        }

        public static MpStreamMessage Parse(string streamMessageStr) {
            if (string.IsNullOrEmpty(streamMessageStr)) {
                return null;
            }
            var msgParts = streamMessageStr.Split(new string[] { MpStreamMessage.HeaderContentParseToken }, StringSplitOptions.RemoveEmptyEntries);
            if(msgParts.Length == 0) {
                return null;
            }
            return new MpStreamMessage() {
                Header = MpStreamHeader.Parse(msgParts[0]),
                Content = msgParts[1]
            };            
        }        

        public override string ToString() {
            return string.Format(@"{0}{1}{2}", Header,HeaderContentParseToken,Content);
        }
    }
}
