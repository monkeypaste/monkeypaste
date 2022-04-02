using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public class MpCopyItemOperation : MpDbModelBase {
        #region Column

        [Column("pk_MpCopyItemOperationId")]
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        [Column("MpCopyItemOperationGuid")]        
        [JsonProperty("opGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [JsonProperty("sourceGuid")]
        public string SourceGuid { get; set; }

        [JsonProperty("targetGuid")]
        public string TargetGuid { get; set; }

        [JsonProperty("operation")]
        public string Operation { get; set; }

        #endregion
    }
}
