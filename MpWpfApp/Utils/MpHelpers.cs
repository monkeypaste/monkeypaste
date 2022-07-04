using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Xml;
using QRCoder;
using System.Windows.Threading;
using System.Security.Principal;
using System.Speech.Synthesis;
using WindowsInput;
using MonkeyPaste;
using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; using MonkeyPaste.Common.Wpf;

namespace MpWpfApp {
    public static class MpHelpers {
        private static InputSimulator sim = new InputSimulator();

        public static Random Rand { get; set; } = new Random((int)DateTime.Now.Ticks);


        #region Documents    

        public static bool HasTable(RichTextBox rtb) {
            return rtb.Document.Blocks.Any(x => x is Table);
        }

        public static void ApplyBackgroundBrushToRangeList(ObservableCollection<ObservableCollection<TextRange>> rangeList, Brush bgBrush, CancellationToken ct) {
            if (rangeList == null || rangeList.Count == 0) {
                return;
            }
            foreach (var range in rangeList) {
                ApplyBackgroundBrushToRangeList(range, bgBrush, ct);
            }
        }

        public static void ApplyBackgroundBrushToRangeList(ObservableCollection<TextRange> rangeList, Brush bgBrush, CancellationToken ct) {
            if (rangeList == null || rangeList.Count == 0) {
                return;
            }
            foreach (var range in rangeList) {
                if(ct.IsCancellationRequested) {
                    //throw new OperationCanceledException();
                    MpConsole.WriteLine("Bg highlighting canceled");
                    return;
                }
                range.ApplyPropertyValue(TextElement.BackgroundProperty, bgBrush);
            }
        }

        public static DependencyObject FindParentOfType(DependencyObject dpo, Type type) {
            if (dpo == null) {
                return null;
            }
            if (dpo.GetType() == type) {
                return dpo;
            }
            if(dpo.GetType().IsSubclassOf(typeof(FrameworkContentElement))) {
                return FindParentOfType(((FrameworkContentElement)dpo).Parent, type);
            } else if (dpo.GetType().IsSubclassOf(typeof(FrameworkElement))) {
                return FindParentOfType(((FrameworkElement)dpo).Parent, type);
            } else {
                return null;
            }
        }

        //public static CurrencyType GetCurrencyTypeFromString(string moneyStr) {
        //    if (moneyStr == null || moneyStr.Length == 0) {
        //        return CurrencyType.USD;
        //    }
        //    char currencyLet = moneyStr[0];
        //    foreach(var c in MpCurrencyConverter.Instance.CurrencyList) {
        //         if(c.CurrencySymbol == currencyLet.ToString()) {
        //            Enum.TryParse(c.Id, out CurrencyType ct);
        //            return ct;
        //        }
        //    }
        //    return CurrencyType.USD;
        //}

        public static double GetCurrencyValueFromString(string moneyStr) {
            if(string.IsNullOrEmpty(moneyStr) || moneyStr.Length < 2) {
                return 0;
            }
            moneyStr = moneyStr.Remove(0, 1);
            try {
                return Math.Round(Convert.ToDouble(moneyStr), 2);
            }
            catch (Exception ex) {
                MpConsole.WriteLine(
                    "MpHelper exception cannot convert moneyStr '" + moneyStr + "' to a value, returning 0");
                MpConsole.WriteLine("Exception Details: " + ex);
                return 0;
            }
        }

        

        

        public static string CurrencyConvert(decimal amount, string fromCurrency, string toCurrency) {
            try {
                //Grab your values and build your Web Request to the API
                string apiURL = String.Format("https://www.google.com/finance/converter?a={0}&from={1}&to={2}&meta={3}", amount, fromCurrency, toCurrency, Guid.NewGuid().ToString());

                //Make your Web Request and grab the results
                var request = WebRequest.Create(apiURL);

                //Get the Response
                var streamReader = new StreamReader(request.GetResponse().GetResponseStream(), System.Text.Encoding.ASCII);

                //Grab your converted value (ie 2.45 USD)
                var result = Regex.Matches(streamReader.ReadToEnd(), "<span class=\"?bld\"?>([^<]+)</span>")[0].Groups[1].Value;

                //Get the Result
                return result;
            }
            catch (Exception ex) {
                MpConsole.WriteLine("MpHelpers Currency Conversion exception: " + ex.ToString());
                return string.Empty;
            }
        }


        #endregion

        #region System

        public static bool IsOnMainThread() {
            return Thread.CurrentThread == System.Windows.Threading.Dispatcher.CurrentDispatcher.Thread;
        }
        
        public static void RunOnMainThread(Action action, DispatcherPriority priority = DispatcherPriority.Normal) {
            Application.Current.Dispatcher.Invoke(action, priority);
        }

