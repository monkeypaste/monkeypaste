using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpSubTextToken : MpDbObject {
        public string Token { get; set; }
        public MpCopyItemType TokenType { get; set; }
        public int StartIdx { get; set; }
        public int EndIdx { get; set; }
        public int InstanceCount { get; set; }
        public int InstanceId { get; set; }

        public MpSubTextToken(string token, MpCopyItemType mpType, int s, int e, int ic, int iid) {
            this.Token = token;
            this.TokenType = mpType;
            this.StartIdx = s;
            this.EndIdx = e;
            this.InstanceCount = ic;
            this.InstanceId = iid;
        }
        public List<MpSubTextToken> ContainsRegEx(string str, string regExStr, MpCopyItemType tokenType) {
            List<MpSubTextToken> tokenList = new List<MpSubTextToken>();
            MatchCollection mc = Regex.Matches(str, regExStr);
            foreach(Match m in mc) {
                int curIdx = 0;
                foreach(Group mg in m.Groups) {
                    tokenList.Add(new MpSubTextToken(mg.Value, tokenType, mg.Index, mg.Index + mg.Length, m.Groups.Count, curIdx++));
                }
            }
            return tokenList;
        }
        public List<MpSubTextToken> ContainsEmail(string str) {
            return ContainsRegEx(str, @"/[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?/g", MpCopyItemType.Email);
        }
        public List<MpSubTextToken> ContainsPhoneNumber(string str) {
            return ContainsRegEx(str, @"/^\s*(?:\+?(\d{1,3}))?([-. (]*(\d{3})[-. )]*)?((\d{3})[-. ]*(\d{2,4})(?:[-.x ]*(\d+))?)\s*$/gm", MpCopyItemType.PhoneNumber);
        }
        public List<MpSubTextToken> ContainsStreetAddress(string str) {
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
            string fullAddress = street + cityStateZip;
            return ContainsRegEx(str, fullAddress, MpCopyItemType.StreetAddress);
        }
        public List<MpSubTextToken> ContainsWebLink(string str) {
            return ContainsRegEx(str, @"/[(http(s)?):\/\/(www\.)?a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)/ig", MpCopyItemType.WebLink);
        }
        public bool IsValidEmail(string email) {
            try {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            } catch {
                return false;
            }
        }
        public bool IsValidPassword(string password) {
            //test password here
            //rule 1: between 8-12 characters
            if(password == null) {
                return false;
            }
            return password.Length >= 8 && password.Length <= 12;
        }

        public override void LoadDataRow(DataRow dr) {
            throw new NotImplementedException();
        }

        public override void WriteToDatabase() {
            throw new NotImplementedException();
        }
    }
}
