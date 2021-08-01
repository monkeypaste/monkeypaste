using Acr.UserDialogs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public enum MpSyncMesageType {
        None = 0,
        WebDeviceRequest,
        WebDeviceResponse,
        HandshakeRequest,
        HandshakeResponse,
        DbLogRequest,
        DbLogResponse,
        FlipRequest, //swap A & B and return to RequestLog
        DisconnectRequest,
        DisconnectResponse,       
        WebDeviceDisconnect,
        //error types
        ErrorBase, //only used to differentiate with normal msgs
        ErrorNotConnected,
        ErrorInvalidChecksum,
        ErrorInvalidAccessToken,
        ErrorInvalidData,
        ErrorRequestDenied,
        ErrorOutOfMemory,        
    }

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

    public class MpStreamMessage : MpISyncableDbObject {
        public const string HeaderContentParseToken = @"#^$*@";
        public const string EofToken = @"<EOF>";

        public MpStreamHeader Header { get; set; }

        public string Content { get; set; } = string.Empty;

        public MpStreamMessage() { }

        public MpStreamMessage(
            MpSyncMesageType msgType, 
            string fromGuid, 
            string toGuid, 
            string content) {
            Header = new MpStreamHeader(
                msgType, 
                fromGuid, 
                toGuid, 
                DateTime.UtcNow,
                content.CheckSum());
            Content = content;
        }

        #region Sync Phase Message Builders
        public static MpStreamMessage CreateWebDeviceRequest(MpDeviceEndpoint dep) {
            var sm = new MpStreamMessage(
                MpSyncMesageType.WebDeviceRequest,
                dep.DeviceGuid,
                "hub",
                dep.SerializeDbObject());
            return sm;
        }

        public static MpStreamMessage CreateHandshakeRequest(MpDeviceEndpoint dep) {
            var sm = new MpStreamMessage(
                MpSyncMesageType.HandshakeRequest,
                dep.DeviceGuid,
                "hub",
                dep.SerializeDbObject());
            return sm;
        }

        public static MpStreamMessage CreateHandshakeResponse(MpDeviceEndpoint dep,string toGuid) {
            var sm = new MpStreamMessage(
                MpSyncMesageType.HandshakeResponse,
                dep.DeviceGuid,
                toGuid,
                dep.SerializeDbObject());
            return sm;
        }

        public static MpStreamMessage CreateDbLogRequest(MpDeviceEndpoint dep, string toGuid, DateTime lastSyncUtc) {
            var sm = new MpStreamMessage(
                MpSyncMesageType.DbLogRequest,
                dep.DeviceGuid,
                toGuid,
                lastSyncUtc.ToString());
            return sm;
        }

        public static MpStreamMessage CreateDbLogResponse(MpDeviceEndpoint dep, string toGuid, string logDbMessageStr) {
            var sm = new MpStreamMessage(
                MpSyncMesageType.DbLogResponse,
                dep.DeviceGuid,
                toGuid,
                logDbMessageStr);
            return sm;
        }
                
        public static MpStreamMessage CreateFlipRequest(MpDeviceEndpoint dep, string toGuid) {
            // once B has needed A data it will make a flipRequest = true
            // once A has needed B data it will make a flipRequest of false to finish sync
            var sm = new MpStreamMessage(
                MpSyncMesageType.FlipRequest,
                dep.DeviceGuid,
                toGuid,
                @"FlipRequest");
            return sm;
        }

        public static MpStreamMessage CreateDisconnectRequest(MpDeviceEndpoint dep, string toGuid, DateTime newSyncUtc) {
            var sm = new MpStreamMessage(
                MpSyncMesageType.DisconnectRequest,
                dep.DeviceGuid,
                toGuid,
                newSyncUtc.ToString());
            return sm;
        }

        public static MpStreamMessage CreateDisconnectResponse(MpDeviceEndpoint dep, string toGuid, DateTime newSyncUtc) {
            var sm = new MpStreamMessage(
                MpSyncMesageType.DisconnectRequest,
                dep.DeviceGuid,
                toGuid,
                newSyncUtc.ToString());
            return sm;
        }

        //public static MpStreamMessage CreateErrorMessage(MpDeviceEndpoint dep, string toGuid, MpSyncMesageType errorType, string msg) {
        //    var sm = new MpStreamMessage(
        //        errorType,
        //        dep.DeviceGuid,
        //        toGuid,
        //        msg);
        //    return sm;
        //}

        #endregion

        public static MpStreamMessage Parse(string streamMessageStr) {
            var msgParts = streamMessageStr.Split(new string[] { MpStreamMessage.HeaderContentParseToken }, StringSplitOptions.RemoveEmptyEntries);

            var sm = new MpStreamMessage() {
                Header = MpStreamHeader.Parse(msgParts[0]),
                Content = msgParts.Length > 1 ? msgParts[1] : string.Empty
            };
            sm.Validate();
            return sm;
        }        

        private void Validate() {
            if(Header == null) {
                throw new Exception("Header must be non-null");
            }
            string givenCheckSum = Header.ContentCheckSum;
            string calcCheckSum = Content.CheckSum();

            if(calcCheckSum != givenCheckSum) {
                throw new Exception(string.Format(@"Checksum mismatch given: {0} calc: {1} for msg: {2}", givenCheckSum, calcCheckSum, Content));
            }
        }

        public bool IsError() {
            if(Header == null) {
                throw new Exception(@"Header must be non-null");
            }
            return (int)Header.MessageType > (int)MpSyncMesageType.ErrorBase;
        }

        public string SerializeDbObject() {
            //format: <header><content><Eof>
            return string.Format(
                @"{1}{0}{2}{0}{3}",
                HeaderContentParseToken,
                Header.SerializeDbObject(),
                Content,
                EofToken);
        }

        public Type GetDbObjectType() {
            return typeof(MpStreamMessage);
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
