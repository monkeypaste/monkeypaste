using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpSubTextToken : MpDbObject {
        public int SubTextTokenId { get; set; }
        public int CopyItemId { get; set; }
        public string Token { get; set; }
        public MpCopyItemType TokenType { get; set; }
        public int StartIdx { get; set; }
        public int EndIdx { get; set; }
        public int InstanceCount { get; set; }
        public int InstanceIdx { get; set; }

        public MpSubTextToken(string token, MpCopyItemType mpType, int s, int e, int ic, int iid) {
            this.Token = token;
            this.TokenType = mpType;
            this.StartIdx = s;
            this.EndIdx = e;
            this.InstanceCount = ic;
            this.InstanceIdx = iid;
        }
        public MpSubTextToken(int subTextTokenId) {
            DataTable dt = MpDataStore.Instance.Db.Execute("select * from MpSubTextToken where pk_MpSubTextTokenId=" + subTextTokenId);
            if (dt != null && dt.Rows.Count > 0) {
                LoadDataRow(dt.Rows[0]);
            }
        }
        public MpSubTextToken(DataRow dr) {
            LoadDataRow(dr);
        }
        public static List<MpSubTextToken> GatherTokens(string searchText) {
            List<MpSubTextToken> tokenList = new List<MpSubTextToken>();

            tokenList?.AddRange(ContainsEmail(searchText));
            tokenList?.AddRange(ContainsPhoneNumber(searchText));
            tokenList?.AddRange(ContainsWebLink(searchText));
            //tokenList?.AddRange(ContainsStreetAddress(searchText));
            return tokenList;
        }

        public override void LoadDataRow(DataRow dr) {
            this.SubTextTokenId = Convert.ToInt32(dr["pk_MpSubTextTokenId"].ToString());
            this.CopyItemId = Convert.ToInt32(dr["fk_MpCopyItemId"].ToString());
            this.TokenType = (MpCopyItemType)Convert.ToInt32(dr["fk_MpCopyItemTypeId"].ToString());
            this.StartIdx = Convert.ToInt32(dr["StartIdx"].ToString());
            this.EndIdx = Convert.ToInt32(dr["EndIdx"].ToString());
            this.InstanceIdx = Convert.ToInt32(dr["InstanceIdx"].ToString());
        }

        public override void WriteToDatabase() {
            //if new
            if(SubTextTokenId == 0) {
                MpDataStore.Instance.Db.ExecuteNonQuery("insert into MpSubTextToken(fk_MpCopyItemId,fk_MpCopyItemTypeId,StartIdx,EndIdx,InstanceIdx) values(" + CopyItemId + "," + (int)TokenType + "," + StartIdx + "," + EndIdx + ",1)");
                SubTextTokenId = MpDataStore.Instance.Db.GetLastRowId("MpSubTextToken", "pk_MpSubTextTokenId");
            } else {
                MpDataStore.Instance.Db.ExecuteNonQuery("update MpSubTextToken set fk_MpCopyItemId=" + CopyItemId + ", fk_MpCopyItemTypeId=" + (int)TokenType + ", StartIdx=" + StartIdx + ", EndIdx=" + EndIdx + ",InstanceIdx=1 where pk_MpSubTextTokenId=" + SubTextTokenId);
            }
        }
        public void DeleteFromDatabase() {
            if(SubTextTokenId <= 0) {
                return;
            }
            MpDataStore.Instance.Db.ExecuteNonQuery("delete from MpSubTextToken where pk_MpSubTextTokenId=" + SubTextTokenId);
        }
        private void MapDataToColumns() {
            TableName = "MpSubTextToken";
            columnData.Clear();
            columnData.Add("pk_MpSubTextTokenId", this.SubTextTokenId);
            columnData.Add("fk_MpCopyItemId", this.CopyItemId);
            columnData.Add("fk_MpCopyItemTypeId", (int)this.TokenType);
            columnData.Add("StartIdx", this.StartIdx);
            columnData.Add("EndIdx", this.EndIdx);
            columnData.Add("InstanceIdx", this.InstanceIdx);
        }

        private static List<MpSubTextToken> ContainsEmail(string str) {
            return ContainsRegEx(str, @"([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})", MpCopyItemType.Email);
        }
        private static List<MpSubTextToken> ContainsPhoneNumber(string str) {
            return ContainsRegEx(str, @"(?:\([2-9]\d{2}\)\ ?|[2-9]\d{2}(?:\-?|\ ?))[2-9]\d{2}[- ]?\d{4}$", MpCopyItemType.PhoneNumber);
        }
        private static List<MpSubTextToken> ContainsStreetAddress(string str) {
            string zip = @"\b\d{5}(?:-\d{4})?\b";
            string city = @"(?:[A-Z][a-z.-]+[ ]?)+";
            string state = @"Alabama|Alaska|Arizona|Arkansas|California|Colorado|Connecticut|Delaware|Florida|Georgia|Hawaii|
                            Idaho|Illinois|Indiana|Iowa|Kansas|Kentucky|Louisiana|Maine|Maryland|Massachusetts|Michigan|
                            Minnesota|Mississippi|Missouri|Montana|Nebraska|Nevada|New[ ]Hampshire|New[ ]Jersey|New[ ]Mexico
                            |New[ ]York|North[ ]Carolina|North[ ]Dakota|Ohio|Oklahoma|Oregon|Pennsylvania|Rhode[ ]Island
                            |South[ ]Carolina|South[ ]Dakota|Tennessee|Texas|Utah|Vermont|Virginia|Washington|West[ ]Virginia
                            |Wisconsin|Wyoming";
            string stateAbbr = @"AL|AK|AS|AZ|AR|CA|CO|CT|DE|DC|FM|FL|GA|GU|HI|ID|IL|IN|IA|KS|KY|LA|ME|MH|MD|MA|MI|MN|MS|MO|MT|NE|NV|NH|NJ|NM|NY|NC|ND|MP|OH|OK|OR|PW|PA|PR|RI|SC|SD|TN|TX|UT|VT|VI|VA|WA|WV|WI|WY";
            string cityStateZip = @"{" + city + "},[ ](?:{" + state + "}|{" + stateAbbr + "})[ ]{" + zip + "}";
            string street = @"\d+[ ](?:[A-Za-z0-9.-]+[ ]?)+(?:Avenue|Court|Loop|Pike|Turnpike|Square|Station|Trail|Terrace|Lane|Parkway|Road|Way|Circle|Boulevard|Drive|Street|Ave|Trnpk|Dr|Trl|Wy|Ter|Sq||Pkwy|Rd|Cir|Blvd|Ln|Ct|St)\.?";
            string fullAddress = street + @"\s"+ cityStateZip;
            return ContainsRegEx(str, fullAddress, MpCopyItemType.StreetAddress);
        }
        private static List<MpSubTextToken> ContainsWebLink(string str) {
            return ContainsRegEx(str, @"([a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,6}$", MpCopyItemType.WebLink);
        }

        private static List<MpSubTextToken> ContainsRegEx(string str, string regExStr, MpCopyItemType tokenType) {
            List<MpSubTextToken> tokenList = new List<MpSubTextToken>();
            //break string into lines 
            foreach(string s in Regex.Split(str, "\r\n|\r|\n")) {
                MatchCollection mc = Regex.Matches(s, regExStr, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Multiline);
                foreach (Match m in mc) {
                    int curIdx = 0;
                    foreach (Group mg in m.Groups) {
                        int actualIdx = str.IndexOf(mg.Value);
                        tokenList.Add(new MpSubTextToken(mg.Value, tokenType, actualIdx, actualIdx + mg.Length, m.Groups.Count, curIdx++));
                    }
                }
            }            
            return tokenList;
        }

    }
}
