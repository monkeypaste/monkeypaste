using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpTagPropertyCollectionViewModel : MpViewModelBase<MpTagTileViewModel> {
        public ObservableCollection<MpTagPropertyViewModel> TagProperties { get; private set; } = new ObservableCollection<MpTagPropertyViewModel>();

        public MpTagPropertyCollectionViewModel() : base(null) { }

        public MpTagPropertyCollectionViewModel(MpTagTileViewModel parent) : base(parent) {
        }

        public async Task InitializeAsync(MpTag tag) {
            IsBusy = true;

            TagProperties.Clear();

            var tpl = await MpDataModelProvider.Instance.GetTagPropertiesById(tag.Id);
            foreach(var tp in tpl) {
                var tpvm = await CreateTagPropertyViewModel(tp);
                TagProperties.Add(tpvm);
            }
            IsBusy = false;
        }

        private async Task<MpTagPropertyViewModel> CreateTagPropertyViewModel(MpTagProperty tp) {
            MpTagPropertyViewModel tpvm = null;
            switch(tp.PropertyType) {
                case MpTagPropertyType.DirectoryWatcher:
                    tpvm = new MpDirectoryWatcherTagPropertyViewModel(this);
                    break;
            }
            await tpvm.InitializeAsync(tp);
            return tpvm;
        }

        #region Commands



        public ICommand ManagePropertiesCommand => new RelayCommand(
            () => {
            });

        public ICommand AddDirectoryPropertyToTagCommand => new RelayCommand(
            async() => {
                MpMainWindowViewModel.Instance.IsShowingDialog = true;
                using (System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog()) {
                    dlg.Description = "Select Directory";
                    dlg.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    dlg.ShowNewFolderButton = true;
                    System.Windows.Forms.DialogResult result = dlg.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK) {

                        var newTagProperty = new MpTagProperty() {
                            TagPropertyGuid = System.Guid.NewGuid(),
                            Tag = Parent.Tag,
                            TagId = Parent.TagId,
                            PropertyData = dlg.SelectedPath,
                            PropertyType = MpTagPropertyType.DirectoryWatcher
                        };

                        //await newTagProperty.WriteToDatabaseAsync();

                        var ntpvm = await CreateTagPropertyViewModel(newTagProperty);

                        TagProperties.Add(ntpvm);
                    }
                }
                MpMainWindowViewModel.Instance.IsShowingDialog = false;
            });
        #endregion
    }
}
