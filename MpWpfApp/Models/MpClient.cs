using System;
using System.Collections.Generic;
using System.Data;

namespace MpWpfApp {
    public class MpClient : MpDbObject {
        public int ClientId { get; set; }
        public int PlatformId { get; set; }
        public string Ip4Address { get; set; }
        public string AccessToken { get; set; }
        public DateTime LoginDateTime { get; set; }
        public DateTime? LogoutDateTime { get; set; }

        public MpClient(int clientId, int platformId, string ip4Address, string accessToken, DateTime loginTime) {
            ClientId = clientId;
            PlatformId = platformId;
            Ip4Address = ip4Address;
            AccessToken = accessToken;
            LoginDateTime = loginTime;
            LogoutDateTime = null;
        }
        public MpClient(int clientId) {
            DataTable dt = MpDb.Instance.Execute("select * from MpClient where MpClientId=@clientId", new Dictionary<string, object> { { "@clientId", clientId } });
            if (dt != null && dt.Rows.Count > 0) {
                LoadDataRow(dt.Rows[0]);
            }
        }
        public MpClient(DataRow dr) {
            LoadDataRow(dr);
        }
        public override void LoadDataRow(DataRow dr) {
            ClientId = Convert.ToInt32(dr["pk_MpClientId"].ToString());
            PlatformId = Convert.ToInt32(dr["fk_MpPlatformId"].ToString());
            Ip4Address = dr["Ip4Address"].ToString();
            AccessToken = dr["AccessToken"].ToString();
            LoginDateTime = DateTime.Parse(dr["LoginDateTime"].ToString());
            LogoutDateTime = DateTime.Parse(dr["LogoutDateTime"].ToString());
        }

        public override void WriteToDatabase() {
            if (string.IsNullOrEmpty(Ip4Address)) {
                Console.WriteLine("MpTag Error, cannot create nameless tag");
                return;
            }
            if (ClientId == 0) {
                MpDb.Instance.ExecuteWrite(
                    "insert into MpClient(fk_MpPlatformId,Ip4Address,AccessToken,LoginDateTime) values(@pId,@ip4,@at,@ldt)",
                    new Dictionary<string, object> {
                        { "@pId", PlatformId },
                        { "@ip4", Ip4Address },
                        { "@at", AccessToken },
                        { "@ldt", LoginDateTime.ToString("yyyy-MM-dd HH:mm:ss") }
                    });

                ClientId = MpDb.Instance.GetLastRowId("MpClient", "pk_MpClientId");
            } else {
                LogoutDateTime = DateTime.Now;
                MpDb.Instance.ExecuteWrite(
                    "update MpClient set LogoutDateTime=@lodt where pk_MpClientId=@cId",
                    new Dictionary<string, object> {
                        { "@lodt", LogoutDateTime.Value.ToString("yyyy-MM-dd HH:mm:ss") },
                        { "cId", ClientId }
                    });
            }
        }
        private void MapDataToColumns() {
            TableName = "MpClient";
            columnData.Clear();
            columnData.Add("pk_MpClientId", this.ClientId);
            columnData.Add("fk_MpPlatformId", this.PlatformId);
            columnData.Add("Ip4Address", this.Ip4Address);
            columnData.Add("AccessToken", this.AccessToken);
            columnData.Add("LoginDateTime", this.LoginDateTime);
            columnData.Add("LogoutDateTime", this.LogoutDateTime);
        }
    }
}
