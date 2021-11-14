using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace MpWpfApp {
    public class MpQuickActionAnalyzerCollectionViewModel : MpSingletonViewModel<MpQuickActionAnalyzerCollectionViewModel> { 
        #region PrivateVariables
        private List<MpAnalyticItemPreset> _presets;
        #endregion

        #region Properties

        #region View Models
        public MpContentItemViewModel HostContentItemViewModel { get; set; }

        #endregion

        #endregion

        #region Constructors

        //public MpQuickActionAnalyzerCollectionViewModel() : base(null) { }

        //public MpQuickActionAnalyzerCollectionViewModel(MpAnalyticItemCollectionViewModel parent) : base(parent) { }

        public async Task Init() {
            var quickActionList = await MpDataModelProvider.Instance.GetAllQuickActionAnalyzers();
            var shortcutList = await MpDataModelProvider.Instance.GetAllShortcutAnalyzers();

            quickActionList.AddRange(shortcutList);
            _presets = quickActionList.Distinct().ToList();

            for (int i = 0; i < _presets.Count; i++) {
                _presets[i] = await MpDb.Instance.GetItemAsync<MpAnalyticItemPreset>(_presets[i].Id);
            }
        }
        #endregion

        #region Public Methods
        public ObservableCollection<MpContextMenuItemViewModel> GetQuickActionAnalyzerMenuItems() {            
            var tmil = new ObservableCollection<MpContextMenuItemViewModel>();
            if (MpClipTrayViewModel.Instance.SelectedItems.Count == 0) {
                return tmil;
            }

            MpCopyItemType itemType = MpClipTrayViewModel.Instance.SelectedItems[0].PrimaryItem.CopyItemType;

            foreach (var preset in _presets) {
                tmil.Add(
                    new MpContextMenuItemViewModel(
                        preset.Label,
                        MpClipTrayViewModel.Instance.AnalyzeSelectedItemCommand,
                        preset.Id,
                        null,
                        preset.Icon.IconImage.ImageBase64,
                        null,
                        (preset.Shortcut == null ? string.Empty:preset.Shortcut.KeyString),
                        null));
            }
            return tmil;
        }
        #endregion

        #region Protected Methods
        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if(e is MpShortcut s) {
                if(s.AnalyticItemPresetId > 0) {
                    MpHelpers.Instance.RunOnMainThread(async () => {
                        var aip = await MpDataModelProvider.Instance.GetAnalyzerPresetById(s.AnalyticItemPresetId);
                        if (aip != null) {
                            aip = await MpDb.Instance.GetItemAsync<MpAnalyticItemPreset>(aip.Id);
                            int aipIdx = _presets.IndexOf(_presets.FirstOrDefault(x=>x.Id == aip.Id));
                            if(aipIdx >= 0) {
                                _presets[aipIdx] = aip;
                            } else {
                                _presets.Add(aip);
                            }                            
                        }
                    });
                }
            }
        }

        protected override void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpAnalyticItemPreset aip) {
                MpHelpers.Instance.RunOnMainThread(async () => {
                    if (aip.IsQuickAction) {
                        aip = await MpDb.Instance.GetItemAsync<MpAnalyticItemPreset>(aip.Id);
                        var dupCheck = _presets.FirstOrDefault(x => x.Id == aip.Id);
                        if (dupCheck != null) {
                            _presets[_presets.IndexOf(dupCheck)] = aip;
                        } else {
                            _presets.Add(aip);
                        }
                    } else {
                        var dupCheck = _presets.FirstOrDefault(x => x.Id == aip.Id);
                        if(dupCheck != null) {
                            var shortcutActions = await MpDataModelProvider.Instance.GetAllShortcutAnalyzers();
                            dupCheck = shortcutActions.FirstOrDefault(x => x.Id == aip.Id);
                            if(dupCheck == null) {
                                int aipIdx = _presets.IndexOf(_presets.FirstOrDefault(x => x.Id == aip.Id));
                                if(aipIdx >= 0) {
                                    _presets.RemoveAt(aipIdx);
                                } 
                            }
                        }
                    }
                });
            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpAnalyticItemPreset aip) {
                MpHelpers.Instance.RunOnMainThread(async () => {
                    int aipIdx = _presets.IndexOf(_presets.FirstOrDefault(x => x.Id == aip.Id));
                    if (aipIdx >= 0) {
                        _presets.RemoveAt(aipIdx);
                    }
                });
            } else if (e is MpShortcut s) {
                if (s.AnalyticItemPresetId > 0) {
                    MpHelpers.Instance.RunOnMainThread(async () => {
                        int aipIdx = _presets.IndexOf(_presets.FirstOrDefault(x => x.Id == s.AnalyticItemPresetId));
                        if (aipIdx >= 0) {
                            _presets.RemoveAt(aipIdx);
                        }
                    });
                }
            }
        }
        #endregion
    }
}
