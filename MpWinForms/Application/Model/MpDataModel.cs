using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWinFormsApp {
    public enum MpPalleteColorNames {
        LightBlue = 0,
        Blue,
        DarkGreen,
        LightGreen,
        Yellow,
        Red
    }
    
    public class MpDataModel : INotifyPropertyChanged   {
        public MpClipboardManager ClipboardManager { get; set; }

        public void ClearAllData() {
            ExcludedAppList = new ObservableCollection<MpApp>();
            SettingList = new ObservableCollection<MpSetting>();
            TagList = new ObservableCollection<MpTag>();
            CopyItemList = new ObservableCollection<MpCopyItem>();
        }
        public void LoadAllData(string dbPath, string dbPassword, string identityToken = null, string accessToken = null) {
            Db = new MpDb(dbPath, dbPassword, identityToken, accessToken);
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
                foreach(MpCopyItem ci in Db.GetCopyItems()) {
                    CopyItemList.Add(ci);
                }
            }
        }

        public MpDataModel() {
            ClearAllData();
            Db = new MpDb(); //inits not NoDb=true

            ClipboardManager = new MpClipboardManager();
            ClipboardManager.ClipboardChangedEvent += (s, ci) => {
                CopyItemList.Add(ci);
            };
        }

        public Color[] AppColors = new Color[] {
            Color.FromArgb(255, 215, 231, 237),
            Color.FromArgb(255, 36, 123, 160),
            Color.FromArgb(255, 112, 193, 179),
            Color.FromArgb(255, 243, 255, 189),
            Color.FromArgb(255, 255, 22, 84)
        };

        public MpDb Db { get; set; } = null;
        
        private ObservableCollection<MpCopyItem> _copyItemList = new ObservableCollection<MpCopyItem>();
        public ObservableCollection<MpCopyItem> CopyItemList {
            get {
                return _copyItemList;
            }
            set {
                if(_copyItemList != value) {
                    _copyItemList = value;
                    OnPropertyChanged("CopyItemList");
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
                    OnPropertyChanged("SettingList");
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
                    OnPropertyChanged("TagList");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            this.VerifyPropertyName(propertyName);
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if(handler != null) {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName) {
            // Verify that the property name matches a real, 
            // public, instance property on this object. 
            if(TypeDescriptor.GetProperties(this)[propertyName] == null) {
                string msg = "Invalid property name: " + propertyName;
                if(this.ThrowOnInvalidPropertyName)
                    throw new Exception(msg);
                else
                    Debug.Fail(msg);
            }
        }

        public bool ThrowOnInvalidPropertyName { get; private set; }

    }
}
