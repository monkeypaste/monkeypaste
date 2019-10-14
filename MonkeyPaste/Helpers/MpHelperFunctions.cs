using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpHelperFunctions {
        private static readonly Lazy<MpHelperFunctions> lazy = new Lazy<MpHelperFunctions>(() => new MpHelperFunctions());
        public static MpHelperFunctions Instance { get { return lazy.Value; } }

        public Icon GetIconFromBitmap(Bitmap bmp) {
           IntPtr Hicon = bmp.GetHicon();
           return Icon.FromHandle(Hicon);
        }
        public string GetColorString(Color c) {
            return (int)c.A + "," + (int)c.R + "," + (int)c.G + "," + (int)c.B;
        }
        public Color GetColorFromString(string colorStr) {
            if(colorStr == null || colorStr == String.Empty) {
                colorStr = GetColorString(GetRandomColor());
            }
            int[] c = new int[colorStr.Split(',').Length];
            for(int i = 0;i < c.Length;i++) {
                c[i] = Convert.ToInt32(colorStr.Split(',')[i]);
            }
            if(c.Length == 3) {
                return Color.FromArgb(255/*c[3]*/,c[0],c[1],c[2]);
            }
            return Color.FromArgb(c[3],c[0],c[1],c[2]);
        }
        public Color GetRandomColor() {
            var random = new Random();
            return Color.FromArgb((int)(0xFF000000 + (random.Next(0xFFFFFF) & 0x7F7F7F)));
        }
        public byte[] ConvertImageToByteArray(Image img) {
            MemoryStream ms = new MemoryStream();
            img.Save(ms,ImageFormat.Png);
            return ms.ToArray();
        }
        public Image ConvertByteArrayToImage(byte[] rawBytes) {
            return Image.FromStream(new MemoryStream(rawBytes),true);
        }
        public Image GetIconImage(string path) {
            return Icon.ExtractAssociatedIcon(path).ToBitmap();
        }
        public IPAddress GetCurrentIPAddress() {
            Ping ping = new Ping();
            var replay = ping.Send(Dns.GetHostName());

            if(replay.Status == IPStatus.Success) {
                return replay.Address;
            }
            return null;
        }
        public bool CheckForInternetConnection() {
            try {
                using(var client = new WebClient())
                using(client.OpenRead("http://www.google.com/")) {
                    return true;
                }
            }
            catch(Exception e) {
                Console.WriteLine(e.ToString());
                return false;
            }
        }
        /*public string GeneratePassword() {
            var generator = new MpPasswordGenerator(minimumLengthPassword: 8,
                                      maximumLengthPassword: 12,
                                      minimumUpperCaseChars: 2,
                                      minimumSpecialChars: 2);
            return generator.Generate();
        }*/
        public string GetCPUInfo() {
            string cpuInfo = string.Empty;
            ManagementClass mc = new ManagementClass("win32_processor");
            ManagementObjectCollection moc = mc.GetInstances();

            foreach(ManagementObject mo in moc) {
                if(cpuInfo == "") {
                    //Get only the first CPU's ID
                    cpuInfo = mo.Properties["processorID"].Value.ToString();
                    break;
                }
            }
            return cpuInfo;
        }
        public List<MpSubTextToken> ContainsRegEx(string str,string regExStr,MpCopyItemType tokenType) {
            List<MpSubTextToken> tokenList = new List<MpSubTextToken>();
            MatchCollection mc = Regex.Matches(str,regExStr);
            foreach(Match m in mc) {
                int curIdx = 0;
                foreach(Group mg in m.Groups) {
                    tokenList.Add(new MpSubTextToken(mg.Value,tokenType,mg.Index,mg.Index + mg.Length,m.Groups.Count,curIdx++));
                }
            }
            return tokenList;
        }
        public List<MpSubTextToken> ContainsEmail(string str) {
            return ContainsRegEx(str,@"/[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?/g",MpCopyItemType.Email);
        }
        public List<MpSubTextToken> ContainsPhoneNumber(string str) {
            return ContainsRegEx(str,@"/^\s*(?:\+?(\d{1,3}))?([-. (]*(\d{3})[-. )]*)?((\d{3})[-. ]*(\d{2,4})(?:[-.x ]*(\d+))?)\s*$/gm",MpCopyItemType.PhoneNumber);
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
            return ContainsRegEx(str,fullAddress,MpCopyItemType.StreetAddress);
        }
        public List<MpSubTextToken> ContainsWebLink(string str) {
            return ContainsRegEx(str,@"/[(http(s)?):\/\/(www\.)?a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)/ig",MpCopyItemType.WebLink);
        }
        public bool IsValidEmail(string email) {
            try {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch {
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
    }
}
