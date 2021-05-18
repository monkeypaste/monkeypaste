using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using SQLite;
using System.Threading;
using SQLiteNetExtensions.Attributes;

namespace MonkeyPaste {
    public class MpUrlDomain : MpDbObject {
        private static List<MpUrlDomain> _AllUrlDomainList = null;
        public static int TotalUrlDomainCount = 0;

        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; } = 0;

        public string UrlDomainPath { get; set; } = string.Empty;
        public string UrlDomainTitle { get; set; } = string.Empty;

        public int IsRejected { get; set; } = 0;

        [Ignore]
        public bool IsUrlDomainRejected
        {
            get
            {
                return IsRejected == 1;
            }
            set
            {
                if (IsUrlDomainRejected != value)
                {
                    IsRejected = value ? 1 : 0;
                }
            }
        }

        [ForeignKey(typeof(MpIcon))]
        public int FavIconId { get; set; } = 0;

        [OneToOne]
        public MpIcon FavIcon { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<MpUrl> Urls { get; set; }

        #region Static Methods
        public static async Task<List<MpUrlDomain>> GetAllUrlDomains() {
            if(_AllUrlDomainList == null) {
                _AllUrlDomainList = await MpDb.Instance.ExecuteAsync<MpUrlDomain>("select * from MpUrlDomain", null);
            }
            return _AllUrlDomainList;
        }

        public static MpUrlDomain GetUrlDomainById(int Id) {
            if (_AllUrlDomainList == null) {
                GetAllUrlDomains();
            }
            var udbpl = _AllUrlDomainList.Where(x => x.Id == Id).ToList();
            if (udbpl.Count > 0) {
                return udbpl[0];
            }
            return null;
        }

        public static MpUrlDomain GetUrlDomainByPath(string urlDomain) {
            if(_AllUrlDomainList == null) {
                GetAllUrlDomains();
            }
            var udbpl = _AllUrlDomainList.Where(x => x.UrlDomainPath.ToLower().Contains(urlDomain)).ToList();
            if(udbpl.Count > 0) {
                return udbpl[0];
            }
            return null;
        }

        public static bool IsUrlDomainRejectedByHandle(string fullUrl) {
            return false;
            //string urlDomainPath = MpHelpers.Instance.GetUrlDomain(fullUrl);
            //foreach (MpUrlDomain urlDomain in GetAllUrlDomains()) {
            //    if (urlDomain.UrlDomainPath == urlDomainPath && urlDomain.IsUrlDomainRejected) {
            //        return true;
            //    }
            //}
            //return false;
        }
        public static async Task<List<MpUrlDomain>> GetAllRejectedUrlDomains() {
            await Task.Run(()=>Thread.Sleep(10));
            return new List<MpUrlDomain>(); 
            //return GetAllUrlDomains().Where(x => x.IsUrlDomainRejected == true).ToList();
        }
        #endregion

        public MpUrlDomain(string domainUrl, int favIconId, string domainTitle = "", bool isUrlDomainRejected = false) {
            UrlDomainPath = domainUrl;
            UrlDomainTitle = string.IsNullOrEmpty(domainTitle) ? UrlDomainPath : domainTitle;
            IsUrlDomainRejected = isUrlDomainRejected;
            FavIconId = favIconId;
        }

        public MpUrlDomain() { }
    }
}
