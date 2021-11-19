using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using FFImageLoading.Work;
using SkiaSharp;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto.Parameters;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace MonkeyPaste {
    public class MpHelpers {
        #region Singleton
        private static readonly Lazy<MpHelpers> _Lazy = new Lazy<MpHelpers>(() => new MpHelpers());
        public static MpHelpers Instance { get { return _Lazy.Value; } }

        private MpHelpers() {
            Rand = new Random((int)DateTime.Now.Ticks);
        }
        #endregion

        public Random Rand { get; set; }

        #region Documents

        public List<int> IndexListOfAll(string text, string matchStr) {
            var idxList = new List<int>();
            int curIdx = text.IndexOf(matchStr);
            int offset = 0;
            while (curIdx >= 0 && curIdx < text.Length) {
                idxList.Add(curIdx + offset);
                if (curIdx + matchStr.Length + 1 >= text.Length) {
                    break;
                }
                text = text.Substring(curIdx + matchStr.Length);
                offset = curIdx + 1;
                curIdx = text.IndexOf(matchStr);
            }
            return idxList;
        }

        public bool IsStringQuillText(string str) {
            if (string.IsNullOrEmpty(str)) {
                return false;
            }
            str = str.ToLower();
            foreach (var quillTag in _quillTags) {
                if (str.Contains($"</{quillTag}>")) {
                    return true;
                }
            }
            return false;
        }

        public string Diff(string str1, string str2) {
            if (str1 == null) {
                return str2;
            }
            if (str2 == null) {
                return str1;
            }

            List<string> set1 = str1.Split(' ').Distinct().ToList();
            List<string> set2 = str2.Split(' ').Distinct().ToList();

            var diff = set2.Count() > set1.Count() ? set2.Except(set1).ToList() : set1.Except(set2).ToList();

            return string.Join("", diff);
        }

        public string LoadTextResource(string resourcePath) {
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(MpCopyItem)).Assembly;
            var stream = assembly.GetManifestResourceStream(resourcePath); 
            using (var reader = new System.IO.StreamReader(stream)) {
                var res = reader.ReadToEnd();
                return res;
            }
        }
        public SKBitmap LoadBitmapResource(string resourcePath) {
            // Ensure "this" is an object that is part of your implementation within your Xamarin forms project
            var assembly = this.GetType().GetTypeInfo().Assembly;
            byte[] buffer = null;

            using (System.IO.Stream s = assembly.GetManifestResourceStream(resourcePath)) {
                if (s != null) {
                    long length = s.Length;
                    buffer = new byte[length];
                    s.Read(buffer, 0, (int)length);
                }
            }

            return new MpImageConverter().Convert(buffer,typeof(SKBitmap)) as SKBitmap;
        }

        public string GetCheckSum(string theString) {
            string hash;
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create()) {
                hash = BitConverter.ToString(
                  md5.ComputeHash(Encoding.UTF8.GetBytes(theString))
                ).Replace("-", String.Empty);
            }
            return hash;
        }

        public const string AlphaNumericChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        public const string OtherChars = @"`~!@#$%^*()_-+[{]}\|;':,<./";
        private string _passwordChars = null;
        public string PasswordChars {
            get {
                if (_passwordChars == null) {
                    var sb = new StringBuilder();
                    for (int i = char.MinValue; i <= char.MaxValue; i++) {
                        char c = Convert.ToChar(i);
                        if (!char.IsControl(c)) {
                            sb.Append(c);
                        }
                    }
                    _passwordChars = sb.ToString();
                }
                return _passwordChars;
            }
        }

        public string GetRandomString(int length, string chars = AlphaNumericChars) {
            return new string(Enumerable.Repeat(chars, length).Select(s => s[Rand.Next(s.Length)]).ToArray());
        }

        public string GetNewAccessToken() {
            return GetRandomString(Rand.Next(20, 50), AlphaNumericChars);
        }

        public bool IsStringCsv(string text) {
            if (string.IsNullOrEmpty(text) || IsStringRichText(text)) {
                return false;
            }
            return text.Contains(",");
        }

        public bool IsStringRichText(string text) {
            if (string.IsNullOrEmpty(text)) {
                return false;
            }
            return text.StartsWith(@"{\rtf");
        }

        public bool IsStringXaml(string text) {
            if (string.IsNullOrEmpty(text)) {
                return false;
            }
            return text.StartsWith(@"<Section xmlns=") || text.StartsWith(@"<Span xmlns=");
        }

        public bool IsStringSpan(string text) {
            if (string.IsNullOrEmpty(text)) {
                return false;
            }
            return text.StartsWith(@"<Span xmlns=");
        }

        public bool IsStringSection(string text) {
            if (string.IsNullOrEmpty(text)) {
                return false;
            }
            return text.StartsWith(@"<Section xmlns=");
        }

        public bool IsStringPlainText(string text) {
            //returns true for csv
            if (text == null) {
                return false;
            }
            if (text == string.Empty) {
                return true;
            }
            if (IsStringRichText(text) || IsStringSection(text) || IsStringSpan(text) || IsStringXaml(text)) {
                return false;
            }
            return true;
        }

        private string[] _quillTags = new string[] {
            "p",
            "ol",
            "li",
            "#text",
            "img",
            "em",
            "span",
            "strong",
            "u",
            "br",
            "a"
        };
        #endregion

        #region System

        public int ParseEnumValue(Type enumType, string typeStr) {
            for (int i = 0; i < Enum.GetValues(enumType).Length; i++) {
                if (Enum.GetName(enumType, i).ToLower() == typeStr.ToLower()) {
                    return i;
                }
            }
            return 0;
        }

        public string AppStorageFilePath {
            get {
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
        }

        public double ConvertBytesToMegabytes(long bytes, int precision = 2) {
            return Math.Round((bytes / 1024f) / 1024f, precision);
        }

        public double ConvertMegaBytesToBytes(long megabytes, int precision = 2) {
            return Math.Round((megabytes * 1024f) * 1024f, precision);
        }

        public double GetFileSizeInBytes(string filePath) {
            try {
                if (File.Exists(filePath)) {
                    FileInfo fi = new FileInfo(filePath);
                    return fi.Length;
                }
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error checking size of path {filePath}", ex);
            }
            return -1;
        }

        public void AppendTextToFile(string path, string textToAppend) {
            try {
                if (!File.Exists(path)) {
                    // Create a file to write to.
                    using (var sw = File.CreateText(path)) {
                        sw.WriteLine(textToAppend);
                    }
                } else {
                    // This text is always added, making the file longer over time
                    // if it is not deleted.
                    using (StreamWriter sw = File.AppendText(path)) {
                        sw.WriteLine(textToAppend);
                    }
                }
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error appending text '{textToAppend}' to path '{path}'");
                MpConsole.WriteTraceLine($"With exception: {ex}");
            }
        }

        public string ReadTextFromFile(string filePath) {
            try {
                using (StreamReader f = new StreamReader(filePath)) {
                    string outStr = string.Empty;
                    outStr = f.ReadToEnd();
                    f.Close();
                    return outStr;
                }
            }
            catch (Exception ex) {
                MpConsole.WriteLine("MpHelpers.ReadTextFromFile error for filePath: " + filePath + ex.ToString());
                return null;
            }
        }

        public async Task<byte[]> ReadBytesFromUriAsync(string url) {
            if(!Uri.IsWellFormedUriString(url,UriKind.Absolute)) {
                MpConsole.WriteTraceLine(@"Cannot read bytes, bad url: " + url);
                return null;
            }
            using var httpClient = new HttpClient();
            byte[] bytes = await httpClient.GetByteArrayAsync(url);

            using var fs = new FileStream("favicon.ico", FileMode.Create);
            fs.Write(bytes, 0, bytes.Length);

            return bytes;
        }

        public byte[] ReadBytesFromUri(string url) {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute)) {
                MpConsole.WriteTraceLine(@"Cannot read bytes, bad url: " + url);
                return null;
            }
            using var httpClient = new HttpClient();
            //RunSync<object>(() => dpv.GetDataAsync(af).AsTask());
            byte[] bytes = MpAsyncHelpers.RunSync<byte[]>(() => httpClient.GetByteArrayAsync(url));

            using var fs = new FileStream("favicon.ico", FileMode.Create);
            fs.Write(bytes, 0, bytes.Length);

            return bytes;
        }

        public byte[] ReadBytesFromFile(string filePath) {
            if(!File.Exists(filePath)) {
                return null;
            }
            try {
                using var fs = new FileStream(filePath, FileMode.Open);

                int c;
                var bytes = new List<byte>();

                while ((c = fs.ReadByte()) != -1) {
                    bytes.Add((byte)c);
                }

                return bytes.ToArray();
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine("MpHelpers.ReadTextFromFile error for filePath: " + filePath + ex.ToString());
                return null;
            }
        }

        public Xamarin.Forms.ImageSource ReadImageFromFile(string filePath) {
            try {
                var bytes = ReadBytesFromFile(filePath);
                return new MpImageConverter().Convert(bytes.ToArray(), typeof(Xamarin.Forms.ImageSource)) as Xamarin.Forms.ImageSource;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine("MpHelpers.ReadTextFromFile error for filePath: " + filePath + ex.ToString());
                return null;
            }
        }

        public bool DeleteFile(string filePath) {
            if (File.Exists(filePath)) {
                try {
                    File.Delete(filePath);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine("MpHelpers.ReadTextFromFile error for filePath: " + filePath + ex.ToString());
                    return false;
                }
            }
            return true;
        }

        public string WriteTextToFile(string filePath, string text, bool isTemporary = false) {
            try {
                if (filePath.ToLower().Contains(@".tmp")) {
                    string extension = string.Empty;
                    if (MpHelpers.Instance.IsStringRichText(text)) {
                        extension = @".rtf";
                    } else if (MpHelpers.Instance.IsStringCsv(text)) {
                        extension = @".csv";
                    } else {
                        extension = @".txt";
                    }
                    filePath = filePath.ToLower().Replace(@".tmp", extension);
                }
                using (var of = new StreamWriter(filePath)) {
                    of.Write(text);
                    of.Close();
                    if (isTemporary) {
                        MpTempFileManager.Instance.AddTempFilePath(filePath);
                    }
                    return filePath;
                }
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error writing to path '{filePath}' with text '{text}'", ex);
                return null;
            }
        }

        public string WriteByteArrayToFile(string filePath, byte[] byteArray, bool isTemporary = false) {
            try {
                if (filePath.ToLower().Contains(@".tmp")) {
                    filePath = filePath.ToLower().Replace(@".tmp", @".png");
                }
                File.WriteAllBytes(filePath, byteArray);
                return filePath;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error writing to path {filePath} for byte array " + (byteArray == null ? "which is null" : "which is NOT null"), ex);
                return null;
            }
        }
        #endregion

        #region Visual

        private List<List<Brush>> _ContentColors = new List<List<Brush>> {
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(248, 160, 174)),
                    new SolidColorBrush(Color.FromRgb(243, 69, 68)),
                    new SolidColorBrush(Color.FromRgb(229, 116, 102)),
                    new SolidColorBrush(Color.FromRgb(211, 159, 161)),
                    new SolidColorBrush(Color.FromRgb(191, 53, 50))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(252, 168, 69)),
                    new SolidColorBrush(Color.FromRgb(251, 108, 40)),
                    new SolidColorBrush(Color.FromRgb(253, 170, 130)),
                    new SolidColorBrush(Color.FromRgb(189, 141, 103)),
                    new SolidColorBrush(Color.FromRgb(177, 86, 55))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(215, 157, 60)),
                    new SolidColorBrush(Color.FromRgb(168, 123, 82)),
                    new SolidColorBrush(Color.FromRgb(214, 182, 133)),
                    new SolidColorBrush(Color.FromRgb(162, 144, 122)),
                    new SolidColorBrush(Color.FromRgb(123, 85, 72))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(247, 245, 144)),
                    new SolidColorBrush(Color.FromRgb(252, 240, 78)),
                    new SolidColorBrush(Color.FromRgb(239, 254, 185)),
                    new SolidColorBrush(Color.FromRgb(198, 193, 127)),
                    new SolidColorBrush(Color.FromRgb(224, 200, 42))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(189, 254, 40)),
                    new SolidColorBrush(Color.FromRgb(143, 254, 115)),
                    new SolidColorBrush(Color.FromRgb(217, 231, 170)),
                    new SolidColorBrush(Color.FromRgb(172, 183, 38)),
                    new SolidColorBrush(Color.FromRgb(140, 157, 45))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(50, 255, 76)),
                    new SolidColorBrush(Color.FromRgb(68, 199, 33)),
                    new SolidColorBrush(Color.FromRgb(193, 214, 135)),
                    new SolidColorBrush(Color.FromRgb(127, 182, 99)),
                    new SolidColorBrush(Color.FromRgb(92, 170, 58))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(54, 255, 173)),
                    new SolidColorBrush(Color.FromRgb(32, 195, 178)),
                    new SolidColorBrush(Color.FromRgb(170, 206, 160)),
                    new SolidColorBrush(Color.FromRgb(160, 201, 197)),
                    new SolidColorBrush(Color.FromRgb(32, 159, 148))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(96, 255, 227)),
                    new SolidColorBrush(Color.FromRgb(46, 238, 249)),
                    new SolidColorBrush(Color.FromRgb(218, 253, 233)),
                    new SolidColorBrush(Color.FromRgb(174, 193, 208)),
                    new SolidColorBrush(Color.FromRgb(40, 103, 146))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(149, 204, 243)),
                    new SolidColorBrush(Color.FromRgb(43, 167, 237)),
                    new SolidColorBrush(Color.FromRgb(215, 244, 248)),
                    new SolidColorBrush(Color.FromRgb(153, 178, 198)),
                    new SolidColorBrush(Color.FromRgb(30, 51, 160))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(99, 141, 227)),
                    new SolidColorBrush(Color.FromRgb(22, 127, 193)),
                    new SolidColorBrush(Color.FromRgb(201, 207, 233)),
                    new SolidColorBrush(Color.FromRgb(150, 163, 208)),
                    new SolidColorBrush(Color.FromRgb(52, 89, 170))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(157, 176, 255)),
                    new SolidColorBrush(Color.FromRgb(148, 127, 220)),
                    new SolidColorBrush(Color.FromRgb(216, 203, 233)),
                    new SolidColorBrush(Color.FromRgb(180, 168, 192)),
                    new SolidColorBrush(Color.FromRgb(109, 90, 179))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(221, 126, 230)),
                    new SolidColorBrush(Color.FromRgb(186, 141, 200)),
                    new SolidColorBrush(Color.FromRgb(185, 169, 231)),
                    new SolidColorBrush(Color.FromRgb(203, 178, 200)),
                    new SolidColorBrush(Color.FromRgb(170, 90, 179))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(225, 103, 164)),
                    new SolidColorBrush(Color.FromRgb(252, 74, 210)),
                    new SolidColorBrush(Color.FromRgb(238, 233, 237)),
                    new SolidColorBrush(Color.FromRgb(195, 132, 163)),
                    new SolidColorBrush(Color.FromRgb(205, 60, 117))
                },
                new List<Brush> {
                    new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    new SolidColorBrush(Color.FromRgb(223, 223, 223)),
                    new SolidColorBrush(Color.FromRgb(187, 187, 187)),
                    new SolidColorBrush(Color.FromRgb(137, 137, 137)),
                    new SolidColorBrush(Color.FromRgb(65, 65, 65))
                }
            };

        public Brush GetContentColor(int c, int r) {
            return _ContentColors[c][r];
        }

        public List<string> CreatePrimaryColorList(SKBitmap skbmp, int listCount = 5) {
            //var sw = new Stopwatch();
            //sw.Start();

            var primaryIconColorList = new List<string>();
            List<KeyValuePair<SKColor, int>> hist = MpImageHistogram.Instance.GetStatistics(skbmp);
            //foreach (var kvp in hist) {
            //    //var c = Color.FromRgba(kvp.Key.Red, kvp.Key.Green, kvp.Key.Blue, 255);
            //    SKColor c = kvp.Key;
            //    c = new SKColor(c.Red, c.Green, c.Blue, 255);
            //    //Console.WriteLine(string.Format(@"R:{0} G:{1} B:{2} Count:{3}", kvp.Key.Red, kvp.Key.Green, kvp.Key.Blue, kvp.Value));
            //    if (primaryIconColorList.Count == listCount) {
            //        break;
            //    }

            //    //between 0-255 where 0 is black 255 is white
            //    var rgDiff = Math.Abs(c.Red - c.Green);
            //    var rbDiff = Math.Abs(c.Red - c.Blue);
            //    var gbDiff = Math.Abs(c.Green - c.Blue);
            //    var totalDiff = rgDiff + rbDiff + gbDiff;

            //    //0-255 0 is black
            //    var grayScaleValue = c.ToGrayScale().Red; //0.2126 * c.R + 0.7152 * c.G + 0.0722 * c.B;
            //    var relativeDist = 100;// primaryIconColorList.Count == 0 ? 100 : primaryIconColorList[primaryIconColorList.Count - 1].ToSkColor().ColorDistance(c);// MpHelpers.Instance.ColorDistance(Color.FromHex(), c);
            //    if (totalDiff > 50 &&
            //        grayScaleValue > 50 &&
            //        relativeDist > 15) {
            //        primaryIconColorList.Add(c.ToString());
            //    }
            //}

            //if only 1 color found within threshold make random list
            for (int i = primaryIconColorList.Count; i < listCount; i++) {
                primaryIconColorList.Add(MpHelpers.Instance.GetRandomColor().ToHex());
            }

            foreach(var c in primaryIconColorList) {
                Console.WriteLine(c);
            }
            //sw.Stop();
            //Console.WriteLine("Time to create icon statistics: " + sw.ElapsedMilliseconds + " ms");
            return primaryIconColorList;
        }

        public double ColorDistance(SKColor e1, SKColor e2) {
            //max between 0 and 764.83331517396653 (found by checking distance from white to black)
            long rmean = (long)((e1.Red + e2.Red) / 2);
            long r = (long)(e1.Red - e2.Red);
            long g = (long)(e1.Green - e2.Green);
            long b = (long)(e1.Blue - e2.Blue);
            double max = 764.83331517396653;
            double d = Math.Sqrt((((512 + rmean) * r * r) >> 8) + 4 * g * g + (((767 - rmean) * b * b) >> 8));
            return d / max;
        }

        public double ColorDistance(Color e1, Color e2) {
            //max between 0 and 764.83331517396653 (found by checking distance from white to black)
            long rmean = ((long)(e1.R*255) + (long)(e2.R*255)) / 2;
            long r = (long)(e1.R * 255) - (long)(e2.R * 255);
            long g = (long)(e1.G * 255) - (long)(e2.G * 255);
            long b = (long)(e1.B * 255) - (long)(e2.B * 255);
            double max = 764.83331517396653;
            double d = Math.Sqrt((((512 + rmean) * r * r) >> 8) + 4 * g * g + (((767 - rmean) * b * b) >> 8));
            return d / max;
        }

        public bool IsBright(Color c, int brightThreshold = 150) {
            double s = c.R < 1 || c.G < 1 || c.B < 1 ? 255 : 1;
            int grayVal = (int)Math.Sqrt(
                (c.R * s) * (c.R * s) * .299 +
                (c.G * s) * (c.G * s) * .587 +
                (c.B * s) * (c.B * s) * .114);
            return grayVal > brightThreshold;
        }

        public SolidColorBrush ChangeBrushAlpha(SolidColorBrush solidColorBrush, byte alpha) {
            var c = solidColorBrush.Color;
            solidColorBrush.Color = Color.FromRgba(c.R, c.G, c.B, (double)alpha);
            return solidColorBrush;
        }

        public SolidColorBrush ChangeBrushBrightness(SolidColorBrush b, double correctionFactor) {
            if (correctionFactor == 0.0f) {
                return b;
            }
            double red = (double)b.Color.R;
            double green = (double)b.Color.G;
            double blue = (double)b.Color.B;

            if (correctionFactor < 0) {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            } else {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }

            return new SolidColorBrush(Color.FromRgba((byte)red, (byte)green, (byte)blue, b.Color.A));
        }

        public Brush GetDarkerBrush(Brush b) {
            return ChangeBrushBrightness((SolidColorBrush)b, -0.5);
        }

        public Brush GetLighterBrush(Brush b) {
            return ChangeBrushBrightness((SolidColorBrush)b, 0.5);
        }

        public Color GetRandomColor(byte alpha = 255) {
            //if (alpha == 255) {
            //    return Color.FromArgb(alpha, (byte)Rand.Next(256), (byte)Rand.Next(256), (byte)Rand.Next(256));
            //}
            //return Color.FromArgb(alpha, (byte)Rand.Next(256), (byte)Rand.Next(256), (byte)Rand.Next(256));
            int x = Rand.Next(0, _ContentColors.Count);
            int y = Rand.Next(0, _ContentColors[0].Count-1);
            return ((SolidColorBrush)GetContentColor(x, y)).Color;
        }

        public Brush GetRandomBrushColor(byte alpha = 255) {
            return (Brush)new SolidColorBrush() { Color = GetRandomColor(alpha) };
        }
        #endregion

        #region Network

        public string GetIpForDomain(string domain) {
            if(string.IsNullOrEmpty(domain)) {
                return "0.0.0.0";
            }
            var al = Dns.GetHostAddresses(domain).ToList();
            foreach(var a in al) {
                if(a.AddressFamily == AddressFamily.InterNetwork) {
                    return a.ToString();
                }
            }
            return "0.0.0.0";
        }
        //if you are using local Hosting or on premises with self signed certficate,   
        //in IOS add domain host address and Android use IP ADDRESS  
        const string SERVICE_BASE_URL = "https://devenvexe.com"; //replace base address   
        const string SERVICE_RELATIVE_URL = "/my/api/path";

        public async Task<string> GetDataAsync(string baseUrl, string relUrl) {
            var uri = new Uri(relUrl, UriKind.Relative);
            var request = new HttpRequestMessage {
                Method = HttpMethod.Get,
                RequestUri = uri
            };

            var client = GetHttpClient(baseUrl);

            HttpResponseMessage response = null;

            try {
                response = await client.GetAsync(request.RequestUri, HttpCompletionOption.ResponseHeadersRead);
            }
            catch (Exception ex) {
                return ex.InnerException.Message;
            }

            var content = await response.Content.ReadAsStringAsync();

            return content;
        }

        HttpClient GetHttpClient(string baseUrl) {
            var handler = new HttpClientHandler {
                UseProxy = true,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            var client = new HttpClient(handler) {
                BaseAddress = new Uri(baseUrl)
            };

            client.DefaultRequestHeaders.Connection.Add("keep-alive");
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

            return client;
        }
        public bool IsConnectedToInternet() {
            var current = Connectivity.NetworkAccess;

            if (current == NetworkAccess.Internet) {
                return true;
            }
            return false;
        }
        public bool IsConnectedToNetwork() {
            return System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();

        }

        public bool IsMpServerAvailable() {
            if (!IsConnectedToNetwork()) {
                return false;
            }
            try {

                using (var client = new WebClient()) {
                    try {
                        var stream = client.OpenRead(@"https://www.monkeypaste.com/");
                        stream.Dispose();
                        return true;
                    }
                    catch (System.AggregateException ex) {
                        MpConsole.WriteTraceLine("Sync Server Unavailable", ex);
                        return false;
                    }
                    catch (Exception ex) {
                        MpConsole.WriteTraceLine("Sync Server Unavailable", ex);
                        return false;
                    }
                }

            }
            catch (Exception e) {
                MpConsole.WriteLine(e.ToString());
                return false;
            }
        }

        public string GetLocalIp4Address() {
            var ips = GetAllLocalIPv4(NetworkInterfaceType.Wireless80211);
            if (ips.Length > 0) {
                return ips[0];
            }
            ips = GetAllLocalIPv4(NetworkInterfaceType.Ethernet);
            if (ips.Length > 0) {
                return ips[0];
            }
            return "0.0.0.0";
        }

        public string[] GetAllLocalIPv4() {
            var ips = GetAllLocalIPv4(NetworkInterfaceType.Wireless80211).ToList();
            ips.AddRange(GetAllLocalIPv4(NetworkInterfaceType.Ethernet));
            return ips.ToArray();
        }

        private string[] GetAllLocalIPv4(NetworkInterfaceType _type) {
            List<string> ipAddrList = new List<string>();
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces()) {
                if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up && !item.Description.ToLower().Contains("virtual")) {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses) {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork) {
                            ipAddrList.Add(ip.Address.ToString());
                        }
                    }
                }
            }
            return ipAddrList.ToArray();
        }

        public string GetExternalIp4Address() {
            return new System.Net.WebClient().DownloadString("https://api.ipify.org");
        }

        public string GetFullyFormattedUrl(string str) {
            //returns url so it has protocol prefix
            if (str.StartsWith(@"http://")) {
                return str;
            }
            if (str.StartsWith(@"https://")) {
                return str;
            }
            //use http without s because if it is https then it will resolve to but otherwise will not load
            return @"http://" + str;
        }

        public async Task<string> GetUrlTitleAsync(string url) {
            string urlSource = await GetHttpSourceCodeAsync(url);

            //sdf<title>poop</title>
            //pre 3
            //post 14
            return GetXmlElementContent(urlSource, @"title");
        }

        public string GetUrlTitle(string url) {
            string urlSource = GetHttpSourceCode(url);

            //sdf<title>poop</title>
            //pre 3
            //post 14
            return GetXmlElementContent(urlSource, @"title");
        }

        public async Task<string> GetHttpSourceCodeAsync(string url) {
            if (!IsValidUrl(url)) {
                return string.Empty;
            }

            using (HttpClient client = new HttpClient()) {
                using (HttpResponseMessage response = await client.GetAsync(url)) {
                    using (HttpContent content = response.Content) {
                        return await content.ReadAsStringAsync();
                    }
                }
            }
        }

        public string GetHttpSourceCode(string url) {
            if (!IsValidUrl(url)) {
                return string.Empty;
            }

            using (HttpClient client = new HttpClient()) {
                using (HttpResponseMessage response = client.GetAsync(url).Result) {
                    using (HttpContent content = response.Content) {
                        return content.ReadAsStringAsync().Result;
                    }
                }
            }
        }

        public bool IsValidUrl(string str) {
            bool hasValidExtension = false;
            string lstr = str.ToLower();
            foreach (var ext in _domainExtensions) {
                if (lstr.Contains(ext)) {
                    hasValidExtension = true;
                    break;
                }
            }
            if (!hasValidExtension) {
                return false;
            }
            var mc = Regex.Match(str, MpRegEx.Instance.GetRegExForTokenType(MpSubTextTokenType.Uri), RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture);
            return mc.Success;
        }

        public string GetXmlElementContent(string xml, string element) {
            if (string.IsNullOrEmpty(xml) || string.IsNullOrEmpty(element)) {
                return string.Empty;
            }
            element = element.Replace(@"<", string.Empty).Replace(@"/>", string.Empty);
            element = @"<" + element + @">";
            var strl = xml.Split(new string[] { element }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (strl.Count > 1) {
                element = element.Replace(@"<", @"</");
                return strl[1].Substring(0, strl[1].IndexOf(element));
            }
            return string.Empty;
            //int sIdx = xml.IndexOf(element);
            //if (sIdx < 0) {
            //    return string.Empty;
            //}
            //sIdx += element.Length;
            //element = element.Replace(@"<", @"</");
            //int eIdx = xml.IndexOf(element);
            //if (eIdx < 0) {
            //    return string.Empty;
            //}
            //return xml.Substring(sIdx, eIdx - sIdx);
        }

        public string GetUrlDomain(string url) {
            //returns protocol prefixed domain url text
            try {
                url = GetFullyFormattedUrl(url);
                string host = new Uri(url).Host;
                var subDomainIdxList = host.IndexListOfAll(".");
                for (int i = subDomainIdxList.Count-1; i > 0; i--) {
                    string subStr = host.Substring(subDomainIdxList[i]);
                    if (_domainExtensions.Contains(subStr)) {
                        return host.Substring(subDomainIdxList[i - 1]+1);
                    }
                }
                return host;

                //int domainStartIdx = url.IndexOf(@"//") + 2;
                //if (url.Length <= domainStartIdx) {
                //    return string.Empty;
                //}
                //if (!url.Substring(domainStartIdx).Contains(@"/")) {
                //    if (subDomainIdxList.Count > 1) {
                //        return url.Substring(domainStartIdx).Substring(subDomainIdxList[subDomainIdxList.Count - 1]);
                //    }
                //    return url.Substring(domainStartIdx);
                //}
                //int domainEndIdx = url.Substring(domainStartIdx).IndexOf(@"/");
                //int preIdx = 0;
                //if (subDomainIdxList.Count > 1) {
                //    preIdx = subDomainIdxList[subDomainIdxList.Count - 1];
                //}
                //return url.Substring(domainStartIdx).Substring(preIdx, domainEndIdx - preIdx);
            }
            catch (Exception ex) {
                MpConsole.WriteLine("MpHelpers.GetUrlDomain error for url: " + url + " with exception: " + ex);
            }
            return null;
        }

        public string GetUrlFavicon(String url) {
            try {
                string urlDomain = GetUrlDomain(url);
                Uri favicon = new Uri(@"https://www.google.com/s2/favicons?sz=128&domain_url=" + urlDomain, UriKind.Absolute);
                //var img = new Image() {
                //    Aspect = Aspect.AspectFit,
                //    Source = Xamarin.Forms.ImageSource.FromUri(favicon),
                //};
                //if (img == null) {
                //    return string.Empty;
                //}
                var bytes = ReadBytesFromUri(favicon.AbsoluteUri);
                return new MpImageConverter().Convert(bytes, typeof(string)) as string;
            }
            catch (Exception ex) {
                Console.WriteLine("MpHelpers.GetUrlFavicon error for url: " + url + " with exception: " + ex);
                return string.Empty;
            }
        }

        public async Task<string> GetUrlFaviconAsync(string url) {
            try {
                string urlDomain = GetUrlDomain(url);
                Uri favicon = new Uri(@"https://www.google.com/s2/favicons?sz=128&domain_url=" + urlDomain, UriKind.Absolute);
                var bytes = await ReadBytesFromUriAsync(favicon.AbsoluteUri);
                return new MpImageConverter().Convert(bytes, typeof(string)) as string;
            }
            catch (Exception ex) {
                Console.WriteLine("MpHelpers.GetUrlFavicon error for url: " + url + " with exception: " + ex);
                return string.Empty;
            }
        }

        private string[] _domainExtensions = new string[] {
            // TODO try to sort these by common use to make more efficient
            ".com",
            ".org",
            ".gov",
            ".abbott",
            ".abogado",
            ".ac",
            ".academy",
            ".accountant",
            ".accountants",
            ".active",
            ".actor",
            ".ad",
            ".ads",
            ".adult",
            ".ae",
            ".aero",
            ".af",
            ".afl",
            ".ag",
            ".agency",
            ".ai",
            ".airforce",
            ".al",
            ".allfinanz",
            ".alsace",
            ".am",
            ".amsterdam",
            ".an",
            ".android",
            ".ao",
            ".apartments",
            ".aq",
            ".aquarelle",
            ".ar",
            ".archi",
            ".army",
            ".arpa",
            ".as",
            ".asia",
            ".associates",
            ".at",
            ".attorney",
            ".au",
            ".auction",
            ".audio",
            ".autos",
            ".aw",
            ".ax",
            ".axa",
            ".az",
            ".ba",
            ".band",
            ".bank",
            ".bar",
            ".barclaycard",
            ".barclays",
            ".bargains",
            ".bauhaus",
            ".bayern",
            ".bb",
            ".bbc",
            ".bd",
            ".be",
            ".beer",
            ".berlin",
            ".best",
            ".bf",
            ".bg",
            ".bh",
            ".bi",
            ".bid",
            ".bike",
            ".bingo",
            ".bio",
            ".biz",
            ".bj",
            ".bl",
            ".black",
            ".blackfriday",
            ".bloomberg",
            ".blue",
            ".bm",
            ".bmw",
            ".bn",
            ".bnpparibas",
            ".bo",
            ".boats",
            ".bond",
            ".boo",
            ".boutique",
            ".bq",
            ".br",
            ".brussels",
            ".bs",
            ".bt",
            ".budapest",
            ".build",
            ".builders",
            ".business",
            ".buzz",
            ".bv",
            ".bw",
            ".by",
            ".bz",
            ".bzh",
            ".ca",
            ".cab",
            ".cafe",
            ".cal",
            ".camera",
            ".camp",
            ".cancerresearch",
            ".canon",
            ".capetown",
            ".capital",
            ".caravan",
            ".cards",
            ".care",
            ".career",
            ".careers",
            ".cartier",
            ".casa",
            ".cash",
            ".casino",
            ".cat",
            ".catering",
            ".cbn",
            ".cc",
            ".cd",
            ".center",
            ".ceo",
            ".cern",
            ".cf",
            ".cfd",
            ".cg",
            ".ch",
            ".channel",
            ".chat",
            ".cheap",
            ".chloe",
            ".christmas",
            ".chrome",
            ".church",
            ".ci",
            ".citic",
            ".city",
            ".ck",
            ".cl",
            ".claims",
            ".cleaning",
            ".click",
            ".clinic",
            ".clothing",
            ".club",
            ".cm",
            ".cn",
            ".co",
            ".coach",
            ".codes",
            ".coffee",
            ".college",
            ".cologne",
            ".community",
            ".company",
            ".computer",
            ".condos",
            ".construction",
            ".consulting",
            ".contractors",
            ".cooking",
            ".cool",
            ".coop",
            ".country",
            ".courses",
            ".cr",
            ".credit",
            ".creditcard",
            ".cricket",
            ".crs",
            ".cruises",
            ".cu",
            ".cuisinella",
            ".cv",
            ".cw",
            ".cx",
            ".cy",
            ".cymru",
            ".cyou",
            ".cz",
            ".dabur",
            ".dad",
            ".dance",
            ".date",
            ".dating",
            ".datsun",
            ".day",
            ".dclk",
            ".de",
            ".deals",
            ".degree",
            ".delivery",
            ".democrat",
            ".dental",
            ".dentist",
            ".desi",
            ".design",
            ".dev",
            ".diamonds",
            ".diet",
            ".digital",
            ".direct",
            ".directory",
            ".discount",
            ".dj",
            ".dk",
            ".dm",
            ".dnp",
            ".do",
            ".docs",
            ".doha",
            ".domains",
            ".doosan",
            ".download",
            ".durban",
            ".dvag",
            ".dz",
            ".eat",
            ".ec",
            ".edu",
            ".education",
            ".ee",
            ".eg",
            ".eh",
            ".email",
            ".emerck",
            ".energy",
            ".engineer",
            ".engineering",
            ".enterprises",
            ".epson",
            ".equipment",
            ".er",
            ".erni",
            ".es",
            ".esq",
            ".estate",
            ".et",
            ".eu",
            ".eurovision",
            ".eus",
            ".events",
            ".everbank",
            ".exchange",
            ".expert",
            ".exposed",
            ".express",
            ".fail",
            ".faith",
            ".fan",
            ".fans",
            ".farm",
            ".fashion",
            ".feedback",
            ".fi",
            ".film",
            ".finance",
            ".financial",
            ".firmdale",
            ".fish",
            ".fishing",
            ".fit",
            ".fitness",
            ".fj",
            ".fk",
            ".flights",
            ".florist",
            ".flowers",
            ".flsmidth",
            ".fly",
            ".fm",
            ".fo",
            ".foo",
            ".football",
            ".forex",
            ".forsale",
            ".foundation",
            ".fr",
            ".frl",
            ".frogans",
            ".fund",
            ".furniture",
            ".futbol",
            ".ga",
            ".gal",
            ".gallery",
            ".garden",
            ".gb",
            ".gbiz",
            ".gd",
            ".gdn",
            ".ge",
            ".gent",
            ".gf",
            ".gg",
            ".ggee",
            ".gh",
            ".gi",
            ".gift",
            ".gifts",
            ".gives",
            ".gl",
            ".glass",
            ".gle",
            ".global",
            ".globo",
            ".gm",
            ".gmail",
            ".gmo",
            ".gmx",
            ".gn",
            ".gold",
            ".goldpoint",
            ".golf",
            ".goo",
            ".goog",
            ".google",
            ".gop",
            ".gp",
            ".gq",
            ".gr",
            ".graphics",
            ".gratis",
            ".green",
            ".gripe",
            ".gs",
            ".gt",
            ".gu",
            ".guge",
            ".guide",
            ".guitars",
            ".guru",
            ".gw",
            ".gy",
            ".hamburg",
            ".hangout",
            ".haus",
            ".healthcare",
            ".help",
            ".here",
            ".hermes",
            ".hiphop",
            ".hiv",
            ".hk",
            ".hm",
            ".hn",
            ".holdings",
            ".holiday",
            ".homes",
            ".horse",
            ".host",
            ".hosting",
            ".house",
            ".how",
            ".hr",
            ".ht",
            ".hu",
            ".ibm",
            ".id",
            ".ie",
            ".ifm",
            ".il",
            ".im",
            ".immo",
            ".immobilien",
            ".in",
            ".industries",
            ".infiniti",
            ".info",
            ".ing",
            ".ink",
            ".institute",
            ".insure",
            ".int",
            ".international",
            ".investments",
            ".io",
            ".iq",
            ".ir",
            ".irish",
            ".is",
            ".it",
            ".iwc",
            ".java",
            ".jcb",
            ".je",
            ".jetzt",
            ".jm",
            ".jo",
            ".jobs",
            ".joburg",
            ".jp",
            ".juegos",
            ".kaufen",
            ".kddi",
            ".ke",
            ".kg",
            ".kh",
            ".ki",
            ".kim",
            ".kitchen",
            ".kiwi",
            ".km",
            ".kn",
            ".koeln",
            ".komatsu",
            ".kp",
            ".kr",
            ".krd",
            ".kred",
            ".kw",
            ".ky",
            ".kyoto",
            ".kz",
            ".la",
            ".lacaixa",
            ".land",
            ".lat",
            ".latrobe",
            ".lawyer",
            ".lb",
            ".lc",
            ".lds",
            ".lease",
            ".leclerc",
            ".legal",
            ".lgbt",
            ".li",
            ".lidl",
            ".life",
            ".lighting",
            ".limited",
            ".limo",
            ".link",
            ".lk",
            ".loan",
            ".loans",
            ".london",
            ".lotte",
            ".lotto",
            ".love",
            ".lr",
            ".ls",
            ".lt",
            ".ltda",
            ".lu",
            ".luxe",
            ".luxury",
            ".lv",
            ".ly",
            ".ma",
            ".madrid",
            ".maif",
            ".maison",
            ".management",
            ".mango",
            ".market",
            ".marketing",
            ".markets",
            ".marriott",
            ".mc",
            ".md",
            ".me",
            ".media",
            ".meet",
            ".melbourne",
            ".meme",
            ".memorial",
            ".menu",
            ".mf",
            ".mg",
            ".mh",
            ".miami",
            ".mil",
            ".mini",
            ".mk",
            ".ml",
            ".mm",
            ".mma",
            ".mn",
            ".mo",
            ".mobi",
            ".moda",
            ".moe",
            ".monash",
            ".money",
            ".mormon",
            ".mortgage",
            ".moscow",
            ".motorcycles",
            ".mov",
            ".movie",
            ".mp",
            ".mq",
            ".mr",
            ".ms",
            ".mt",
            ".mtn",
            ".mtpc",
            ".mu",
            ".museum",
            ".mv",
            ".mw",
            ".mx",
            ".my",
            ".mz",
            ".na",
            ".nagoya",
            ".name",
            ".navy",
            ".nc",
            ".ne",
            ".net",
            ".network",
            ".neustar",
            ".new",
            ".news",
            ".nexus",
            ".nf",
            ".ng",
            ".ngo",
            ".nhk",
            ".ni",
            ".nico",
            ".ninja",
            ".nissan",
            ".nl",
            ".no",
            ".np",
            ".nr",
            ".nra",
            ".nrw",
            ".ntt",
            ".nu",
            ".nyc",
            ".nz",
            ".okinawa",
            ".om",
            ".one",
            ".ong",
            ".onl",
            ".online",
            ".ooo",
            ".organic",
            ".osaka",
            ".otsuka",
            ".ovh",
            ".pa",
            ".page",
            ".panerai",
            ".paris",
            ".partners",
            ".parts",
            ".party",
            ".pe",
            ".pf",
            ".pg",
            ".ph",
            ".pharmacy",
            ".photo",
            ".photography",
            ".photos",
            ".physio",
            ".piaget",
            ".pics",
            ".pictet",
            ".pictures",
            ".pink",
            ".pizza",
            ".pk",
            ".pl",
            ".place",
            ".plumbing",
            ".plus",
            ".pm",
            ".pn",
            ".pohl",
            ".poker",
            ".porn",
            ".post",
            ".pr",
            ".praxi",
            ".press",
            ".pro",
            ".prod",
            ".productions",
            ".prof",
            ".properties",
            ".property",
            ".ps",
            ".pt",
            ".pub",
            ".pw",
            ".py",
            ".qa",
            ".qpon",
            ".quebec",
            ".racing",
            ".re",
            ".realtor",
            ".recipes",
            ".red",
            ".redstone",
            ".rehab",
            ".reise",
            ".reisen",
            ".reit",
            ".ren",
            ".rentals",
            ".repair",
            ".report",
            ".republican",
            ".rest",
            ".restaurant",
            ".review",
            ".reviews",
            ".rich",
            ".rio",
            ".rip",
            ".ro",
            ".rocks",
            ".rodeo",
            ".rs",
            ".rsvp",
            ".ru",
            ".ruhr",
            ".rw",
            ".ryukyu",
            ".sa",
            ".saarland",
            ".sale",
            ".samsung",
            ".sap",
            ".sarl",
            ".saxo",
            ".sb",
            ".sc",
            ".sca",
            ".scb",
            ".schmidt",
            ".scholarships",
            ".school",
            ".schule",
            ".schwarz",
            ".science",
            ".scot",
            ".sd",
            ".se",
            ".services",
            ".sew",
            ".sexy",
            ".sg",
            ".sh",
            ".shiksha",
            ".shoes",
            ".shriram",
            ".si",
            ".singles",
            ".site",
            ".sj",
            ".sk",
            ".sky",
            ".sl",
            ".sm",
            ".sn",
            ".so",
            ".social",
            ".software",
            ".sohu",
            ".solar",
            ".solutions",
            ".soy",
            ".space",
            ".spiegel",
            ".spreadbetting",
            ".sr",
            ".ss",
            ".st",
            ".study",
            ".style",
            ".su",
            ".sucks",
            ".supplies",
            ".supply",
            ".support",
            ".surf",
            ".surgery",
            ".suzuki",
            ".sv",
            ".sx",
            ".sy",
            ".sydney",
            ".systems",
            ".sz",
            ".taipei",
            ".tatar",
            ".tattoo",
            ".tax",
            ".tc",
            ".td",
            ".tech",
            ".technology",
            ".tel",
            ".temasek",
            ".tennis",
            ".tf",
            ".tg",
            ".th",
            ".tickets",
            ".tienda",
            ".tips",
            ".tires",
            ".tirol",
            ".tj",
            ".tk",
            ".tl",
            ".tm",
            ".tn",
            ".to",
            ".today",
            ".tokyo",
            ".tools",
            ".top",
            ".toshiba",
            ".tours",
            ".town",
            ".toys",
            ".tp",
            ".tr",
            ".trade",
            ".trading",
            ".training",
            ".travel",
            ".trust",
            ".tt",
            ".tui",
            ".tv",
            ".tw",
            ".tz",
            ".ua",
            ".ug",
            ".uk",
            ".um",
            ".university",
            ".uno",
            ".uol",
            ".us",
            ".uy",
            ".uz",
            ".va",
            ".vacations",
            ".vc",
            ".ve",
            ".vegas",
            ".ventures",
            ".versicherung",
            ".vet",
            ".vg",
            ".vi",
            ".viajes",
            ".video",
            ".villas",
            ".vision",
            ".vlaanderen",
            ".vn",
            ".vodka",
            ".vote",
            ".voting",
            ".voto",
            ".voyage",
            ".vu",
            ".wales",
            ".wang",
            ".watch",
            ".webcam",
            ".website",
            ".wed",
            ".wedding",
            ".wf",
            ".whoswho",
            ".wien",
            ".wiki",
            ".williamhill",
            ".win",
            ".wme",
            ".work",
            ".works",
            ".world",
            ".ws",
            ".wtc",
            ".wtf",
            ".xin",
            ".æµ‹è¯•",
            ".à¤ªà¤°à¥€à¤•à¥à¤·à¤¾",
            ".ä½›å±±",
            ".æ…ˆå–„",
            ".é›†å›¢",
            ".åœ¨çº¿",
            ".í•œêµ­",
            ".à¦­à¦¾à¦°à¦¤",
            ".å…«å¦",
            ".Ù…ÙˆÙ‚Ø¹",
            ".à¦¬à¦¾à¦‚à¦²à¦¾",
            ".å…¬ç›Š",
            ".å…¬å¸",
            ".ç§»åŠ¨",
            ".æˆ‘çˆ±ä½ ",
            ".Ð¼Ð¾ÑÐºÐ²Ð°",
            ".Ð¸ÑÐ¿Ñ‹Ñ‚Ð°Ð½Ð¸Ðµ",
            ".Ò›Ð°Ð·",
            ".Ð¾Ð½Ð»Ð°Ð¹Ð½",
            ".ÑÐ°Ð¹Ñ‚",
            ".ÑÑ€Ð±",
            ".Ð±ÐµÐ»",
            ".æ—¶å°š",
            ".í…ŒìŠ¤íŠ¸",
            ".æ·¡é©¬é”¡",
            ".Ð¾Ñ€Ð³",
            ".ì‚¼ì„±",
            ".à®šà®¿à®™à¯à®•à®ªà¯à®ªà¯‚à®°à¯",
            ".å•†æ ‡",
            ".å•†åº—",
            ".å•†åŸŽ",
            ".Ð´ÐµÑ‚Ð¸",
            ".Ð¼ÐºÐ´",
            ".×˜×¢×¡×˜",
            ".ä¸­æ–‡ç½‘",
            ".ä¸­ä¿¡",
            ".ä¸­å›½",
            ".ä¸­åœ‹",
            ".è°·æ­Œ",
            ".à°­à°¾à°°à°¤à±",
            ".à¶½à¶‚à¶šà·",
            ".æ¸¬è©¦",
            ".àª­àª¾àª°àª¤",
            ".à¤­à¤¾à¤°à¤¤",
            ".Ø¢Ø²Ù…Ø§ÛŒØ´ÛŒ",
            ".à®ªà®°à®¿à®Ÿà¯à®šà¯ˆ",
            ".ç½‘åº—",
            ".à¤¸à¤‚à¤—à¤ à¤¨",
            ".ç½‘ç»œ",
            ".ÑƒÐºÑ€",
            ".é¦™æ¸¯",
            ".Î´Î¿ÎºÎ¹Î¼Î®",
            ".é£žåˆ©æµ¦",
            ".Ø¥Ø®ØªØ¨Ø§Ø±",
            ".å°æ¹¾",
            ".å°ç£",
            ".æ‰‹æœº",
            ".Ð¼Ð¾Ð½",
            ".Ø§Ù„Ø¬Ø²Ø§Ø¦Ø±",
            ".Ø¹Ù…Ø§Ù†",
            ".Ø§ÛŒØ±Ø§Ù†",
            ".Ø§Ù…Ø§Ø±Ø§Øª",
            ".Ø¨Ø§Ø²Ø§Ø±",
            ".Ù¾Ø§Ú©Ø³ØªØ§Ù†",
            ".Ø§Ù„Ø§Ø±Ø¯Ù†",
            ".Ø¨Ú¾Ø§Ø±Øª",
            ".Ø§Ù„Ù…ØºØ±Ø¨",
            ".Ø§Ù„Ø³Ø¹ÙˆØ¯ÙŠØ©",
            ".Ø³ÙˆØ¯Ø§Ù†",
            ".Ø¹Ø±Ø§Ù‚",
            ".Ù…Ù„ÙŠØ³ÙŠØ§",
            ".æ”¿åºœ",
            ".Ø´Ø¨ÙƒØ©",
            ".áƒ’áƒ”",
            ".æœºæž„",
            ".ç»„ç»‡æœºæž„",
            ".å¥åº·",
            ".à¹„à¸—à¸¢",
            ".Ø³ÙˆØ±ÙŠØ©",
            ".Ñ€ÑƒÑ",
            ".Ñ€Ñ„",
            ".ØªÙˆÙ†Ø³",
            ".ã¿ã‚“ãª",
            ".ã‚°ãƒ¼ã‚°ãƒ«",
            ".ä¸–ç•Œ",
            ".à¨­à¨¾à¨°à¨¤",
            ".ç½‘å€",
            ".æ¸¸æˆ",
            ".vermÃ¶gensberater",
            ".vermÃ¶gensberatung",
            ".ä¼ä¸š",
            ".ä¿¡æ¯",
            ".Ù…ØµØ±",
            ".Ù‚Ø·Ø±",
            ".å¹¿ä¸œ",
            ".à®‡à®²à®™à¯à®•à¯ˆ",
            ".à®‡à®¨à¯à®¤à®¿à®¯à®¾",
            ".Õ°Õ¡Õµ",
            ".æ–°åŠ å¡",
            ".ÙÙ„Ø³Ø·ÙŠÙ†",
            ".ãƒ†ã‚¹ãƒˆ",
            ".æ”¿åŠ¡",
            ".xxx",
            ".xyz",
            ".yachts",
            ".yandex",
            ".ye",
            ".yodobashi",
            ".yoga",
            ".yokohama",
            ".youtube",
            ".yt",
            ".za",
            ".zip",
            ".zm",
            ".zone",
            ".zuerich",
            ".zw"
        };
        #endregion

        #region Converters
        #endregion
    }
}