        public static TResult RunOnMainThread<TResult>(Func<TResult> action, DispatcherPriority priority = DispatcherPriority.Normal) {
            return Application.Current.Dispatcher.Invoke<TResult>(action, priority);
        }

        public static DispatcherOperation RunOnMainThreadAsync(Action action, DispatcherPriority priority = DispatcherPriority.Normal) {
            return Application.Current.Dispatcher.InvokeAsync(action, priority);
        }
        
        public static DispatcherOperation<TResult> RunOnMainThreadAsync<TResult>(Func<TResult> action, DispatcherPriority priority = DispatcherPriority.Normal) where TResult : class {
            return Application.Current.Dispatcher.InvokeAsync<TResult>(action, priority);
        }

        public static string GetTempFileNameWithExtension(string ext) {
            if(string.IsNullOrEmpty(ext)) {
                return Path.GetTempFileName();
            }
            return Path.GetTempFileName().Replace(@".tmp",string.Empty) + ext;
        }

        public static void PassKeysListToWindow(IntPtr handle,List<List<Key>> keyList) {     
            try {
                WinApi.SetForegroundWindow(handle);
                WinApi.SetActiveWindow(handle);
                for (int i = 0; i < keyList.Count; i++) {
                    var combo = keyList[i];
                    var vkCombo = new List<WindowsInput.Native.VirtualKeyCode>();
                    foreach (var key in combo) {
                        WindowsInput.Native.VirtualKeyCode vk = (WindowsInput.Native.VirtualKeyCode)KeyInterop.VirtualKeyFromKey(key);
                        vkCombo.Add(vk);
                    }
                    sim.Keyboard.KeyPress(vkCombo.ToArray());
                }
            }
            catch(Exception ex) {
                MpConsole.WriteLine("MpHelpers.PassKeysListToWindow exception: " + ex);
            }
        }

        public static InstalledVoice GetInstalledVoiceByName(string voiceName) {
            var speechSynthesizer = new SpeechSynthesizer();
            foreach (var voice in speechSynthesizer.GetInstalledVoices()) {
                if(voice.VoiceInfo.Name.Contains(voiceName)) {
                    return voice;
                }
            }
            return null;
        }

        public static double ConvertBytesToMegabytes(long bytes, int precision = 2) {
            return Math.Round((bytes / 1024f) / 1024f,precision);
        }

        public static void CreateBinding(
            object source, 
            PropertyPath sourceProperty, 
            DependencyObject target, 
            DependencyProperty targetProperty, 
            BindingMode mode = BindingMode.OneWay,
            IValueConverter converter = null,
            object converterParam = null) {
            Binding b = new Binding();
            b.Converter = converter;
            b.ConverterParameter = converterParam;
            b.Source = source;
            b.Path = sourceProperty;
            b.Mode = mode;
            if(b.Mode == BindingMode.TwoWay) {
                b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            }
            BindingOperations.SetBinding(target, targetProperty, b);
        }


        public static bool IsInDesignMode {
            get {
                return DesignerProperties.GetIsInDesignMode(new DependencyObject());
            }
        }

        public static bool ApplicationIsActivated() {
            var activatedHandle = WinApi.GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero) {
                return false;       // No window is currently activated
            }
            var procId = Process.GetCurrentProcess().Id;
            WinApi.GetWindowThreadProcessId(activatedHandle, out uint activeProcId);

            return (int)activeProcId == procId;
        }

        
        
        

        public static string ReadTextFromFile(string filePath) {
            try {
                using (StreamReader f = new StreamReader(filePath)) {
                    string outStr = string.Empty;
                    outStr = f.ReadToEnd();
                    f.Close();
                    return outStr;
                }
            }
            catch(Exception ex) {
                MpConsole.WriteLine("MpHelpers.ReadTextFromFile error for filePath: " + filePath + ex.ToString());
                return string.Empty;
            }
        }

