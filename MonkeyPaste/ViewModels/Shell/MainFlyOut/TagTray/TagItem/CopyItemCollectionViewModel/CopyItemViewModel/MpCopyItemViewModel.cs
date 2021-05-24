using FFImageLoading.Forms;
using System;
using System.IO;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpCopyItemViewModel : MpViewModelBase {
        #region Private Variables
        public event EventHandler ItemStatusChanged;
        #endregion

        #region Properties
        public bool IsSelected { get; set; } = false;

        public MpCopyItem CopyItem { get; set; }

        public string StatusText { get; set; } = "Status";

        public bool IsFavorite {
            get {
                if(CopyItem == null) {
                    return false;
                }
                var favTagList = MpDb.Instance.QueryAsync<MpTag>("select * from MpTag where TagName=?", "Favorites").Result;

                if (favTagList != null && favTagList.Count > 0) {
                    var result = MpDb.Instance.QueryAsync<MpCopyItemTag>("select * from MpCopyItemTag where CopyItemId=? and TagId=?", CopyItem.Id, favTagList[0].Id).Result;
                    return result != null && result.Count > 0;
                }
                return false;
            }
        }

        public ImageSource IconImageSource {
            get {
                if(CopyItem == null || string.IsNullOrEmpty(CopyItem.Host)) {
                    return null;
                }
                var imgSrc = MpPackageNameSource.FromPackageName(CopyItem.Host);
                if(imgSrc == null) {
                    Console.WriteLine("Null icon for host: " + CopyItem.Host);
                }
                return imgSrc;
            }
        }
        #endregion

        #region Public Methods
        public MpCopyItemViewModel() { }

        public MpCopyItemViewModel(MpCopyItem item)
        {
            CopyItem = item;
        }
        #endregion

        #region Private Methods

        #endregion

        #region Commands

        public ICommand AddToFavoritesCommand => new Command(async () => {
            var favTagList = await MpDb.Instance.QueryAsync<MpTag>("select * from MpTag where TagName=?", "Favorites");

            if (favTagList != null) {
                foreach (var tag in favTagList) {
                    var copyItemTag = new MpCopyItemTag() {
                        CopyItemId = CopyItem.Id,
                        TagId = tag.Id
                    };
                    await MpDb.Instance.AddItem<MpCopyItemTag>(copyItemTag);
                }
            }
        });
        

        public ICommand Save => new Command(async () => {
            await MpDb.Instance.AddOrUpdate<MpCopyItem>(CopyItem);
            //wait Navigation.PopAsync();
        });

        private Command _setClipboardToItemCommand = null;
        public ICommand SetClipboardToItemCommand {
            get {
                if (_setClipboardToItemCommand == null) {
                    _setClipboardToItemCommand = new Command(SetClipboardToItem);
                }
                return _setClipboardToItemCommand;
            }
        }
        private void SetClipboardToItem() {
            Clipboard.SetTextAsync(CopyItem.ItemPlainText);
            ItemStatusChanged?.Invoke(this, new EventArgs());
        }
        #endregion
    }
}

