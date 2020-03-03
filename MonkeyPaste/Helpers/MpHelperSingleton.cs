using ExtractLargeIconFromFile;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ExtractLargeIconFromFile.ShellEx;

namespace MonkeyPaste {
    public class MpHelperSingleton {
        private static readonly Lazy<MpHelperSingleton> lazy = new Lazy<MpHelperSingleton>(() => new MpHelperSingleton());
        public static MpHelperSingleton Instance { get { return lazy.Value; } }

        public Bitmap GetBitmapFromFilePath(string filepath,IconSizeEnum iconsize) {
            return ShellEx.GetBitmapFromFilePath(filepath,iconsize);
        }
        public Image GetIconImage(IntPtr sourceHandle) {
            return GetBitmapFromFilePath(MpHelperSingleton.Instance.GetProcessPath(sourceHandle),IconSizeEnum.ExtraLargeIcon);
            //return IconReader.GetFileIcon(MpHelperSingleton.Instance.GetProcessPath(sourceHandle),IconReader.IconSize.Large,false).ToBitmap();
        }
        public Image GetIconImage(string sourcePath) {
            return GetBitmapFromFilePath(sourcePath,IconSizeEnum.ExtraLargeIcon);
            //return IconReader.GetFileIcon(MpHelperSingleton.Instance.GetProcessPath(sourceHandle),IconReader.IconSize.Large,false).ToBitmap();
        }
        /// <summary>
        /// Method to rotate an Image object. The result can be one of three cases:
        /// - upsizeOk = true: output image will be larger than the input, and no clipping occurs 
        /// - upsizeOk = false & clipOk = true: output same size as input, clipping occurs
        /// - upsizeOk = false & clipOk = false: output same size as input, image reduced, no clipping
        /// 
        /// A background color must be specified, and this color will fill the edges that are not 
        /// occupied by the rotated image. If color = transparent the output image will be 32-bit, 
        /// otherwise the output image will be 24-bit.
        /// 
        /// Note that this method always returns a new Bitmap object, even if rotation is zero - in 
        /// which case the returned object is a clone of the input object. 
        /// </summary>
        /// <param name="inputImage">input Image object, is not modified</param>
        /// <param name="angleDegrees">angle of rotation, in degrees</param>
        /// <param name="upsizeOk">see comments above</param>
        /// <param name="clipOk">see comments above, not used if upsizeOk = true</param>
        /// <param name="backgroundColor">color to fill exposed parts of the background</param>
        /// <returns>new Bitmap object, may be larger than input image</returns>
        public Image RotateImage(Image inputImage,float angleDegrees,bool upsizeOk,
                                         bool clipOk,Color backgroundColor) {
            // Test for zero rotation and return a clone of the input image
            if(angleDegrees == 0f)
                return (Image)inputImage.Clone();

            // Set up old and new image dimensions, assuming upsizing not wanted and clipping OK
            int oldWidth = inputImage.Width;
            int oldHeight = inputImage.Height;
            int newWidth = oldWidth;
            int newHeight = oldHeight;
            float scaleFactor = 1f;

            // If upsizing wanted or clipping not OK calculate the size of the resulting bitmap
            if(upsizeOk || !clipOk) {
                double angleRadians = angleDegrees * Math.PI / 180d;

                double cos = Math.Abs(Math.Cos(angleRadians));
                double sin = Math.Abs(Math.Sin(angleRadians));
                newWidth = (int)Math.Round(oldWidth * cos + oldHeight * sin);
                newHeight = (int)Math.Round(oldWidth * sin + oldHeight * cos);
            }

            // If upsizing not wanted and clipping not OK need a scaling factor
            if(!upsizeOk && !clipOk) {
                scaleFactor = Math.Min((float)oldWidth / newWidth,(float)oldHeight / newHeight);
                newWidth = oldWidth;
                newHeight = oldHeight;
            }

            // Create the new bitmap object. If background color is transparent it must be 32-bit, 
            //  otherwise 24-bit is good enough.
            Bitmap newBitmap = new Bitmap(newWidth,newHeight,backgroundColor == Color.Transparent ?
                                             PixelFormat.Format32bppArgb : PixelFormat.Format24bppRgb);
            newBitmap.SetResolution(inputImage.HorizontalResolution,inputImage.VerticalResolution);

            // Create the Graphics object that does the work
            using(Graphics graphicsObject = Graphics.FromImage(newBitmap)) {
                graphicsObject.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphicsObject.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphicsObject.SmoothingMode = SmoothingMode.HighQuality;

                // Fill in the specified background color if necessary
                if(backgroundColor != Color.Transparent)
                    graphicsObject.Clear(backgroundColor);

                // Set up the built-in transformation matrix to do the rotation and maybe scaling
                graphicsObject.TranslateTransform(newWidth / 2f,newHeight / 2f);

                if(scaleFactor != 1f)
                    graphicsObject.ScaleTransform(scaleFactor,scaleFactor);

                graphicsObject.RotateTransform(angleDegrees);
                graphicsObject.TranslateTransform(-oldWidth / 2f,-oldHeight / 2f);

                // Draw the result 
                graphicsObject.DrawImage(inputImage,0,0);
            }

            return (Image)newBitmap;
        }
        public Color GetDominantColor(Bitmap bmp) {            
            //Used for tally
            int r = 0;
            int g = 0;
            int b = 0;

            int total = 0;

            for(int x = 0;x < bmp.Width;x++) {
                for(int y = 0;y < bmp.Height;y++) {
                    Color clr = bmp.GetPixel(x,y);

                    r += clr.R;
                    g += clr.G;
                    b += clr.B;

                    total++;
                }
            }

            //Calculate average
            r /= total;
            g /= total;
            b /= total;

            return Color.FromArgb(r,g,b);
        }

