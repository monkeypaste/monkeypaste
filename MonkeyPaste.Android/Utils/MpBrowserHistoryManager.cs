using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonkeyPaste.Plugin;

namespace MonkeyPaste.Droid {
    public class MpBrowserHistoryManager {
        #region Singleton
        private static readonly Lazy<MpBrowserHistoryManager> _Lazy = new Lazy<MpBrowserHistoryManager>(() => new MpBrowserHistoryManager());
        public static MpBrowserHistoryManager Instance { get { return _Lazy.Value; } }

        private MpBrowserHistoryManager() {
            //CreateConnection();
            //Init();
        }
        #endregion
        public void GetBrowserHistory() {
            var browserHistory = Browser.HistoryProjection;
            if(browserHistory != null) {
                MpConsole.WriteLine("Browser History:");
                foreach (var item in browserHistory) {
                    MpConsole.WriteLine(item);
                }
            }

            var browserTruncHistory = Browser.TruncateHistoryProjection;
            if (browserTruncHistory != null) {
                MpConsole.WriteLine("Browser Trunc History:");
                foreach (var item in browserTruncHistory) {
                    MpConsole.WriteLine(item);
                }
            }

            var urlCursor = Browser.GetAllVisitedUrls(MainActivity.Current.ContentResolver);
            urlCursor.MoveToFirst();
            if(urlCursor.MoveToFirst() && urlCursor.Count > 0) {
                bool canContinue = true;
                while (!urlCursor.IsAfterLast && canContinue) {
                    var title = urlCursor.GetString(urlCursor.GetColumnIndex(Browser.BookmarkColumns.Title));
                    var url = urlCursor.GetString(urlCursor.GetColumnIndex(Browser.BookmarkColumns.Url));

                    MpConsole.WriteLine($"Url: {url} title: {title} ");

                    urlCursor.MoveToNext();
                }
            }
            
            //String[] lProject = new string[] {
            //    Browser.BookmarkColumns.Title,
            //    Browser.BookmarkColumns.Url,
            //};
            //string lSelect = Browser.BookmarkColumns.Bookmark + " = 0";

            //var lItem = MainActivity.Current.ContentResolver.Query(Browser.BookmarksUri, lProject, lSelect, null, null);
            //lItem.MoveToFirst();

            //string title = string.Empty;
            //string url = string.Empty;

            //if (lItem.MoveToFirst() && lItem.Count > 0) {
            //    bool lContinue = true;
            //    while (lItem.IsAfterLast == false && lContinue) {
            //        title = lItem.GetString(lItem.GetColumnIndex(Browser.BookmarkColumns.Title));
            //        url = lItem.GetString(lItem.GetColumnIndex(Browser.BookmarkColumns.Url));

            //        mTitleList.Add(title);
            //        mURLList.Add(url);

            //        lItem.MoveToNext();
            //    }
            //}

            ////Add list items to UI            
            //mListView.Adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, mTitleList);

        }
    }
}