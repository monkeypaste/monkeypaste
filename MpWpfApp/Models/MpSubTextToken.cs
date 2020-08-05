
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MpWpfApp {
    public enum MpSubTextTokenType {
        Uri = 1,
        Email,
        PhoneNumber,
        Currency,
        HexColor,
        StreetAddress
    }
    public class MpSubTextToken : MpDbObject {
        public int SubTextTokenId { get; set; }
        public int CopyItemId { get; set; }
        public string TokenText { get; set; }
        public MpSubTextTokenType TokenType { get; set; }
        public int StartIdx { get; set; }
        public int EndIdx { get; set; }
        public int BlockIdx { get; set; }
        public int InlineIdx { get; set; }

        public MpSubTextToken(string token, MpSubTextTokenType tokenType, int s, int e, int b,int i) {
            this.TokenText = token;
            this.TokenType = tokenType;
            this.StartIdx = s;
            this.EndIdx = e;
            this.BlockIdx = b;
            this.InlineIdx = i;
        }
        public MpSubTextToken(int subTextTokenId) {
            DataTable dt = MpDb.Instance.Execute("select * from MpSubTextToken where pk_MpSubTextTokenId=" + subTextTokenId);
            if (dt != null && dt.Rows.Count > 0) {
                LoadDataRow(dt.Rows[0]);
            }
        }
        public MpSubTextToken(DataRow dr) {
            LoadDataRow(dr);
        }
        public static List<MpSubTextToken> GatherTokens(string searchText) {
            List<MpSubTextToken> tokenList = new List<MpSubTextToken>();
            RichTextBox rtb = new RichTextBox();
            rtb.SetRtf(searchText);
            tokenList?.AddRange(ContainsEmail(rtb.Document));
            tokenList?.AddRange(ContainsWebLink(rtb.Document));
            tokenList?.AddRange(ContainsPhoneNumber(rtb.Document));
            tokenList?.AddRange(ContainsCurrency(rtb.Document));
            tokenList?.AddRange(ContainsHexColor(rtb.Document));

            //ensure no weblinks are part of emails
            List<MpSubTextToken> tokensToRemove = new List<MpSubTextToken>();
            foreach(MpSubTextToken token in tokenList) {
                if (token.TokenType == MpSubTextTokenType.Uri) {
                    var emailTokenList = tokenList.Where(stt => stt.TokenType == MpSubTextTokenType.Email).ToList();
                    //check if this weblink is within email token's range
                    foreach(var emailToken in emailTokenList) {
                        if(token.StartIdx >= emailToken.StartIdx && token.StartIdx <= emailToken.EndIdx) {
                            tokensToRemove.Add(token);
                        }
                    }
                }
            }
            foreach(MpSubTextToken tokenToRemove in tokensToRemove) {
                tokenList.Remove(tokenToRemove);
            }

            return tokenList;
        }       
        private static List<MpSubTextToken> ContainsEmail(FlowDocument doc) {
            return ContainsRegEx(doc, @"([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})", MpSubTextTokenType.Email);
        }
        private static List<MpSubTextToken> ContainsPhoneNumber(FlowDocument doc) {
            return ContainsRegEx(doc, @"(?:\([2-9]\d{2}\)\ ?|[2-9]\d{2}(?:\-?|\ ?))[2-9]\d{2}[- ]?\d{4}", MpSubTextTokenType.PhoneNumber);
        }
        private static List<MpSubTextToken> ContainsStreetAddress(FlowDocument doc) {
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
            return ContainsRegEx(doc, fullAddress, MpSubTextTokenType.StreetAddress);
        }
        private static List<MpSubTextToken> ContainsWebLink(FlowDocument doc) {
            return ContainsRegEx(doc, @"\b(?:https?://|www\.)\S+\b", MpSubTextTokenType.Uri);
        }
        private static List<MpSubTextToken> ContainsCurrency(FlowDocument doc) {
            return ContainsRegEx(doc, @"\$?([0-9]{1,3},([0-9]{3},)*[0-9]{3}|[0-9]+)(\.[0-9][0-9])?$", MpSubTextTokenType.Currency);
        }
        private static List<MpSubTextToken> ContainsHexColor(FlowDocument doc) {
            return ContainsRegEx(doc, @"#(([\da-fA-F]{3}){1,2}|([\da-fA-F]{4}){1,2})$", MpSubTextTokenType.Currency);
        }
        //        tkefauver@gmail.com www.google.com
        //804-459-9980
        private static List<MpSubTextToken> ContainsRegEx(FlowDocument doc, string regExStr, MpSubTextTokenType tokenType) {
            List<MpSubTextToken> tokenList = new List<MpSubTextToken>();
            //break document into blocks and then blocks into lines and regex lines
            for (int i = 0;i < doc.Blocks.Count;i++) {
                // TODO Maybe account for Paragraph and Table (more?) here...
                Block block = doc.Blocks.ToArray()[i];
                TextRange textRange = new TextRange(block.ContentStart, block.ContentEnd);
                MatchCollection mc = Regex.Matches(textRange.Text, regExStr, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Multiline);
                foreach (Match m in mc) {
                    foreach (Group mg in m.Groups) {
                        foreach (Capture c in mg.Captures) {
                            int sIdx = textRange.Text.IndexOf(mg.Value);
                            tokenList.Add(new MpSubTextToken(mg.Value, tokenType, sIdx, sIdx + c.Value.Length, i, 0));
                        }
                    }
                }
                //if(block.GetType() == typeof(Table)) {
                //    Table table = (Table)block;
                //    foreach(var r in table.RowGroups) {
                //        foreach(var c in table.Columns) {
                //            table.
                //        }
                //    }
                //}


                //for(int j = 0;j < block.Inlines.Count;j++) {
                //    Inline inline = block.Inlines.ToArray()[j];
                //    TextRange textRange = new TextRange(inline.ContentStart, inline.ContentEnd);
                //    MatchCollection mc = Regex.Matches(textRange.Text, regExStr, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Multiline);
                //    foreach (Match m in mc) {
                //        foreach (Group mg in m.Groups) {
                //            foreach(Capture c in mg.Captures) {
                //                int sIdx = textRange.Text.IndexOf(mg.Value);
                //                tokenList.Add(new MpSubTextToken(mg.Value, tokenType, sIdx, sIdx+c.Value.Length, i, j));
                //            }
                //        }
                //    }
                //}                
            }                       
            return tokenList;
        }
        public override void LoadDataRow(DataRow dr) {
            this.SubTextTokenId = Convert.ToInt32(dr["pk_MpSubTextTokenId"].ToString());
            this.CopyItemId = Convert.ToInt32(dr["fk_MpCopyItemId"].ToString());
            this.TokenType = (MpSubTextTokenType)Convert.ToInt32(dr["fk_MpSubTextTokenTypeId"].ToString());
            this.StartIdx = Convert.ToInt32(dr["StartIdx"].ToString());
            this.EndIdx = Convert.ToInt32(dr["EndIdx"].ToString());
            this.BlockIdx = Convert.ToInt32(dr["BlockIdx"].ToString());
            this.InlineIdx = Convert.ToInt32(dr["InlineIdx"].ToString());
            this.TokenText = dr["TokenText"].ToString();
        }

        public override void WriteToDatabase() {
            //if new
            if (SubTextTokenId == 0) {
                MpDb.Instance.ExecuteNonQuery("insert into MpSubTextToken(fk_MpCopyItemId,fk_MpSubTextTokenTypeId,StartIdx,EndIdx,BlockIdx,InlineIdx,TokenText) values(" + CopyItemId + "," + (int)TokenType + "," + StartIdx + "," + EndIdx + "," + BlockIdx + "," + InlineIdx + ",'"+TokenText+"')");
                SubTextTokenId = MpDb.Instance.GetLastRowId("MpSubTextToken", "pk_MpSubTextTokenId");
            } else {
                MpDb.Instance.ExecuteNonQuery("update MpSubTextToken set fk_MpCopyItemId=" + CopyItemId + ", fk_MpSubTextTokenTypeId=" + (int)TokenType + ", StartIdx=" + StartIdx + ", EndIdx=" + EndIdx + ",BlockIdx=" + BlockIdx + ",InlineIdx=" + InlineIdx + ",TokenText='"+TokenText+"' where pk_MpSubTextTokenId=" + SubTextTokenId);
            }
        }
        public void DeleteFromDatabase() {
            if (SubTextTokenId <= 0) {
                return;
            }
            MpDb.Instance.ExecuteNonQuery("delete from MpSubTextToken where pk_MpSubTextTokenId=" + SubTextTokenId);
        }
        private void MapDataToColumns() {
            TableName = "MpSubTextToken";
            columnData.Clear();
            columnData.Add("pk_MpSubTextTokenId", this.SubTextTokenId);
            columnData.Add("fk_MpCopyItemId", this.CopyItemId);
            columnData.Add("fk_MpSubTextTokenTypeId", (int)this.TokenType);
            columnData.Add("StartIdx", this.StartIdx);
            columnData.Add("EndIdx", this.EndIdx);
            columnData.Add("BlockIdx", this.BlockIdx);
            columnData.Add("TokenText", this.TokenText);
        }
    }
}
