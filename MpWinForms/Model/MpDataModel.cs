using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public enum MpPalleteColorNames {
        LightBlue=0,
        Blue,
        DarkGreen,
        LightGreen,
        Yellow,
        Red
    }
    public class MpDataModel   {
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
                    _copyItemList.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => {

                    };
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
                    _excludedAppList.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => {

                    };
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
                    _settingList.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => {

                    };
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
                    _tagist.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => {

                    };
                }
            }
        }

        public MpDataModel() {
            ClearData();
            Db = new MpDb(); //inits not NoDb=true
        }
        private void ClearData() {
            CopyItemList = new ObservableCollection<MpCopyItem>();
            ExcludedAppList = new ObservableCollection<MpApp>();
            SettingList = new ObservableCollection<MpSetting>();
            TagList = new ObservableCollection<MpTag>();
        }
        public bool ConnectToDatabase(string dbPath, string dbPassword, string identityToken, string accessToken) {
            Db = new MpDb(dbPath, dbPassword, identityToken, accessToken);
            if(!Db.IsLoaded) {
                Db.NoDb = true;
                ClearData();
            } else {
                CopyItemList = new ObservableCollection<MpCopyItem>(Db.GetCopyItems() as List<MpCopyItem>);
                ExcludedAppList = new ObservableCollection<MpApp>(Db.GetExcludedAppList() as List<MpApp>);
                SettingList = new ObservableCollection<MpSetting>(Db.GetAppSettingList() as List<MpSetting>);
                TagList = new ObservableCollection<MpTag>(Db.GetTags() as List<MpTag>);
            }
            return Db.IsLoaded;
        }
    }
}
