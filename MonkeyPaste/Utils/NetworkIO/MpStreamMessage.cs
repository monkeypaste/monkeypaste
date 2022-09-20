using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using MonkeyPaste.Common;

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

    public class MpStreamMessage : MpISyncableDbObject {
        public const string HeaderContentParseToken = @"#^$*@";
        public const string EofToken = @"<EOF>";

        public MpStreamHeader Header { get; set; }

        public string Content { get; set; } = string.Empty;

        public static async Task<MpStreamMessage> Create(string streamMsgStr) {
            var msgParts = streamMsgStr.Split(new string[] { MpStreamMessage.HeaderContentParseToken }, StringSplitOptions.RemoveEmptyEntries);

            var header = await MpStreamHeader.Parse(msgParts[0]);
            string content = msgParts.Length > 1 ? msgParts[1] : string.Empty;
            return new MpStreamMessage() {
                Header = header,
                Content = content
            };
        }

        public static async Task<MpStreamMessage> Create(
            MpSyncMesageType msgType,
            string fromGuid,
            string toGuid,
            string content) {

            string checkSum = await content.CheckSumAsync();
            return new MpStreamMessage() {
                Header = new MpStreamHeader(
                    msgType,
                    fromGuid,
                    toGuid,
                    DateTime.UtcNow,
                    checkSum),
                Content = content
            };
        }

        private MpStreamMessage() { }

        #region Sync Phase Message Builders
        public static async Task<MpStreamMessage> CreateWebDeviceRequest(MpDeviceEndpoint dep) {
            string depStr = await dep.SerializeDbObject();
            var sm = await MpStreamMessage.Create(
                MpSyncMesageType.WebDeviceRequest,
                dep.DeviceGuid,
                "hub",
                depStr);
            return sm;
        }

        public static async Task<MpStreamMessage> CreateHandshakeRequest(MpDeviceEndpoint dep) {
            string depStr = await dep.SerializeDbObject();
            var sm = await MpStreamMessage.Create(
                MpSyncMesageType.HandshakeRequest,
                dep.DeviceGuid,
                "hub",
                depStr);
            return sm;
        }

        public static async Task<MpStreamMessage> CreateHandshakeResponse(MpDeviceEndpoint dep, string toGuid) {
            string depStr = await dep.SerializeDbObject();
            var sm = await MpStreamMessage.Create(
                MpSyncMesageType.HandshakeResponse,
                dep.DeviceGuid,
                toGuid,
                depStr);
            return sm;
        }

        public static async Task<MpStreamMessage> CreateDbLogRequest(MpDeviceEndpoint dep, string toGuid, DateTime lastSyncUtc) {
            string depStr = await dep.SerializeDbObject();
            var sm = await MpStreamMessage.Create(
                MpSyncMesageType.DbLogRequest,
                dep.DeviceGuid,
                toGuid,
                lastSyncUtc.ToString());
            return sm;
        }

        public static async Task<MpStreamMessage> CreateDbLogResponse(MpDeviceEndpoint dep, string toGuid, string logDbMessageStr) {
            string depStr = await dep.SerializeDbObject();
            var sm = await MpStreamMessage.Create(
                MpSyncMesageType.DbLogResponse,
                dep.DeviceGuid,
                toGuid,
                logDbMessageStr);
            return sm;
        }

        public static async Task<MpStreamMessage> CreateFlipRequest(MpDeviceEndpoint dep, string toGuid) {
            string depStr = await dep.SerializeDbObject();
            // once B has needed A data it will make a flipRequest = true
            // once A has needed B data it will make a flipRequest of false to finish sync
            var sm = await MpStreamMessage.Create(
                MpSyncMesageType.FlipRequest,
                dep.DeviceGuid,
                toGuid,
                @"FlipRequest");
            return sm;
        }

        public static async Task<MpStreamMessage> CreateDisconnectRequest(MpDeviceEndpoint dep, string toGuid, DateTime newSyncUtc) {
            string depStr = await dep.SerializeDbObject();
            var sm = await MpStreamMessage.Create(
                MpSyncMesageType.DisconnectRequest,
                dep.DeviceGuid,
                toGuid,
                newSyncUtc.ToString());
            return sm;
        }

        public static async Task<MpStreamMessage> CreateDisconnectResponse(MpDeviceEndpoint dep, string toGuid, DateTime newSyncUtc) {
            string depStr = await dep.SerializeDbObject();
            var sm = await MpStreamMessage.Create(
                MpSyncMesageType.DisconnectRequest,
                dep.DeviceGuid,
                toGuid,
                newSyncUtc.ToString());
            return sm;
        }

//public static async Task<MpStreamMessage> CreateErrorMessage(MpDeviceEndpoint dep, string toGuid, MpSyncMesageType errorType, string msg) {
        //string depStr = await dep.SerializeDbObject();
        //    var sm = await MpStreamMessage.Create(
        //        errorType,
        //        dep.DeviceGuid,
        //        toGuid,
        //        msg);
        //    return sm;
        //}

        #endregion

        public static async Task<MpStreamMessage> Parse(string streamMessageStr) {
            var sm = await MpStreamMessage.Create(streamMessageStr);
            await sm.Validate();
            return sm;
        }

        private async Task Validate() {
            if (Header == null) {
                throw new Exception("Header must be non-null");
            }
            string givenCheckSum = Header.ContentCheckSum;
            string calcCheckSum = await Content.CheckSumAsync();

            if (calcCheckSum != givenCheckSum) {
                throw new Exception(string.Format(@"Checksum mismatch given: {0} calc: {1} for msg: {2}", givenCheckSum, calcCheckSum, Content));
            }
        }

        public bool IsError() {
            if (Header == null) {
                throw new Exception(@"Header must be non-null");
            }
            return (int)Header.MessageType > (int)MpSyncMesageType.ErrorBase;
        }

        public async Task<string> SerializeDbObject() {
            await Task.Delay(1);

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

        public Task<Dictionary<string, string>> DbDiff(object drOrModel) {
            throw new NotImplementedException();
        }

        public Task<object> CreateFromLogs(string dboGuid, List<MpDbLog> logs, string fromClientGuid) {
            throw new NotImplementedException();
        }

    }
}