        /// <summary>Determines the current screen resolution in DPI.</summary>
        /// <returns>Point.X is the X DPI, Point.Y is the Y DPI.</returns>
        public Point GetSystemDpi() {
            Point result = new Point();

            IntPtr hDC = WinApi.GetDC(IntPtr.Zero);

            result.X = WinApi.GetDeviceCaps(hDC,88); //LOGPIXELSX
            result.Y = WinApi.GetDeviceCaps(hDC,90); //LOGPIXELSY

            WinApi.ReleaseDC(IntPtr.Zero,hDC);

            return result;
        }

        /// <summary>
        /// Checks if font is not default.
        /// </summary>
        /// <returns>True if font DPI is not 96.</returns>
        public bool IsDifferentFont() {
            Point result = GetSystemDpi();

            return result.X != 96 || result.Y != 96;
        }


        public string GetProcessPath(IntPtr hwnd) {
            uint pid = 0;
            WinApi.GetWindowThreadProcessId(hwnd,out pid);
            //return MpHelperSingleton.Instance.GetMainModuleFilepath((int)pid);
            Process proc = Process.GetProcessById((int)pid);
            return proc.MainModule.FileName.ToString();
        }
        public string GetMainModuleFilepath(int processId) {
            string wmiQueryString = "SELECT ProcessId, ExecutablePath FROM Win32_Process WHERE ProcessId = " + processId;
            using(var searcher = new ManagementObjectSearcher(wmiQueryString)) {
                using(var results = searcher.Get()) {
                    ManagementObject mo = results.Cast<ManagementObject>().FirstOrDefault();
                    if(mo != null) {
                        return (string)mo["ExecutablePath"];
                    }
                }
            }
            return null;
        }
        public void ColorToHSV(Color color,out double hue,out double saturation,out double value) {
            int max = Math.Max(color.R,Math.Max(color.G,color.B));
            int min = Math.Min(color.R,Math.Min(color.G,color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }
        public Color ColorFromHSV(double hue,double saturation,double value) {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if(hi == 0)
                return Color.FromArgb(255,v,t,p);
            else if(hi == 1)
                return Color.FromArgb(255,q,v,p);
            else if(hi == 2)
                return Color.FromArgb(255,p,v,t);
            else if(hi == 3)
                return Color.FromArgb(255,p,q,v);
            else if(hi == 4)
                return Color.FromArgb(255,t,p,v);
            else
                return Color.FromArgb(255,v,p,q);
        }
        public Color GetInvertedColor(Color c) {
            double h, s, v;
            ColorToHSV(c,out h,out s,out v);
            h = (h + 180) % 360;
            return ColorFromHSV(h,s,v);
        }
        public Color GetRandomColor(int alpha = 255) {
            if(alpha == 255) {
                return Color.FromArgb(MpSingletonController.Instance.Rand.Next(256),MpSingletonController.Instance.Rand.Next(256),MpSingletonController.Instance.Rand.Next(256));
            }
            return Color.FromArgb(alpha,MpSingletonController.Instance.Rand.Next(256),MpSingletonController.Instance.Rand.Next(256),MpSingletonController.Instance.Rand.Next(256));
        }
        /*public Color GetRandomColor() {
            var random = new Random();
            return Color.FromArgb((int)(0xFF000000 + (MpSingletonController.Instance.Rand.Next(0xFFFFFF) & 0x7F7F7F)));
        }*/
        public int GetMaxLine(string text) {
            int cr = 0, mr = 0,mc = int.MinValue,cc = 0;
            char LF = '\n';
            foreach(char c in text.ToCharArray()) {
                if(c == LF) {
                    if(cc > mc) {
                        mc = cc;
                        mr = cr;
                    }
                    cc = 0;
                    cr++;
                } else {
                    cc++;
                }
            }
            return mr;
        }
        public int GetRowCount(string text,int lineNum) {
            int ccount = 0,cr = 0;
            char LF = '\n';
            foreach(char c in text.ToCharArray()) {
                if(cr == lineNum) {
                    ccount++;
                }
                if(c == LF) {
                    if(cr == lineNum) {
                        return ccount;
                    }
                    cr++;
                }
            }
            return ccount;
        }
        public Size GetTextDimensions(string text) {
            int rcount = 0, ccount = int.MinValue;
            char LF = '\n';
            //cur col count
            int  ccc = 0;
            foreach(char c in text.ToCharArray()) {
                ccc++;
                if(c == LF) {
                    rcount++;
                    if(ccc > ccount) {
                        ccount = ccc;
                    }
                    ccc = 0;
                }
            }
            ccount = ccount == 0 ? 1 : ccount;
            rcount = rcount == 0 ? 1 : rcount;
            return new Size(ccount,rcount);
        }
        public Size GetTextSize(string text,Font f) {
            Image fakeImage = new Bitmap(1,1);
            Graphics graphics = Graphics.FromImage(fakeImage);
            SizeF s = graphics.MeasureString(text,f);
            return new Size((int)s.Width,(int)s.Height);
        }
        public bool IsBright(Color c) {
            return (int)Math.Sqrt(
            c.R * c.R * .299 +
            c.G * c.G * .587 +
            c.B * c.B * .114) > 130;
        }
        public Color ChangeColorBrightness(Color color,float correctionFactor) {
            float red = (float)color.R;
            float green = (float)color.G;
            float blue = (float)color.B;

            if(correctionFactor < 0) {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }

            return Color.FromArgb(color.A,(int)red,(int)green,(int)blue);
        }
        public long FileListSize(string[] paths) {
            long total = 0;
            foreach(string path in paths) {
                if(Directory.Exists(path)) {
                    total += CalcDirSize(path,true);
                } else if(File.Exists(path)) {
                    total += (new FileInfo(path)).Length;
                }
            }
            return total;
        }
        private long CalcDirSize(string sourceDir,bool recurse = true) {
            return _CalcDirSize(new DirectoryInfo(sourceDir),recurse);
        }
        private long _CalcDirSize(DirectoryInfo di,bool recurse = true) {
            long size = 0;
            FileInfo[] fiEntries = di.GetFiles();
            foreach(var fiEntry in fiEntries) {
                Interlocked.Add(ref size,fiEntry.Length);
            }

            if(recurse) {
                DirectoryInfo[] diEntries = di.GetDirectories("*.*",SearchOption.TopDirectoryOnly);
                System.Threading.Tasks.Parallel.For<long>(0,diEntries.Length,() => 0,(i,loop,subtotal) =>
                {
                    if((diEntries[i].Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint) return 0;
                    subtotal += _CalcDirSize(diEntries[i],true);
                    return subtotal;
                }, 
                    (x) => Interlocked.Add(ref size,x)
                );

            }
            return size;
        }
       /* public long DirSize(string sourceDir,bool recurse) {
            long size = 0;
            string[] fileEntries = Directory.GetFiles(sourceDir);

            foreach(string fileName in fileEntries) {
                Interlocked.Add(ref size,(new FileInfo(fileName)).Length);
            }

            if(recurse) {
                string[] subdirEntries = Directory.GetDirectories(sourceDir);

                Parallel.For<long>(0,subdirEntries.Length,() => 0,(i,loop,subtotal) =>
                {
                    if((File.GetAttributes(subdirEntries[i]) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint) {
                        subtotal += DirSize(subdirEntries[i],true);
                        return subtotal;
                    }
                    return 0;
                },
                    (x) => Interlocked.Add(ref size,x)
                );
            }
            return size;
        }*/
        public int GetLineCount(string str) {
            char CR = '\r';
            char LF = '\n';
            //line count
            int lc = 0;
            //previous char
            char pc = '\0';
            //pending termination
            bool pt = false;
            foreach(char c in str.ToCharArray()) {
                if(c == CR || c == LF) {
                    if(pc == CR && c == LF) {
                        continue;
                    }
                    lc++;
                    pt = false;
                } else if(!pt) {
                    pt = true;
                }
                pc = c;
            }
            if(pt) {
                lc++;
            }
            return lc;
        }
        public Rectangle GetScreenBoundsWithMouse() {
            foreach(Screen screen in Screen.AllScreens) {
                Point mp = MpCursorPosition.GetCursorPosition();
                if(screen.WorkingArea.Contains(mp)) {
                    return screen.Bounds;
                }
            }
            return Screen.FromHandle(Process.GetCurrentProcess().Handle).Bounds;
        }
        public void SetPadding(TextBoxBase textBox,Padding padding) {
            var rect = new Rectangle(padding.Left,padding.Top,textBox.Size.Width - padding.Left - padding.Right,textBox.Size.Height - padding.Top - padding.Bottom);
            RECT rc = new RECT(rect);
            WinApi.SendMessageRefRect(textBox.Handle,WinApi.EM_SETRECT,0,ref rc);
        }
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
        
        public byte[] ConvertImageToByteArray(Image img) {
            MemoryStream ms = new MemoryStream();
            img.Save(ms,ImageFormat.Png);
            return ms.ToArray();
        }
        public Image ConvertByteArrayToImage(byte[] rawBytes) {
            return Image.FromStream(new MemoryStream(rawBytes),true);
        }
        //public Image GetIconImage(string path) {
        //    return (Image)IconReader.GetFileIcon(path,IconReader.IconSize.Large,false).ToBitmap();//Icon.ExtractAssociatedIcon(path).ToBitmap();
        //}
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
