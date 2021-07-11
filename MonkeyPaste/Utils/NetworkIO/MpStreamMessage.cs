using Acr.UserDialogs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public enum MpSyncMesageType {
        None = 0,
        HandshakeRequest,
        HandshakeResponse,
        DbLogRequest,
        DbLogResponse,
        DbObjectRequest,
        DbObjectResponse,
        FlipRequest, //swap A & B and return to RequestLog
        FlipResponse,
        DisconnectRequest,
        DisconnectResponse,
        //error types
        ErrorBase, //only used to differentiate with normal msgs
        ErrorNotConnected,
        ErrorInvalidChecksum,
        ErrorInvalidAccessToken,
        ErrorInvalidData,
        ErrorRequestDenied,
        ErrorOutOfMemory
    }

    public class MpStreamHeader : MpISyncableDbObject {
        private string _headerStr;
        public const string HeaderParseToken = @"$$##@";
        public const string FlipCheckSumPrefix = @"-";

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
            string checkSum = "",
            bool isFlip = false) {
            MessageType = msgType;
            FromGuid = fromGuid;
            ToGuid = toGuid;
            MessageDateTime = sendDateTime;
            ContentCheckSum = (isFlip ? FlipCheckSumPrefix : string.Empty) + checkSum;
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

        public string SerializeDbObject(string parseToken = HeaderParseToken) {
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

        public bool IsFlipped() {
            return ContentCheckSum.StartsWith(FlipCheckSumPrefix);
        }

        public Task<object> DeserializeDbObject(string objStr, string parseToken = "^(@!@") {
            throw new NotImplementedException();
        }
    }

    public class MpStreamMessage : MpISyncableDbObject {
        public const string HeaderContentParseToken = @"#^$*&";
        public const string EofToken = @"<EOF>";

        public MpStreamHeader Header { get; set; }

        public string Content { get; set; } = string.Empty;

        public MpStreamMessage() { }

        public MpStreamMessage(
            MpSyncMesageType msgType, 
            string fromGuid, 
            string toGuid, 
            string content,
            bool isFlip = false) {
            Header = new MpStreamHeader(
                msgType, 
                fromGuid, 
                toGuid, 
                DateTime.UtcNow,
                content.CheckSum(),
                isFlip);
            Content = content;
        }

        #region Sync Phase Message Builders
        public static MpStreamMessage CreateHandshakeRequest(MpDeviceEndpoint dep) {
            var sm = new MpStreamMessage(
                MpSyncMesageType.HandshakeRequest,
                dep.DeviceGuid,
                "unknown",
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

        public static MpStreamMessage CreateDbLogRequest(MpDeviceEndpoint dep, string toGuid, DateTime lastSyncUtc, bool flip = false) {
            var sm = new MpStreamMessage(
                MpSyncMesageType.DbLogRequest,
                dep.DeviceGuid,
                toGuid,
                lastSyncUtc.ToString(),
                flip);
            return sm;
        }

        public static MpStreamMessage CreateDbLogResponse(MpDeviceEndpoint dep, string toGuid, string logDbMessageStr, bool flip = false) {
            var sm = new MpStreamMessage(
                MpSyncMesageType.DbLogResponse,
                dep.DeviceGuid,
                toGuid,
                logDbMessageStr,
                flip);
            return sm;
        }

        public static MpStreamMessage CreateDbObjectRequest(MpDeviceEndpoint dep, string toGuid, string dboRequests, bool flip = false) {
            var sm = new MpStreamMessage(
                MpSyncMesageType.DbObjectRequest,
                dep.DeviceGuid,
                toGuid,
                dboRequests,
                flip);
            return sm;
        }

        public static MpStreamMessage CreateDbObjectResponse(MpDeviceEndpoint dep, string toGuid, string dboMessageStr, bool flip = false) {
            var sm = new MpStreamMessage(
                MpSyncMesageType.DbObjectResponse,
                dep.DeviceGuid,
                toGuid,
                dboMessageStr,
                flip);
            return sm;
        }
                
        public static MpStreamMessage CreateFlipRequest(MpDeviceEndpoint dep, string toGuid, bool flipRequest) {
            // once B has needed A data it will make a flipRequest = true
            // once A has needed B data it will make a flipRequest of false to finish sync
            var sm = new MpStreamMessage(
                flipRequest ? MpSyncMesageType.FlipRequest : MpSyncMesageType.FlipResponse,
                dep.DeviceGuid,
                toGuid,
                @"FlipRequest",
                flipRequest);
            return sm;
        }
                
        public static MpStreamMessage CreateFlipResponse(MpDeviceEndpoint dep, string toGuid, bool flipResponse) {
            //if flip is starting the response will be true if flip is complete it will be false signaling to move to disconnect
            var sm = new MpStreamMessage(
                flipResponse ? MpSyncMesageType.FlipRequest : MpSyncMesageType.FlipResponse,
                dep.DeviceGuid,
                toGuid,
                @"FlipResponse",
                flipResponse);
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

        public static MpStreamMessage CreateErrorMessage(MpDeviceEndpoint dep, string toGuid, MpSyncMesageType errorType, string msg) {
            var sm = new MpStreamMessage(
                errorType,
                dep.DeviceGuid,
                toGuid,
                msg);
            return sm;
        }

        #endregion

        public static MpStreamMessage Parse(string streamMessageStr) {
            var msgParts = streamMessageStr.Split(new string[] { MpStreamMessage.HeaderContentParseToken }, StringSplitOptions.RemoveEmptyEntries);            
            var sm = new MpStreamMessage() { 
                Header = MpStreamHeader.Parse(msgParts[0]),
                Content = msgParts[1]
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
            if(Header.IsFlipped()) {
                calcCheckSum = MpStreamHeader.FlipCheckSumPrefix + calcCheckSum;
            }
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

        public string SerializeDbObject(string parseToken=HeaderContentParseToken) {
            //format: <header><content><Eof>
            return string.Format(
                @"{1}{0}{2}{0}{3}",
                parseToken,
                Header.SerializeDbObject(),
                Content,
                EofToken);
        }

        public Type GetDbObjectType() {
            return typeof(MpStreamMessage);
        }

        public Task<object> DeserializeDbObject(string objStr, string parseToken = "^(@!@") {
            throw new NotImplementedException();
        }
    }
}
