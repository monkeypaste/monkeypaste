using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpStreamHeader : MpISyncableDbObject {
        public const string HeaderParseToken = @"$$##@";

        public DateTime MessageDateTime { get; set; }
        public MpSyncMesageType MessageType { get; set; }

        public string FromGuid { get; set; }
        public string ToGuid { get; set; }

        public string ContentCheckSum { get; private set; }

        public MpStreamHeader(
            MpSyncMesageType msgType,
            string fromGuid,
            string toGuid,
            DateTime sendDateTime,
            string checkSum = "") {
            MessageType = msgType;
            FromGuid = fromGuid;
            ToGuid = toGuid;
            MessageDateTime = sendDateTime;
            ContentCheckSum = checkSum;
        }


        public static MpStreamHeader Parse(string headerStr) {
            //header string format: <MessageTypeId><FromGuid><ToGuid><SendDateTime><checksum>
            var headerParts = headerStr.Split(new string[] { HeaderParseToken }, StringSplitOptions.RemoveEmptyEntries);
            var header = new MpStreamHeader(
                (MpSyncMesageType)Convert.ToInt32(headerParts[0]),
                 headerParts[1],
                 headerParts[2],
                 DateTime.Parse(headerParts[3]),
                 headerParts[4]
            );
            return header;
        }

        public string SerializeDbObject() {
            //header string format: <MessageTypeId><FromGuid><ToGuid><SendDateTime><checksum>
            return string.Format(
                 @"{1}{0}{2}{0}{3}{0}{4}{0}{5}",
                 HeaderParseToken,
                 (int)MessageType,
                 FromGuid,
                 ToGuid,
                 MessageDateTime.ToString(),
                 ContentCheckSum);
        }

        public Type GetDbObjectType() {
            return typeof(MpStreamHeader);
        }


        public Task<object> DeserializeDbObject(string objStr) {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> DbDiff(object drOrModel) {
            throw new NotImplementedException();
        }

        public Task<object> CreateFromLogs(string dboGuid, List<MpDbLog> logs, string fromClientGuid) {
            throw new NotImplementedException();
        }
    }
}