        public static string WriteTextToFile(string filePath, string text, bool isTemporary = false) {
            if (filePath.ToLower().Contains(@".tmp")) {
                string extension = string.Empty;
                if (text.IsStringRichText()) {
                    extension = @".rtf";
                } else if (text.IsStringCsv()) {
                    extension = @".csv";
                } else {
                    extension = @".txt";
                }
                filePath = filePath.ToLower().Replace(@".tmp", extension);
            }
            using (StreamWriter of = new StreamWriter(filePath)) {
                of.Write(text);
                of.Close();
                if (isTemporary) {
                    MpTempFileManager.AddTempFilePath(filePath);
                }
                return filePath;
            }
        }
        public static string WriteBitmapSourceToFile(string filePath, BitmapSource bmpSrc, bool isTemporary = false) {
            if (filePath.ToLower().Contains(@".tmp")) {
                filePath = filePath.ToLower().Replace(@".tmp", @".png");
            }
            using (var fileStream = new FileStream(filePath, FileMode.Create)) {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmpSrc));
                encoder.Save(fileStream);
            }

            if (isTemporary) {
                MpTempFileManager.AddTempFilePath(filePath);
            }
            return filePath;
        }

        public static string WriteStringListToCsvFile(string filePath, IList<string> strList, bool isTemporary = false) {
            var textList = new List<string>();
            foreach (var str in strList) {
                if (!string.IsNullOrEmpty(str.Trim())) {
                    textList.Add(str);
                }
            }
            if (filePath.ToLower().Contains(@".tmp")) {
                filePath = filePath.ToLower().Replace(@".tmp", @".csv");
            }
            //using (var writer = new StreamWriter(filePath)) {
            //    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture)) {
            //        csv.WriteRecords(textList);
            //    }
            //}
            if (isTemporary) {
                MpTempFileManager.AddTempFilePath(filePath);
            }
            return filePath;
            //using (var stream = File.OpenRead(filePath)) {
            //    using (var reader = new StreamReader(stream)) {
            //        return reader.ReadToEnd();
            //    }
            //}
        }

        /* public static long DirSize(string sourceDir,bool recurse) {
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

        /*public static string GeneratePassword() {
            var generator = new MpPasswordGenerator(minimumLengthPassword: 8,
                                      maximumLengthPassword: 12,
                                      minimumUpperCaseChars: 2,
                                      minimumSpecialChars: 2);
            return generator.Generate();
        }*/

        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs) {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if (!dir.Exists) {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory does not exist, create it.
            if (!Directory.Exists(destDirName)) {
                Directory.CreateDirectory(destDirName);
            }

            // Get the file contents of the directory to copy.
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files) {
                // Create the path to the new copy of the file.
                string temppath = Path.Combine(destDirName, file.Name);

                // Copy the file.
                file.CopyTo(temppath, false);
            }

            // If copySubDirs is true, copy the subdirectories.
            if (copySubDirs) {
                foreach (DirectoryInfo subdir in dirs) {
                    // Create the subdirectory.
                    string temppath = Path.Combine(destDirName, subdir.Name);

                    // Copy the subdirectories.
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        public static string GetCPUInfo() {
            string cpuInfo = string.Empty;
            ManagementClass mc = new ManagementClass("win32_processor");
            ManagementObjectCollection moc = mc.GetInstances();

            foreach (ManagementObject mo in moc) {
                if (string.IsNullOrEmpty(cpuInfo)) {
                    //Get only the first CPU's ID
                    cpuInfo = mo.Properties["processorID"].Value.ToString();
                    break;
                }
            }
            return cpuInfo;
        }

        public static string GetShortcutTargetPath(string file) {
            try {
                if (System.IO.Path.GetExtension(file).ToLower() != ".lnk") {
                    throw new Exception("Supplied file must be a .LNK file");
                }

                FileStream fileStream = File.Open(file, FileMode.Open, FileAccess.Read);
                using (System.IO.BinaryReader fileReader = new BinaryReader(fileStream)) {
                    fileStream.Seek(0x14, SeekOrigin.Begin);     // Seek to flags
                    uint flags = fileReader.ReadUInt32();        // Read flags
                    if ((flags & 1) == 1) {                      // Bit 1 set means we have to
                                                                 // skip the shell item ID list
                        fileStream.Seek(0x4c, SeekOrigin.Begin); // Seek to the end of the header
                        uint offset = fileReader.ReadUInt16();   // Read the length of the Shell item ID list
                        fileStream.Seek(offset, SeekOrigin.Current); // Seek past it (to the file locator info)
                    }

                    long fileInfoStartsAt = fileStream.Position; // Store the offset where the file info
                                                                 // structure begins
                    uint totalStructLength = fileReader.ReadUInt32(); // read the length of the whole struct
                    fileStream.Seek(0xc, SeekOrigin.Current); // seek to offset to base pathname
                    uint fileOffset = fileReader.ReadUInt32(); // read offset to base pathname
                                                               // the offset is from the beginning of the file info struct (fileInfoStartsAt)
                    fileStream.Seek((fileInfoStartsAt + fileOffset), SeekOrigin.Begin); // Seek to beginning of
                                                                                        // base pathname (target)
                    long pathLength = (totalStructLength + fileInfoStartsAt) - fileStream.Position - 2; // read
                                                                                                        // the base pathname. I don't need the 2 terminating nulls.
                    char[] linkTarget = fileReader.ReadChars((int)pathLength); // should be unicode safe
                    var link = new string(linkTarget);

                    int begin = link.IndexOf("\0\0");
                    if (begin > -1) {
                        int end = link.IndexOf("\\\\", begin + 2) + 2;
                        end = link.IndexOf('\0', end) + 1;

                        string firstPart = link.Substring(0, begin);
                        string secondPart = link.Substring(end);

                        return firstPart + secondPart;
                    } else {
                        return link;
                    }
                }
            }
            catch {
                return string.Empty;
            }
        }


        public static IntPtr RunAsDesktopUser(string fileName, string args = "") {            
            if (string.IsNullOrWhiteSpace(fileName)) {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(fileName));
            }

            // To start process as shell user you will need to carry out these steps:
            // 1. Enable the SeIncreaseQuotaPrivilege in your current token
            // 2. Get an HWND representing the desktop shell (GetShellWindow)
            // 3. Get the Process ID(PID) of the process associated with that window(GetWindowThreadProcessId)
            // 4. Open that process(OpenProcess)
            // 5. Get the access token from that process (OpenProcessToken)
            // 6. Make a primary token with that token(DuplicateTokenEx)
            // 7. Start the new process with that primary token(CreateProcessWithTokenW)

            var hProcessToken = IntPtr.Zero;
            // Enable SeIncreaseQuotaPrivilege in this process.  (This won't work if current process is not elevated.)
            try {
                var process = WinApi.GetCurrentProcess();
                if (!WinApi.OpenProcessToken(process, 0x0020, ref hProcessToken)) {
                    return IntPtr.Zero;
                }

                var tkp = new WinApi.TOKEN_PRIVILEGES {
                    PrivilegeCount = 1,
                    Privileges = new WinApi.LUID_AND_ATTRIBUTES[1]
                };

                if (!WinApi.LookupPrivilegeValue(null, "SeIncreaseQuotaPrivilege", ref tkp.Privileges[0].Luid)) {
                    return IntPtr.Zero;
                }

                tkp.Privileges[0].Attributes = 0x00000002;

                if (!WinApi.AdjustTokenPrivileges(hProcessToken, false, ref tkp, 0, IntPtr.Zero, IntPtr.Zero)) {
                    return IntPtr.Zero;
                }
            } finally {
                WinApi.CloseHandle(hProcessToken);
            }

            // Get an HWND representing the desktop shell.
            // CAVEATS:  This will fail if the shell is not running (crashed or terminated), or the default shell has been
            // replaced with a custom shell.  This also won't return what you probably want if Explorer has been terminated and
            // restarted elevated.
            var hwnd = WinApi.GetShellWindow();
            if (hwnd == IntPtr.Zero) {
                return IntPtr.Zero;
            }

            var hShellProcess = IntPtr.Zero;
            var hShellProcessToken = IntPtr.Zero;
            var hPrimaryToken = IntPtr.Zero;
            try {
                // Get the PID of the desktop shell process.
                uint dwPID;
                if (WinApi.GetWindowThreadProcessId(hwnd, out dwPID) == 0) {
                    return IntPtr.Zero;
                }
                // Open the desktop shell process in order to query it (get the token)
                hShellProcess = WinApi.OpenProcess(WinApi.ProcessAccessFlags.QueryInformation, false, dwPID);
                if (hShellProcess == IntPtr.Zero) {
                    return IntPtr.Zero;
                }

                // Get the process token of the desktop shell.
                if (!WinApi.OpenProcessToken(hShellProcess, 0x0002, ref hShellProcessToken)) {
                    return IntPtr.Zero;
                }

                var dwTokenRights = 395U;

                // Duplicate the shell's process token to get a primary token.
                // Based on experimentation, this is the minimal set of rights required for CreateProcessWithTokenW (contrary to current documentation).
                if (!WinApi.DuplicateTokenEx(hShellProcessToken, dwTokenRights, IntPtr.Zero, WinApi.SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation, WinApi.TOKEN_TYPE.TokenPrimary, out hPrimaryToken)) {
                    return IntPtr.Zero;
                }

                // Start the target process with the new token.
                var si = new WinApi.STARTUPINFO();
                var pi = new WinApi.PROCESS_INFORMATION();
                if(string.IsNullOrEmpty(args)) {
                    args = "";
                }
                if (!WinApi.CreateProcessWithTokenW(hPrimaryToken, 0, fileName, args, 0, IntPtr.Zero, Path.GetDirectoryName(fileName), ref si, out pi)) {
                    return IntPtr.Zero;
                }

                return pi.hProcess;
            } finally {
                WinApi.CloseHandle(hShellProcessToken);
                WinApi.CloseHandle(hPrimaryToken);
                WinApi.CloseHandle(hShellProcess);
            }            
        }


        public static IntPtr GetThisAppHandle() {
            return Process.GetCurrentProcess().Handle;
        }

        public static bool IsThisAppAdmin() {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                      .IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static string GetApplicationDirectory() {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static string GetResourcesDirectory() {
            return Path.Combine(GetApplicationDirectory(), "Resources");
        }

        public static string GetImagesDirectory() {
            return Path.Combine(GetResourcesDirectory(), "Images");
        }

        public static string GetApplicationProcessPath() {
            try {
                var process = Process.GetCurrentProcess();
                return process.MainModule.FileName;
            } catch(Exception ex) {
                MpConsole.WriteLine("Error getting this application process path: " + ex.ToString());
                MpConsole.WriteLine("Attempting queryfullprocessimagename...");
                //return GetExecutablePathAboveVista(Process.GetCurrentProcess().Handle);
                return GetApplicationProcessPath();
            }
        }


        

        public static bool IsPathDirectory(string str) {
            // get the file attributes for file or directory
            return File.GetAttributes(str).HasFlag(FileAttributes.Directory);
        }

        
        #endregion

        #region Visual


        public static void PrintVisualTree(int depth, object obj) {
            // Print the object with preceding spaces that represent its depth
            Trace.WriteLine(new string(' ', depth) + obj.GetType().ToString());

            // If current element is a grid, display information about its rows and columns
            if (obj is Grid) {
                Grid gd = (Grid)obj;
                Trace.WriteLine(new string(' ', depth) + "Grid has " + gd.RowDefinitions.Count +
                                " rows and " + gd.ColumnDefinitions.Count + " columns.");
                foreach (UIElement element in gd.Children) {
                    Trace.WriteLine(new string(' ', depth) +
                        element.GetType().ToString() + " in row " + Grid.GetRow(element) +
                        " column " + Grid.GetColumn(element));
                }
            }

            // Recursive call for each visual child
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj as DependencyObject); i++)
                PrintVisualTree(depth + 1, VisualTreeHelper.GetChild(obj as DependencyObject, i));
        }



            

        public static IList<T> GetRandomizedList<T>(IList<T> orderedList) where T : class {
            var preRandomList = new List<T>();
            foreach (var c in orderedList) {
                preRandomList.Add(c);
            }
            var randomList = new List<T>();
            for (int i = 0; i < orderedList.Count; i++) {
                int randIdx = Rand.Next(0, preRandomList.Count - 1);
                var t = preRandomList[randIdx];
                preRandomList.RemoveAt(randIdx);
                randomList.Add(t);
            }
            return randomList;
        }

        

        public static Brush GetContentColor(int c, int r) {
            return MpThemeColors.Instance.ContentColors[c][r];
        }

        
        public static void SetColorChooserMenuItem(
            ContextMenu cm,
            MenuItem cmi,
            MouseButtonEventHandler selectedEventHandler) {
            var cmic = new Canvas();
            var _ContentColors = MpThemeColors.Instance.ContentColors; 
            double s = 15;
            double pad = 2.5;
            double w = (_ContentColors.Count * (s + pad)) + pad;
            double h = (_ContentColors[0].Count * (s + pad)) + pad;
            for (int x = 0; x < _ContentColors.Count; x++) {
                for (int y = 0; y < _ContentColors[0].Count; y++) {
                    Border b = new Border();
                    if(x == _ContentColors.Count -1 && y == _ContentColors[0].Count - 1) {
                        var addBmpSrc = (BitmapSource)new BitmapImage(new Uri(MpJsonPreferenceIO.Instance.AbsoluteResourcesPath + @"/Images/add2.png"));
                        b.Background = new ImageBrush(addBmpSrc);
                        MouseButtonEventHandler bMouseLeftButtonUp = (object o, MouseButtonEventArgs e3) => {
                            var result = new MpWpfCustomColorChooserMenu().ShowCustomColorMenu(MpWpfColorHelpers.GetRandomBrushColor().ToHex(),null);
                            if (result != null) {
                                b.Tag = result;
                            }
                        };
                        b.MouseLeftButtonUp += bMouseLeftButtonUp;

                        RoutedEventHandler bUnload = null;
                        bUnload = (object o, RoutedEventArgs e) =>{
                            b.MouseLeftButtonUp -= bMouseLeftButtonUp;
                            b.Unloaded -= bUnload;
                        };
                        b.Unloaded += bUnload;
                    } else {
                        b.Background = _ContentColors[x][y];
                        b.Tag = b.Background;
                    }
                    
                    b.BorderThickness = new Thickness(1.5);
                    b.BorderBrush = Brushes.DarkGray;
                    b.CornerRadius = new CornerRadius(2);
                    b.Width = b.Height = s;

                    MouseEventHandler bMouseEnter = (object o, MouseEventArgs e3) => {
                        b.BorderBrush = Brushes.DimGray;
                    };

                    MouseEventHandler bMouseLeave = (object o, MouseEventArgs e3) => {
                        b.BorderBrush = Brushes.DarkGray;
                    };
                    b.MouseEnter += bMouseEnter;

                    RoutedEventHandler bGotFocus = (object o, RoutedEventArgs e3) => {
                        b.BorderBrush = Brushes.DimGray;
                    };
                    b.GotFocus += bGotFocus;

                    b.MouseLeave += bMouseLeave;

                    b.MouseLeftButtonUp += selectedEventHandler;

                    RoutedEventHandler bUnloaded = null;
                    bUnloaded = (object o, RoutedEventArgs e3) => {
                        b.MouseEnter -= bMouseEnter;
                        b.MouseLeave -= bMouseLeave;
                        b.GotFocus -= bGotFocus;
                        b.MouseLeftButtonUp -= selectedEventHandler;
                        b.Unloaded -= bUnloaded;
                    };

                    b.Unloaded += bUnloaded;

                    RoutedEventHandler cmClosed = null;
                    cmClosed = (object o, RoutedEventArgs e3) => {
                        b.MouseEnter -= bMouseEnter;
                        b.MouseLeave -= bMouseLeave;
                        b.GotFocus -= bGotFocus;
                        b.MouseLeftButtonUp -= selectedEventHandler;
                        b.Unloaded -= bUnloaded;
                        cm.Closed -= cmClosed;
                    };
                    cm.Closed += cmClosed;

                    cmic.Children.Add(b);

                    Canvas.SetLeft(b, (x * (s + pad)) + pad);
                    Canvas.SetTop(b, (y * (s + pad)) + pad);
                }
            }
            cmic.Background = Brushes.Transparent;
            cmi.Header = cmic;
            cmi.Height = h;
            cmi.Style = (Style)Application.Current.MainWindow.FindResource("ColorPalleteMenuItemStyle");
            cm.Width = 300;
        }

        private static double sign(Point p1, Point p2, Point p3) {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }

        public static bool IsPointInTriangle(Point pt, Point v1, Point v2, Point v3) {
            double d1, d2, d3;
            bool has_neg, has_pos;

            d1 = sign(pt, v1, v2);
            d2 = sign(pt, v2, v3);
            d3 = sign(pt, v3, v1);

            has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);
            
            return !(has_neg && has_pos);
        }

        public static double DistanceBetweenPoints(Point a, Point b) {
            return Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2));
        }

        public static double DistanceBetweenValues(double a,double b) {
            return Math.Abs(Math.Abs(b) - Math.Abs(a));
        }
        public static DoubleAnimation AnimateDoubleProperty(
            double from = 0, 
            double to = 0, 
            double dt = 0, 
            object obj = null, 
            DependencyProperty property =  null, 
            EventHandler onCompleted = null) {
            DoubleAnimation animation = new DoubleAnimation();
            animation.From = from;
            animation.To = to;
            animation.Duration = new Duration(TimeSpan.FromMilliseconds(dt));

            CubicEase easing = new CubicEase();
            easing.EasingMode = EasingMode.EaseIn;
            animation.EasingFunction = easing;
            
            if(onCompleted != null) {
                animation.Completed += onCompleted;
            }
            if(obj.GetType() == typeof(List<FrameworkElement>)) {
                foreach(var fe in (List<FrameworkElement>)obj) {
                    if(fe == null) {
                        continue;
                    }
                    fe.BeginAnimation(property, animation);
                }
            } else {
                ((FrameworkElement)obj)?.BeginAnimation(property, animation);
            }

            return animation;
        }

        public static void AnimateVisibilityChange(
            object obj, 
            Visibility tv, 
            EventHandler onCompleted, 
            double ms = 1000, 
            double bt = 0) {
            var da = new DoubleAnimation {
                Duration = new Duration(TimeSpan.FromMilliseconds(ms))
            };
            var easing = new CubicEase();
            easing.EasingMode = EasingMode.EaseIn;
            da.EasingFunction = easing;

            da.Completed += (o, e) => {
                if (obj.GetType() == typeof(List<FrameworkElement>)) {
                    foreach (var fe in (List<FrameworkElement>)obj) {
                        if(fe == null) {
                            continue;
                        }
                        fe.Visibility = tv;
                    }
                } else if(obj != null) {
                    ((FrameworkElement)obj).Visibility = tv;
                }
            };
            if(onCompleted != null) {
                da.Completed += onCompleted;
            }
            
            da.From = tv == Visibility.Visible ? 0 : 1;
            da.To = tv == Visibility.Visible ? 1 : 0;
            da.BeginTime = TimeSpan.FromMilliseconds(bt);

            if (tv == Visibility.Visible) {
                if (obj.GetType() == typeof(List<FrameworkElement>)) {
                    foreach (var fe in (List<FrameworkElement>)obj) {
                        if (fe == null) {
                            continue;
                        }
                        fe.Opacity = 0;
                        fe.Visibility = Visibility.Visible;
                    }
                } else if(obj != null) {
                    ((FrameworkElement)obj).Opacity = 0;
                    ((FrameworkElement)obj).Visibility = Visibility.Visible;
                }
            }

            if (obj.GetType() == typeof(List<FrameworkElement>)) {
                foreach (var fe in (List<FrameworkElement>)obj) {
                    if(fe == null) {
                        continue;
                    }
                    fe.BeginAnimation(FrameworkElement.OpacityProperty, da);
                    if(onCompleted != null) {
                        // this ensures the oncompleted is only called ONCE for the items
                        da = da.Clone();
                        da.Completed -= onCompleted;
                    }
                }
            } else if(obj != null) {
                ((FrameworkElement)obj).BeginAnimation(FrameworkElement.OpacityProperty, da);
            }
        }

        public static Size MeasureText(string text, Typeface typeface, double fontSize) {
            var formattedText = new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize,
                Brushes.Black,
                new NumberSubstitution(),
                VisualTreeHelper.GetDpi(Application.Current.MainWindow).PixelsPerDip);

            return new Size(formattedText.Width, formattedText.Height);
        }

        #endregion

        #region Converters
        


        #endregion

        #region Http

        public static string ExecuteCurl(string curlCommand, int timeoutInSeconds = 60) {
            if (string.IsNullOrEmpty(curlCommand))
                return "";

            curlCommand = curlCommand.Trim();

            // remove the curl keworkd
            if (curlCommand.StartsWith("curl")) {
                curlCommand = curlCommand.Substring("curl".Length).Trim();
            }

            // this code only works on windows 10 or higher
            {

                curlCommand = curlCommand.Replace("--compressed", "");

                // windows 10 should contain this file
                var fullPath = System.IO.Path.Combine(Environment.SystemDirectory, "curl.exe");

                if (System.IO.File.Exists(fullPath) == false) {
                    if (Debugger.IsAttached) { Debugger.Break(); }
                    throw new Exception("Windows 10 or higher is required to run this application");
                }

                // on windows ' are not supported. For example: curl 'http://ublux.com' does not work and it needs to be replaced to curl "http://ublux.com"
                List<string> parameters = new List<string>();

                // separate parameters to escape quotes
                try {
                    Queue<char> q = new Queue<char>();

                    foreach (var c in curlCommand.ToCharArray()) {
                        q.Enqueue(c);
                    }

                    StringBuilder currentParameter = new StringBuilder();

                    void insertParameter() {
                        var temp = currentParameter.ToString().Trim();
                        if (string.IsNullOrEmpty(temp) == false) {
                            parameters.Add(temp);
                        }

                        currentParameter.Clear();
                    }

                    while (true) {
                        if (q.Count == 0) {
                            insertParameter();
                            break;
                        }

                        char x = q.Dequeue();

                        if (x == '\'') {
                            insertParameter();

                            // add until we find last '
                            while (true) {
                                x = q.Dequeue();

                                // if next 2 characetrs are \' 
                                if (x == '\\' && q.Count > 0 && q.Peek() == '\'') {
                                    currentParameter.Append('\'');
                                    q.Dequeue();
                                    continue;
                                }

                                if (x == '\'') {
                                    insertParameter();
                                    break;
                                }

                                currentParameter.Append(x);
                            }
                        } else if (x == '"') {
                            insertParameter();

                            // add until we find last "
                            while (true) {
                                x = q.Dequeue();

                                // if next 2 characetrs are \"
                                if (x == '\\' && q.Count > 0 && q.Peek() == '"') {
                                    currentParameter.Append('"');
                                    q.Dequeue();
                                    continue;
                                }

                                if (x == '"') {
                                    insertParameter();
                                    break;
                                }

                                currentParameter.Append(x);
                            }
                        } else {
                            currentParameter.Append(x);
                        }
                    }
                }
                catch {
                    if (Debugger.IsAttached) { Debugger.Break(); }
                    throw new Exception("Invalid curl command");
                }

                StringBuilder finalCommand = new StringBuilder();

                foreach (var p in parameters) {
                    if (p.StartsWith("-")) {
                        finalCommand.Append(p);
                        finalCommand.Append(" ");
                        continue;
                    }

                    var temp = p;

                    if (temp.Contains("\"")) {
                        temp = temp.Replace("\"", "\\\"");
                    }
                    if (temp.Contains("'")) {
                        temp = temp.Replace("'", "\\'");
                    }

                    finalCommand.Append($"\"{temp}\"");
                    finalCommand.Append(" ");
                }


                using (var proc = new Process {
                    StartInfo = new ProcessStartInfo {
                        FileName = "curl.exe",
                        Arguments = finalCommand.ToString(),
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WorkingDirectory = Environment.SystemDirectory
                    }
                }) {
                    proc.Start();

                    proc.WaitForExit(timeoutInSeconds * 1000);

                    return proc.StandardOutput.ReadToEnd();
                }
            }
        }


        public static void OpenUrl(string url, bool openInNewWindow = true) {
            if (url.StartsWith(@"http") && !openInNewWindow) {
                //WinApi.SetActiveWindow()
            } else {
                Process.Start(url);
            }
        }

        public static string CreateEmail(string fromAddress, string subject, object body, string attachmentPath = "") {
            //this returns the .eml file that will need to be deleted
            var mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(fromAddress);
            mailMessage.Subject = "Your subject here";
            if (body.GetType() == typeof(BitmapSource)) {
                mailMessage.IsBodyHtml = true;
                mailMessage.Body = string.Format("<img src='{0}'>", WriteBitmapSourceToFile(Path.GetTempPath(), (BitmapSource)body), true);
            } else {
                mailMessage.Body = (string)body;
            }

            if (!string.IsNullOrEmpty(attachmentPath)) {
                mailMessage.Attachments.Add(new Attachment(attachmentPath));
            }

            var filename = Path.GetTempPath() + "mymessage.eml";

            //save the MailMessage to the filesystem
            mailMessage.Save(filename);

            //Open the file with the default associated application registered on the local machine
            Process.Start(filename);
            return filename;
        }
        //public static string GetLocalIp4Address() {
        //    Ping ping = new Ping();
        //    var replay = ping.Send(Dns.GetHostName());

        //    if (replay.Status == IPStatus.Success) {
        //        return replay.Address.MapToIPv4().ToString();
        //    }
        //    return null;
        //}
        //public static string GetLocalIp4Address() {
        //    return MonkeyPaste.MpHelpers.GetLocalIp4Address();
        //    //string localIP;
        //    //using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0)) {
        //    //    socket.Connect("8.8.8.8", 65530);
        //    //    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
        //    //    localIP = endPoint.Address.ToString();
        //    //}
        //    //return localIP;
        //    //IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        //    //foreach (var ip in ipHostInfo.AddressList) { 
        //    //    if(ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal || ip.IsIPv6Multicast || ip.IsIPv6Teredo) {
        //    //        continue;
        //    //    }
        //    //    string a = ip.MapToIPv4().ToString();
        //    //    MpConsole.WriteLine(a);
        //    //}
        //    //IPAddress ipAddress = ipHostInfo.AddressList[ipHostInfo.AddressList.Length - 1];
        //    //if (ipAddress != null) {
        //    //    return ipAddress.MapToIPv4().ToString();
        //    //}
        //    //return "0.0.0.0";
        //}

        public static bool IsConnectedToInternet() {
            try {
                var client = new WebClient();
                var stream = client.OpenRead("http://www.google.com");
                bool isConnected = false;
                if(stream != null) {
                    isConnected = true;
                }
                stream?.Dispose();
                client?.Dispose();
                return isConnected;
            }
            catch (Exception e) {
                MpConsole.WriteLine(e.ToString());
                return false;
            }
        }


        public static BitmapSource ConvertUrlToQrCode(string url) {
            using (var qrGenerator = new QRCodeGenerator()) {
                using (var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q)) {
                    using (var qrCode = new QRCoder.PngByteQRCode(qrCodeData)) {
                        var qrCodeAsXaml = qrCode.GetGraphic(20);                        
                        //var bmpSrc= ConvertDrawingImageToBitmapSource(qrCodeAsXaml);
                        return qrCodeAsXaml.ToBitmapSource().Scale(new Size(0.2, 0.2));
                    }
                }
            }
        }

        

        #endregion

        #region private static Methods

        

        #endregion

        private static string[] _quillTags = new string[] {
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

        private static string[] _domainExtensions = new string[] {
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
    }
}
