using MonkeyPaste;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpMasterTemplateModelCollectionViewModel : 
        MpViewModelBase, 
        MpIMenuItemViewModel,
        MpISingletonViewModel<MpMasterTemplateModelCollectionViewModel> {
        #region Statics

        private static MpMasterTemplateModelCollectionViewModel _instance;
        public static MpMasterTemplateModelCollectionViewModel Instance => _instance ?? (_instance = new MpMasterTemplateModelCollectionViewModel());
        #endregion

        #region Properties

        #region View Models

        public MpMenuItemViewModel MenuItemViewModel {
            get {
                var mivm = new MpMenuItemViewModel();
                //mivm.SubItems = new List<MpMenuItemViewModel>() {
                //    new MpMenuItemViewModel() {
                //        Header = "Date & Time",
                //        SubItems = new List<MpMenuItemViewModel>() {

                //        }
                //    }
                //};
                //foreach (var tvm in Items) {
                //    var tvm_mi = new MpMenuItemViewModel() {
                //        Header = tvm.TemplateName,
                //        IconHexStr = tvm.TemplateHexColor,
                //        Command = CreateTemplateViewModelCommand,
                //        CommandParameter = tvm
                //    };
                //    mivm.SubItems.Add(tvm_mi);
                //}
                //var ntvm_mi = new MpMenuItemViewModel() {
                //    Header = "Add New",
                //    IconResourceKey = Application.Current.Resources["AddIcon"] as string,
                //    Command = CreateTemplateViewModelCommand
                //};
                //mivm.SubItems.Add(ntvm_mi);
                return mivm;
            }
        }

        #endregion

        #region Model

        public ObservableCollection<MpTextTemplate> AllTemplates { get; set; } = new ObservableCollection<MpTextTemplate>();

        #endregion

        #endregion

        #region Constructors

        public MpMasterTemplateModelCollectionViewModel() : base(null) { }

        #endregion

        #region Public Methods

        public async Task Init() {
            var ttl = await MpDb.GetItemsAsync<MpTextTemplate>();
            AllTemplates = new ObservableCollection<MpTextTemplate>(ttl);
        }

        public async Task Update(List<MpTextTemplate> updatedTemplates, List<string> removedGuids) {            
            if(removedGuids != null) {
                var rtl = await MpDataModelProvider.GetTextTemplatesByGuids(removedGuids);
                await Task.WhenAll(rtl.Select(x => x.DeleteFromDatabaseAsync()));
            }

            if(updatedTemplates != null) {
                await Task.WhenAll(updatedTemplates.Select(x => x.WriteToDatabaseAsync()));
                AllTemplates = new ObservableCollection<MpTextTemplate>(updatedTemplates);
            } else {
                // NOTE not sure how to handle this..
                await Init();
            }
        }

        #endregion
    }
}
