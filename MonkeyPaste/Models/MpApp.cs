﻿using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using SQLiteNetExtensions.Attributes;
using System.Threading.Tasks;
using System.Linq;

namespace MonkeyPaste {
    public class MpApp : MpDbObject, MpICopyItemSource {
        private static List<MpApp> _AllApps = null;
        public override int Id { get; set; } = 0;
        
        [Indexed]
        public string AppPath { get; set; } = string.Empty;
        
        public string AppName { get; set; } = string.Empty;

        public int IsRejected { get; set; } = 0;

        [Ignore]
        public bool IsAppRejected
        {
            get
            {
                return IsRejected == 1;
            }
            set
            {
                if(IsAppRejected != value)
                {
                    IsRejected = value ? 1 : 0;
                }
            }
        }

        [ForeignKey(typeof(MpIcon))]
        public int IconId { get; set; }

        [OneToOne]
        public MpIcon Icon { get; set; }

        public static async Task<List<MpApp>> GetAllApps() {
            if(_AllApps == null) {
                _AllApps = await MpDb.Instance.GetItems<MpApp>();
            }
            return _AllApps;
        }
        public static async Task<MpApp> GetAppByPath(string appPath) {
            var allApps = await GetAllApps();
            return allApps.Where(x => x.AppPath.ToLower() == appPath.ToLower()).FirstOrDefault();
        }

        public static async Task<MpApp> GetAppById(int appId) {
            var allApps = await GetAllApps();
            return allApps.Where(x => x.Id == appId).FirstOrDefault();
        }

        public static async Task<MpApp> Create(string appPath,string appName,byte[] appIcon) {
            //if app doesn't exist create image,icon,app and source

            var newIcon = await MpIcon.Create(appIcon);

            var newApp = new MpApp() {
                AppPath = appPath,
                AppName = appName,
                IconId = newIcon.Id,
                Icon = newIcon
            };

            await MpDb.Instance.AddItem<MpApp>(newApp);

            return newApp;
        }
        public MpApp() : base(typeof(MpApp)) { }

        #region MpICopyItemSource Implementation
        public MpIcon SourceIcon => Icon;

        public string SourcePath => AppPath;

        public string SourceName => AppName;

        public int RootId => Id;

        public bool IsSubSource => false;
        #endregion
    }
}