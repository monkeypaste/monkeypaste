using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpHttpRequest : MpDbModelBase {
        #region Columns
        [Column("pk_MpHttpRequestId")]
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        [Column("MpHttpRequestGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Column("fk_MpAnalyticItemId")]
        [ForeignKey(typeof(MpAnalyticItem))]
        public int AnalyticItemId { get; set; }

        [Column("e_MpEncodingTypeId")]
        public int EncodingTypeId { get; set; } = 0;

        [Column("e_MpHttpRequestTypeId")]
        public int HttpRequestTypeId { get; set; } = 0;

        public string ContentType { get; set; }

        public string EndPoint { get; set; }

        public string EndPointArg0 { get; set; }

        public string EndPointArg1 { get; set; }

        public string EndPointArg2 { get; set; }

        public string EndPointArg3 { get; set; }

        public string ApiKey { get; set; } = string.Empty;

        #endregion

        #region Properties
        [Ignore]
        public MpEncodingType EncodingType {
            get {
                return (MpEncodingType)EncodingTypeId;
            }
            set {
                EncodingTypeId = (int)value;
            }
        }

        [Ignore]
        public MpHttpRequestType HttpRequestType {
            get {
                return (MpHttpRequestType)HttpRequestTypeId;
            }
            set {
                HttpRequestTypeId = (int)value;
            }
        }

        [Ignore]
        public Guid HttpRequestGuid {
            get {
                return System.Guid.Parse(Guid);
            }
            set {
                Guid = value.ToString();
            }
        }
        #endregion

        public static async Task<MpHttpRequest> Create(
            List<MpHttpHeaderItem> headerItems,
            string endpoint,
            string apikey,
            string contentType = "application/json",
            MpEncodingType encodingType = MpEncodingType.Utf8,
            MpHttpRequestType requestType = MpHttpRequestType.Post,
            string arg0 = "", string arg1 = "", string arg2 = "", string arg3 = "") {

            var httpRequest = new MpHttpRequest() {
                HttpRequestGuid = System.Guid.NewGuid(),
                EndPoint = endpoint,
                ApiKey = apikey,
                ContentType = contentType,
                EncodingType = encodingType,
                HttpRequestType = requestType,
                EndPointArg0 = arg0,
                EndPointArg1 = arg1,
                EndPointArg2 = arg2,
                EndPointArg3 = arg3,
            };

            await httpRequest.WriteToDatabaseAsync();

            headerItems.ForEach(x => x.HttpRequestId = httpRequest.Id);
            await Task.WhenAll(headerItems.Select(x => x.WriteToDatabaseAsync()));

            return httpRequest;
        }
    }
}
