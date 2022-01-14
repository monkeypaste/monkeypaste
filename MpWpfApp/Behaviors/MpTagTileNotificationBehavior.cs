using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace MpWpfApp {
    public class MpTagTileNotificationBehavior : MpBehavior<MpTagTileView> {
        #region Private Variables

        private ObservableCollection<int> _copyItemIdsNeedingView = new ObservableCollection<int>();

        private AdornerLayer _adornerLayer;

        #endregion

        #region Properties

        public MpNotificationAdorner NotificationAdorner { get; set; }

        #endregion

        #region Constructors

        protected override void OnLoad() {
            base.OnLoad();

            AssociatedObject.BindingContext.OnCopyItemLinked += BindingContext_OnCopyItemLinked;
            AssociatedObject.BindingContext.OnCopyItemUnlinked += BindingContext_OnCopyItemUnlinked;

            _copyItemIdsNeedingView.CollectionChanged += _copyItemIdsNeedingView_CollectionChanged;

            MpMessenger.Instance.Register<MpMessageType>(
                    MpClipTrayViewModel.Instance,
                    ReceivedClipTrayViewModelMessage);
        }

        protected override void OnUnload() {
            base.OnUnload();

            AssociatedObject.BindingContext.OnCopyItemLinked -= BindingContext_OnCopyItemLinked;
            AssociatedObject.BindingContext.OnCopyItemUnlinked -= BindingContext_OnCopyItemUnlinked;

            _copyItemIdsNeedingView.CollectionChanged -= _copyItemIdsNeedingView_CollectionChanged;

            MpMessenger.Instance.Unregister<MpMessageType>(
                    MpClipTrayViewModel.Instance,
                    ReceivedClipTrayViewModelMessage);
        }

        #endregion

        #region Public Methods

        public void InitAdorner() {
            NotificationAdorner = new MpNotificationAdorner(AssociatedObject.TagCountBorder);
            _adornerLayer = AdornerLayer.GetAdornerLayer(AssociatedObject);
            _adornerLayer.Add(NotificationAdorner);

        }

        public void UpdateAdorner() {
            if (_adornerLayer == null) {
                InitAdorner();
            }
            _adornerLayer?.Update();
        }

        #endregion

        #region Private Methods

        private void UpdateNotifier() {
            var idsSeen = new List<int>();
            foreach (int ciid in _copyItemIdsNeedingView) {
                var civm = MpClipTrayViewModel.Instance.GetContentItemViewModelById(ciid);
                if (civm != null && civm.IsVisible && AssociatedObject.BindingContext.IsSelected) {
                    idsSeen.Add(ciid);
                }
            }
            int idsToRemoveCount = idsSeen.Count;
            while (idsToRemoveCount > 0) {
                _copyItemIdsNeedingView.Remove(idsSeen[idsToRemoveCount - 1]);
                idsToRemoveCount--;
            }

            AssociatedObject.BindingContext.HasNotification = _copyItemIdsNeedingView.Count > 0;
            UpdateAdorner();
        }

        private void _copyItemIdsNeedingView_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            MpHelpers.Instance.RunOnMainThread(UpdateNotifier);
        }

        private void BindingContext_OnCopyItemLinked(object sender, int e) {
            // NOTE triggered from tag tile OnItemAdded db event for MpCopyItemTag
            if(_copyItemIdsNeedingView.Contains(e)) {
                return;
            }
            _copyItemIdsNeedingView.Add(e);
        }

        private void BindingContext_OnCopyItemUnlinked(object sender, int e) {
            // NOTE triggered from tag tile OnItemDeleted db event for MpCopyItemTag
            if (_copyItemIdsNeedingView.Contains(e)) {
                _copyItemIdsNeedingView.Remove(e);
            }            
        }

        private void ReceivedClipTrayViewModelMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.TrayScrollChanged:
                case MpMessageType.RequeryCompleted:
                case MpMessageType.JumpToIdxCompleted:
                    MpHelpers.Instance.RunOnMainThread(UpdateNotifier);
                    break;
            }
        }

        #endregion
    }
}
