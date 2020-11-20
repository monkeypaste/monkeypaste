using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MpWpfApp {
    public enum MpCurrencyType {
        None = 0,
        Dollars,
        Pounds,
        Euros,
        Yen
    }

    public enum MpSubTextTokenType {
        Uri = 1,
        Email,
        PhoneNumber,
        Currency,
        HexColor,
        StreetAddress,
        CopyItemSegment
    }
    public class MpSubTextToken : MpDbObject {
        public MpSubTextToken ParentToken { get; set; }
        public MpSubTextToken FirstChildToken { get; set; }
        public MpSubTextToken NextToken { get; set; }
        public MpSubTextToken PrevToken { get; set; }

        public int SubTextTokenId { get; set; }
        public int CopyItemId { get; set; }
        public string TokenText { get; set; }
        public MpSubTextTokenType TokenType { get; set; }
        public int StartIdx { get; set; }
        public int EndIdx { get; set; }
        public int BlockIdx { get; set; }
        public int InlineIdx { get; set; }

        public MpSubTextToken(
            string token, 
            MpSubTextTokenType tokenType, 
            int startIdx, 
            int endIdx, 
            int blockIdx, 
            int inlineIdx) {
            this.TokenText = token;
            this.TokenType = tokenType;
            this.StartIdx = startIdx;
            this.EndIdx = endIdx;
            this.BlockIdx = blockIdx;
            this.InlineIdx = inlineIdx;
            MapDataToColumns();
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
        public static List<MpSubTextToken> GatherTokens(FlowDocument fd) {
            List<MpSubTextToken> tokenList = new List<MpSubTextToken>();
            //RichTextBox rtb = new RichTextBox();
            //rtb.SetRtf(richText);

            tokenList?.AddRange(ExtractSegment(fd));
            tokenList?.AddRange(ExtractEmail(fd));
            tokenList?.AddRange(ExtractWebLink(fd));
            tokenList?.AddRange(ExtractStreetAddress(fd));
            tokenList?.AddRange(ExtractPhoneNumber(fd));
            tokenList?.AddRange(ExtractCurrency(fd));
            tokenList?.AddRange(ExtractHexColor(fd));

            //ensure no weblinks are part of emails
            //List<MpSubTextToken> tokensToRemove = new List<MpSubTextToken>();
            //foreach (MpSubTextToken token in tokenList) {
            //    if (token.TokenType == MpSubTextTokenType.Uri) {
            //        var emailTokenList = tokenList.Where(stt => stt.TokenType == MpSubTextTokenType.Email).ToList();
            //        //check if this weblink is within email token's range
            //        foreach (var emailToken in emailTokenList) {
            //            if (token.StartIdx >= emailToken.StartIdx && token.StartIdx <= emailToken.EndIdx && token != emailToken) {
            //                tokensToRemove.Add(token);
            //            }
            //        }
            //    }
            //}
            //foreach (MpSubTextToken tokenToRemove in tokensToRemove) {
            //    tokenList.Remove(tokenToRemove);
            //}

            return tokenList;
        }
        // created from https://www.regexr.com
        private static List<MpSubTextToken> ExtractEmail(FlowDocument doc) {
            return ExtractRegEx(doc, @"([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})", MpSubTextTokenType.Email);
        }

        private static List<MpSubTextToken> ExtractPhoneNumber(FlowDocument doc) {
            return ExtractRegEx(doc, @"(\+?\d{1,3}?[ -.]?)?\(?(\d{3})\)?[ -.]?(\d{3})[ -.]?(\d{4})", MpSubTextTokenType.PhoneNumber);
        }

        private static List<MpSubTextToken> ExtractStreetAddress(FlowDocument doc) {
            return ExtractRegEx(doc, @"\d+[ ](?:[A-Za-z0-9.-]+[ ]?)+(?:Avenue|Lane|Road|Boulevard|Drive|Street|Ave|Dr|Rd|Blvd|Ln|St)\.?,\s(?:[A-Z][a-z.-]+[ ]?)+ \b\d{5}(?:-\d{4})?\b", MpSubTextTokenType.StreetAddress);
        }

        private static List<MpSubTextToken> ExtractWebLink(FlowDocument doc) {
            return ExtractRegEx(doc, @"(?:https?://|www\.)\S+", MpSubTextTokenType.Uri);
        }

        private static List<MpSubTextToken> ExtractCurrency(FlowDocument doc) {
            return ExtractRegEx(doc, @"[$|£|€|¥]([0-9]{1,3},([0-9]{3},)*[0-9]{3}|[0-9]+)?(\.[0-9][0-9])?", MpSubTextTokenType.Currency);
        }

        private static List<MpSubTextToken> ExtractHexColor(FlowDocument doc) {
            return ExtractRegEx(doc, @"#([0-9]|[a-fA-F]){6}", MpSubTextTokenType.HexColor);
        }
        private static List<MpSubTextToken> ExtractSegment(FlowDocument doc) {
            return new List<MpSubTextToken>() { new MpSubTextToken(
                new TextRange(doc.ContentStart, doc.ContentEnd).Text,
                MpSubTextTokenType.CopyItemSegment,
                0,  //represents first block
                doc.Blocks.Count,
                -1, // flag showing startIdx/endIdx represents blocks not pointer idx
                -1)
            };
            //return ExtractRegEx(doc, @"^" + new TextRange(doc.ContentStart,doc.ContentEnd).Text + "$", MpSubTextTokenType.CopyItemSegment);
        }

        private static List<MpSubTextToken> ExtractRegEx(FlowDocument doc, string regExStr, MpSubTextTokenType tokenType) {
            List<MpSubTextToken> tokenList = new List<MpSubTextToken>();
            //break document into blocks and then blocks into lines and regex lines
            for (int bIdx = 0; bIdx < doc.Blocks.Count; bIdx++) {
                // TODO Maybe account for Paragraph and Table (more?) here...
                Block block = doc.Blocks.ToArray()[bIdx];
                TextRange textRange = new TextRange(block.ContentStart, block.ContentEnd);
                MatchCollection mc = Regex.Matches(textRange.Text, regExStr, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Multiline);
                foreach (Match m in mc) {
                    foreach (Group mg in m.Groups) {
                        foreach (Capture c in mg.Captures) {
                            int sIdx = textRange.Text.IndexOf(mg.Value);
                            tokenList.Add(new MpSubTextToken(mg.Value, tokenType, sIdx, sIdx + c.Value.Length, bIdx, 0));
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

            MapDataToColumns();
        }

        public override void WriteToDatabase() {
            //if new
            if (SubTextTokenId == 0) {
                MpDb.Instance.ExecuteNonQuery("insert into MpSubTextToken(fk_MpCopyItemId,fk_MpSubTextTokenTypeId,StartIdx,EndIdx,BlockIdx,InlineIdx,TokenText) values(" + CopyItemId + "," + (int)TokenType + "," + StartIdx + "," + EndIdx + "," + BlockIdx + "," + InlineIdx + ",'" + TokenText + "')");
                SubTextTokenId = MpDb.Instance.GetLastRowId("MpSubTextToken", "pk_MpSubTextTokenId");
            } else {
                MpDb.Instance.ExecuteNonQuery("update MpSubTextToken set fk_MpCopyItemId=" + CopyItemId + ", fk_MpSubTextTokenTypeId=" + (int)TokenType + ", StartIdx=" + StartIdx + ", EndIdx=" + EndIdx + ",BlockIdx=" + BlockIdx + ",InlineIdx=" + InlineIdx + ",TokenText='" + TokenText + "' where pk_MpSubTextTokenId=" + SubTextTokenId);
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
