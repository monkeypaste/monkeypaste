using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpSyncDbObjectRequestMessage : MpISyncableDbObject {
        public string TableName { get; set; }
        public string DbObjectGuid { get; set; }
        //when all columns needed (besides pk) will only have *
        public List<string> ColumnNames { get; set; } = new List<string>();

        public MpSyncDbObjectRequestMessage() { }

        public MpSyncDbObjectRequestMessage(string tn, string dboguid,List<string> cn) {
            TableName = tn;
            DbObjectGuid = dboguid;
            ColumnNames = cn;
        }
        public Dictionary<string, string> DbDiff(object drOrModel) {
            throw new NotImplementedException();
        }

        public async Task<object> DeserializeDbObject(string objStr, string parseToken = "^(@!@") {
            var objParts = objStr.Split(new string[] { parseToken }, StringSplitOptions.RemoveEmptyEntries);
            await Task.Delay(0);
            
            var dbLog = new MpSyncDbObjectRequestMessage() {
                TableName = objParts[0],
                DbObjectGuid = objParts[1],
                ColumnNames = new List<string>(objParts[2].Split(new string[] {","},StringSplitOptions.RemoveEmptyEntries))
            };
            return dbLog;
        }

        public Type GetDbObjectType() {
            return typeof(MpSyncDbObjectRequestMessage);
        }

        public string SerializeDbObject(string parseToken = "^(@!@") {
            var sb = new StringBuilder();
            foreach(var cn in ColumnNames) {
                sb.Append(string.Format(@"{0},", cn));
            }
            return string.Format(
                @"{0}{1}{0}{2}{0}{3}{0}",
                parseToken,
                TableName,
                DbObjectGuid,
                sb.ToString());
        }
    }
}
