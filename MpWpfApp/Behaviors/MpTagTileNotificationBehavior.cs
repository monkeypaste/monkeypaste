﻿using MonkeyPaste;
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

            MpMessenger.Register<MpMessageType>(
                    MpClipTrayViewModel.Instance,
                    ReceivedClipTrayViewModelMessage);
        }

        protected override void OnUnload() {
            base.OnUnload();

            AssociatedObject.BindingContext.OnCopyItemLinked -= BindingContext_OnCopyItemLinked;
            AssociatedObject.BindingContext.OnCopyItemUnlinked -= BindingContext_OnCopyItemUnlinked;

            _copyItemIdsNeedingView.CollectionChanged -= _copyItemIdsNeedingView_CollectionChanged;

            MpMessenger.Unregister<MpMessageType>(
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
            foreach (int ciid in _copyItemIdsNeedingView.ToList()) {
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
            MpHelpers.RunOnMainThread(UpdateNotifier);
        }

        private void BindingContext_OnCopyItemLinked(object sender, MpCopyItem ci) {
            // NOTE triggered from tag tile OnItemAdded db event for MpCopyItemTag
            if(_copyItemIdsNeedingView.Contains(ci.Id)) {
                return;
            }
            _copyItemIdsNeedingView.Add(ci.Id);
        }

        private void BindingContext_OnCopyItemUnlinked(object sender, MpCopyItem ci) {
            // NOTE triggered from tag tile OnItemDeleted db event for MpCopyItemTag
            if (_copyItemIdsNeedingView.Contains(ci.Id)) {
                _copyItemIdsNeedingView.Remove(ci.Id);
            }            
        }

        private void ReceivedClipTrayViewModelMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.TrayScrollChanged:
                case MpMessageType.RequeryCompleted:
                case MpMessageType.JumpToIdxCompleted:
                    MpHelpers.RunOnMainThread(UpdateNotifier);
                    break;
            }
        }

        #endregion
    }
}