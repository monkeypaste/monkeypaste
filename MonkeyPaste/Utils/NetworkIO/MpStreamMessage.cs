using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public enum MpStreamMesageType {
        None = 0,
        HandshakeStart,
        HandshakeBack,
        RequestLog,
        ResponseLog,
        ConfirmLog,
        RequestData,
        ResponseData,
        ConfirmData,
        RequestComplete,
        ConfirmComplete,
        Disconnect
    }

    public class MpStreamHeader {
        public DateTime MessageDateTime { get; set; }
        public MpStreamMesageType MessageType { get; set; }
        public string ClientGuid { get; set; }
    }

    public class MpStreamMessage {
        public const string HeaderContentParseToken = @"#^$*&";
        public const string HeaderParseToken = @"$$##@";

        public string Header { get; set; }
        public string Contents { get; set; }

        public static MpStreamMessage Create(MpStreamMesageType msgType, string content, string thisGuid) {
            // Stream Message Format: <SendDateTime><MessageTypeId><ClientGuid><Content>
            var sm = new MpStreamMessage();
            sm.Contents = content;
            sm.Header = sm.CreateHeader(msgType,thisGuid);
            return sm;
        }

        public static MpStreamHeader ParseHeader(string streamMessageStr) {
            if (string.IsNullOrEmpty(streamMessageStr)) {
                return null;
            }
            var msgParts = streamMessageStr.Split(new string[] { MpStreamMessage.HeaderContentParseToken }, StringSplitOptions.RemoveEmptyEntries);
            if(msgParts.Length == 0) {
                return null;
            }
            string headerStr = msgParts[0];
            var headerParts = headerStr.Split(new string[] { MpStreamMessage.HeaderParseToken }, StringSplitOptions.RemoveEmptyEntries);
            var header = new MpStreamHeader() {
                MessageDateTime = DateTime.Parse(headerParts[0]),
                MessageType = (MpStreamMesageType)Convert.ToInt32(headerParts[1]),
                ClientGuid = headerParts[2]
            };
            return header;
        }

        protected string CreateHeader(MpStreamMesageType msgType, string thisGuid) {
            return string.Format(
                @"{1}{0}{2}{0}{3}",
                HeaderParseToken,
                DateTime.Now.ToString(),
                (int)msgType,
                thisGuid);

            //string header = string.Empty;

            //switch(msgType) {
            //    case MpStreamMesageType.RequestLog:

            //        break;
            //}
        }

        public override string ToString() {
            return string.Format(@"{0}{1}{2}", Header,HeaderContentParseToken,Contents);
        }
    }
}
