using MpWinFormsClassLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MpWpfApp {
    
    public class MpDataStore   {
        private static readonly Lazy<MpDataStore> lazy = new Lazy<MpDataStore>(() => new MpDataStore());
        private static Lazy<MpDataStore> _cfgSrc = new Lazy<MpDataStore>(System.Threading.LazyThreadSafetyMode.PublicationOnly);
        public static MpDataStore Instance { get { return lazy.Value; } }       

        public MpDb Db { get; set; } = null;
        public MpClipboardManager ClipboardManager { get; set; }

        //First call is in MpClipTray (for now)
        public MpDataStore() {      
            Db = new MpDb((string)MpRegistryHelper.Instance.GetValue("DBPath"), (string)MpRegistryHelper.Instance.GetValue("DBPassword"),string.Empty,string.Empty); //inits not NoDb=true 
            ClipboardManager = new MpClipboardManager();
            ClipboardManager.Init();
            ClipboardManager.ClipboardChangedEvent += () => {
                MpClip newClip = MpClip.CreateFromClipboard();
                var mainWindowViewModel = ((MpMainWindowViewModel)((MpMainWindow)Application.Current.MainWindow).DataContext);
                if(mainWindowViewModel.IsInAppendMode) {
                    //when in append mode just append the new items text to selecteditem
                    mainWindowViewModel.SelectedClipTiles[0].Text = mainWindowViewModel.SelectedClipTiles[0].CopyItem.Text + '\n' + newClip.Text;
                    return;
                }
                if(newClip != null) {
                    newClip.WriteToDatabase();
                    MpTag historyTag = new MpTag(1);
                    historyTag.LinkWithCopyItem(newClip);
                    ClipList.Insert(0, newClip);
                }
            };
        }
        public void Init() {
            ClearData();
            LoadAllData();
        }
        private void ClearData() {
            ExcludedAppList = new ObservableCollection<MpApp>();
            SettingList = new ObservableCollection<MpSetting>();
            TagList = new ObservableCollection<MpTag>();
            ClipList = new ObservableCollection<MpClip>();
        }
        private ObservableCollection<MpClip> _copyItemList = new ObservableCollection<MpClip>();
        public ObservableCollection<MpClip> ClipList {
            get {
                return _copyItemList;
            }
            set {
                if(_copyItemList != value) {
                    _copyItemList = value;
                   
                }
            }
        }

        private ObservableCollection<MpApp> _excludedAppList = new ObservableCollection<MpApp>();
        public ObservableCollection<MpApp> ExcludedAppList {
            get {
                return _excludedAppList;
            }
            set {
                if (_excludedAppList != value) {
                    _excludedAppList = value;
                }
            }
        }

        private ObservableCollection<MpSetting> _settingList = new ObservableCollection<MpSetting>();
        public ObservableCollection<MpSetting> SettingList {
            get {
                return _settingList;
            }
            set {
                if (_settingList != value) {
                    _settingList = value;
                }
            }
        }

        private ObservableCollection<MpTag> _tagist = new ObservableCollection<MpTag>();
        public ObservableCollection<MpTag> TagList {
            get {
                return _tagist;
            }
            set {
                if (_tagist != value) {
                    _tagist = value;
                }
            }
        }
        
        public void LoadAllData() {            
            if(!Db.IsLoaded) {
                Db.NoDb = true;
            } else {
                foreach(MpApp a in Db.GetExcludedAppList()) {
                    ExcludedAppList.Add(a);
                }
                foreach(MpSetting s in Db.GetAppSettingList()) {
                    SettingList.Add(s);
                }
                foreach(MpTag t in Db.GetTags()) {
                    TagList.Add(t);
                }
                foreach (MpClip ci in Db.GetCopyItems()) {
                    ClipList.Add(ci);
                }
            }
        }
        public Color[] AppColors = new Color[] {
            Color.FromArgb(255, 215, 231, 237),
            Color.FromArgb(255, 36, 123, 160),
            Color.FromArgb(255, 112, 193, 179),
            Color.FromArgb(255, 243, 255, 189),
            Color.FromArgb(255, 255, 22, 84)
        };
    }
    public enum MpPalleteColorNames {
        LightBlue = 0,
        Blue,
        DarkGreen,
        LightGreen,
        Yellow,
        Red
    }
}
