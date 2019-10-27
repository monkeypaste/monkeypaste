using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MonkeyPaste {
    public class MpData {

        private MpClient _client;
        private MpUser _user;

        public MpDb Db { get; set; }

        private ObservableCollection<MpPasteHistory> _pasteList = new ObservableCollection<MpPasteHistory>();
        private ObservableCollection<MpIcon> _iconList = new ObservableCollection<MpIcon>();
        private ObservableCollection<MpApp> _appList = new ObservableCollection<MpApp>();
        private ObservableCollection<MpCopyItem> _copyItemList = new ObservableCollection<MpCopyItem>();

        private ObservableCollection<string> _searchStringList = new ObservableCollection<string>();

        public MpData(string dbPath,string dbPassword,string identityToken,string accessToken) {
            Db = new MpDb(dbPath,dbPassword);
            InitUser(identityToken);
            InitClient(accessToken);            
        }
        public void Init() {            
            Db.InitDb();
            InitMpIcon();
            InitMpApp();
            InitMpCopyItem();
            InitPasteHistory();
            InitSearchString();
        }
        public bool Load(string path,string password,bool isNew) {
            ResetData();
            return true;
        }
        public void AddOnDataListChangeListener(MpTileChooserPanelController lf) {
            //_mpIconList.CollectionChanged += lf.CopyItemCollection_CollectionChanged;
            //_mpAppList.CollectionChanged += lf.CopyItemCollection_CollectionChanged;
            _searchStringList.CollectionChanged += lf.SearchStrCollection_CollectionChanged;
            _copyItemList.CollectionChanged += lf.CopyItemCollection_CollectionChanged;
        }
        public MpClient GetMpClient() {
            return _client;
        }
        private void InitSearchString() {
            _searchStringList = new ObservableCollection<string>();
        }
        private void InitPasteHistory() {
            _pasteList = new ObservableCollection<MpPasteHistory>();

            if(Db.NoDb) {
                DataTable dt = Db.Execute("select * from MpPasteHistory");
                if(dt != null) {
                    foreach(DataRow dr in dt.Rows) {
                        _pasteList.Add(new MpPasteHistory(dr));
                    }
                }
                Console.WriteLine("Init w/ " + _pasteList.Count + " pasted history items.");
            }
        }
        private void InitUser(string idToken) {
            _user = new MpUser() { IdentityToken = idToken };
        }
        private void InitClient(string accessToken) {
            _client = new MpClient() { AccessToken = accessToken };
        }
        
        private void InitMpIcon() {
            _iconList = new ObservableCollection<MpIcon>();

            if(!Db.NoDb) {
                DataTable dt = Db.Execute("select * from MpIcon");
                if(dt != null) {
                    foreach(DataRow dr in dt.Rows) {
                        _iconList.Add(new MpIcon(dr));
                    }
                }
                Console.WriteLine("Init w/ " + _iconList.Count + " icons.");
            }
            
        }
        public void InitMpApp() {
            _appList = new ObservableCollection<MpApp>();

            if(!Db.NoDb) {
                DataTable dt = Db.Execute("select * from MpApp");
                if(dt != null) {
                    foreach(DataRow dr in dt.Rows) {
                        _appList.Add(new MpApp(dr));
                    }
                }
                Console.WriteLine("Init w/ " + _appList.Count + " apps.");
            }
        }
        public void InitMpCopyItem() {
            _copyItemList = new ObservableCollection<MpCopyItem>();

            if(!Db.NoDb) {
                DataTable dt = Db.Execute("select * from MpCopyItem");
                if(dt != null) {
                    foreach(DataRow dr in dt.Rows) {
                        _copyItemList.Add(new MpCopyItem(dr));
                    }
                }

                Console.WriteLine("Init w/ " + _copyItemList.Count + " copyitems added");
            }
        }
       public void AddPasteHistory(MpPasteHistory newPasteHistory) {
            foreach(MpPasteHistory ph in _pasteList) {
                if(ph.CopyItemId == newPasteHistory.CopyItemId && ph.PasteDateTime == newPasteHistory.PasteDateTime) {
                    _pasteList[_pasteList.IndexOf(ph)] = newPasteHistory;
                    return;
                }
            }
            _pasteList.Add(newPasteHistory);
        }
        public void AddMpIcon(IntPtr sourceHandle) {
            int lastId = 0;
            if(_iconList.Count > 0) {
                lastId = _iconList[_iconList.Count - 1].iconId;
            }
            MpIcon newMpIcon = new MpIcon(0,sourceHandle);
            if(newMpIcon.iconId > lastId) {
                _iconList.Add(newMpIcon);
            }
        }
        public void AddMpIcon(MpIcon newIcon) {
            foreach(MpIcon i in _iconList) {
                if(i.iconId == newIcon.iconId) {
                    _iconList[_iconList.IndexOf(i)] = newIcon;
                    return;
                }
            }
            _iconList.Add(newIcon);
        }
        public void AddMpApp(IntPtr sourceHandle) {
            int lastId = 0;
            if(_appList.Count > 0) {
                lastId = _appList[_appList.Count - 1].appId;
            }
            MpApp newMpApp = new MpApp(0,0,sourceHandle,false);
            if(newMpApp.appId > lastId) {
                _appList.Add(newMpApp);
            }
            AddMpIcon(newMpApp.Icon);
        }
        public void AddMpApp(MpApp newApp) {
            foreach(MpApp a in _appList) {
                if(a.appId == newApp.appId) {
                    _appList[_appList.IndexOf(a)] = newApp;
                    AddMpIcon(newApp.Icon);
                    return;
                }
            }
            AddMpIcon(newApp.Icon);
            _appList.Add(newApp);
        }
        public void AddMpCopyItem(IDataObject iData,IntPtr sourceHandle) {
            MpCopyItem ci = null;
           if(iData.GetDataPresent(DataFormats.Bitmap)) {
                ci = new MpCopyItem((Image)iData.GetData(DataFormats.Bitmap,true),sourceHandle);
            }
            else if(iData.GetDataPresent(DataFormats.FileDrop)) {
                ci = new MpCopyItem((string[])iData.GetData(DataFormats.FileDrop,true),sourceHandle);
            }
            else if(iData.GetDataPresent(DataFormats.Rtf)) {
                ci = new MpCopyItem((string)iData.GetData(DataFormats.Rtf),MpCopyItemType.RichText,sourceHandle);
            }
            else if(iData.GetDataPresent(DataFormats.Html)) {
                ci = new MpCopyItem((string)iData.GetData(DataFormats.Html),MpCopyItemType.HTMLText,sourceHandle);
            }
            else if(iData.GetDataPresent(DataFormats.Text)) {
                ci = new MpCopyItem((string)iData.GetData(DataFormats.Text),MpCopyItemType.Text,sourceHandle);
            }
            else {
                Console.WriteLine("MpData error clipboard data is not known format");
                return;
            }
           //if ci is a duplicate ignore it
           if(ci.copyItemId < 0) {
                return;
            }

            AddMpApp(ci.App);
            foreach(MpCopyItem mpci in _copyItemList) {
                if(ci.copyItemId == mpci.copyItemId) {
                    UpdateMpCopyItem(ci);
                    return;
                }
            }
            _copyItemList.Add(ci);
        }
        
        public void UpdateMpIcon(MpIcon updatedMpIcon) {
            for(int i = 0;i < _iconList.Count;i++) {
                if(_iconList[i].iconId == updatedMpIcon.iconId) {
                    _iconList[i] = updatedMpIcon;
                    _iconList[i].WriteToDatabase();
                    break;
                }
            }
        }
        public void UpdateMpApp(MpApp updatedMpApp) {
            for(int i = 0;i < _appList.Count;i++) {
                if(_appList[i].appId == updatedMpApp.appId) {
                    _appList[i] = updatedMpApp;
                    _appList[i].WriteToDatabase();
                    break;
                }
            }
        }
        public void UpdateMpCopyItem(MpCopyItem updatedMpCopyItem) {
            for(int i = 0;i < _copyItemList.Count;i++) {
                if(_copyItemList[i].copyItemId == updatedMpCopyItem.copyItemId) {
                    _copyItemList.Remove(_copyItemList[i]);
                    _copyItemList.Insert(i,updatedMpCopyItem);
                    updatedMpCopyItem.WriteToDatabase();
                    break;
                }
            }
        }
        public void UpdateSearchString(string newStr) {
            string temp = string.Empty;
            if(_searchStringList.Count > 0) {
                temp = _searchStringList[0];
            }
            temp += newStr;
            _searchStringList.Clear();
            _searchStringList.Add(temp);
        }
        public MpPasteHistory GetPasteHistory(int MpPasteHistoryId) {
            foreach(MpPasteHistory ph in _pasteList) {
                if(ph.PasteHistoryId == MpPasteHistoryId) {
                    return ph;
                }
            }
            return null;
        }
        public List<MpPasteHistory> GetPasteHistoryList(int MpCopyItemId) {
            List<MpPasteHistory> pl = new List<MpPasteHistory>();
            foreach(MpPasteHistory ph in _pasteList) {
                if(ph.CopyItemId == MpCopyItemId) {
                    pl.Add(ph);
                }
            }
            return pl;
        }
        public MpIcon GetMpIcon(int MpIconId) {
            foreach(MpIcon mpi in _iconList) {
                if(mpi.iconId == MpIconId) {
                    return mpi;
                }
            }
            return null;
        }
        public MpIcon GetMpIcon(Image iconImage) {
            foreach(MpIcon mpi in _iconList) {
                if(mpi.IconImage == iconImage) {
                    return mpi;
                }
            }
            return null;
        }
        public MpApp GetMpApp(int MpAppId) {
            foreach(MpApp mpa in _appList) {
                if(mpa.appId == MpAppId) {
                    return mpa;
                }
            }
            return null;
        }
        public MpApp GetMpApp(string path) {
            foreach(MpApp mpa in _appList) {
                if(mpa.SourcePath == path) {
                    return mpa;
                }
            }
            return null;
        }
        public MpCopyItem[] GetMpCopyItemList() {
            MpCopyItem[] tempList = new MpCopyItem[_copyItemList.Count];
            _copyItemList.CopyTo(tempList,0);
            return tempList;
        }
        public MpCopyItem GetMpCopyItem(int MpCopyItemId) {
            foreach(MpCopyItem mpci in _copyItemList) {
                if(mpci.copyItemId == MpCopyItemId) {
                    return mpci;
                }
            }
            return null;
        }
        public string GetSearchString() {
            if(_searchStringList.Count > 0) {
                return _searchStringList[0];
            }
            return string.Empty;
        }
        public void ResetData() {
            Db.ResetDb();
            _iconList.Clear();
            _copyItemList.Clear();
            _appList.Clear();
            _pasteList.Clear();
            _searchStringList.Clear();
        }
    }
}
