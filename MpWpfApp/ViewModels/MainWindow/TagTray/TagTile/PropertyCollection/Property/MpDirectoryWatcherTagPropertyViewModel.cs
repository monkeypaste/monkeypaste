using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpDirectoryWatcherTagPropertyViewModel : MpTagPropertyViewModel, MpIFileSystemEventHandler {
        #region Properties

        #region State

        public bool IsNew {
            get {
                if(TagProperty == null) {
                    return false;
                }
                return TagProperty.Id == 0;
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpDirectoryWatcherTagPropertyViewModel(MpTagPropertyCollectionViewModel parent) : base(parent) { }

        #endregion

        #region Protected Methods

        public override async Task InitializeAsync(MpTagProperty tp) {
            await base.InitializeAsync(tp);

            MpFileSystemWatcher.Instance.AddWatcher(tp.PropertyData,this);
            if(IsNew) {
                MpCopyItem fci = await CreateFolderItem(tp.PropertyData,true);

                await tp.WriteToDatabaseAsync();
            }
        }

        public async Task<MpCopyItem> CreateFolderItem(string path, bool recursive = false, MpCopyItem parent = null) {
            if(!File.Exists(path) && !Directory.Exists(path)) {
                throw new Exception($"{path} does not exist");
            }

            MpCopyItem rootItem = new MpCopyItem() {
                CopyItemGuid = System.Guid.NewGuid(),
                CopyDateTime = DateTime.Now,
                CopyCount = 1,
                ItemColor = MpHelpers.Instance.GetRandomColor().ToHex(),
                Title = path,
                ItemType = MpCopyItemType.FileList,
                ItemData = path,
                Source = MpPreferences.Instance.ThisAppSource,
                SourceId = MpPreferences.Instance.ThisAppSource.Id,
                CompositeParentCopyItemId = parent == null ? 0 : parent.Id,
                CompositeSortOrderIdx = parent == null ? 0 : parent.CompositeSortOrderIdx + 1
            };

            await rootItem.WriteToDatabaseAsync();

            await MpCopyItemTag.Create(Parent.Parent.TagId, rootItem.Id);                       

            if(Directory.Exists(path)) {
                string[] dirFiles = null;
                try {
                    dirFiles = Directory.GetFiles(path);
                }
                catch(Exception ex) {
                    MpConsole.WriteTraceLine(ex);
                    dirFiles = new string[] { };
                }

                for (int i = 0; i < dirFiles.Length; i++) {
                    string fileOrPath =dirFiles[i];

                    if (recursive || parent == null) {
                        await CreateFolderItem(fileOrPath, recursive, rootItem);
                    }
                }
            }

            MpConsole.WriteLine($"Folder item {path} created parent: {rootItem.CompositeParentCopyItemId} sort: {rootItem.CompositeSortOrderIdx}");
            return rootItem;
        }
        #endregion

        #region Private Methods

        #endregion

        #region MpIFileSystemEventHandler Implementation

        public void OnChanged(object sender, FileSystemEventArgs e) {
            
        }

        public void OnCreated(object sender, FileSystemEventArgs e) {
            
        }

        public void OnDeleted(object sender, FileSystemEventArgs e) {
            
        }

        public void OnRenamed(object sender, RenamedEventArgs e) {
            
        }

        public void OnError(object sender, ErrorEventArgs e) {
            
        }

        #endregion

        #region Commands


        public ICommand OpenPathCommand => new RelayCommand<object>(
            (args) => {
                try {
                    Process.Start(args as string);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine(ex);
                }
            },
            (args) => args != null);

        #endregion
    }
}
