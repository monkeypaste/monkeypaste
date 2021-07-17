using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpClipTagAssociatedItemViewModel : MpViewModelBase {
        #region Properties
        public MpClip Clip { get; set; }

        public MpTag Tag { get; set; }

        public Color TagColor { get; set; }

        public bool IsAssociated { get; set; }

        public bool IsEnabled {
            get {
                if(Tag == null) {
                    return false;
                }

                return Tag.Id != MpTag.AllTagId && Tag.Id != MpTag.RecentTagId;
            }
        }

        #endregion

        #region Public Methods
        public MpClipTagAssociatedItemViewModel() : base() { }

        public MpClipTagAssociatedItemViewModel(MpClip clip, MpTag tag) : this() {
            PropertyChanged += MpClipTagAssociatedItemViewModel_PropertyChanged;
            Clip = clip;
            Tag = tag;
            Task.Run(Initialize);
        }

        private async void MpClipTagAssociatedItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(Tag):
                    var c = await MpColor.GetColorById(Tag.ColorId);
                    TagColor = c.Color;
                    break;
            }
        }
        #endregion

        #region Private Methods
        private async Task Initialize() {
            await UpdateAssocation();

            Tag.ClipList = await MpClip.GetAllClipsByTagId(Tag.Id);
            //var c = await MpColor.GetColorById(Tag.ColorId);
            //TagColor = c.ColorBrush;
        }

        private async Task UpdateAssocation() {
            if (Clip == null || Tag == null) {
                return;
            }

            IsAssociated = await Tag.IsLinkedWithClipAsync(Clip);
        }
        #endregion

        #region Commands
        public ICommand ToggleAssociationCommand => new Command(
            async () => {
                if(IsAssociated) {
                    await Tag.UnlinkWithClipAsync(Clip);
                } else {
                    await Tag.LinkWithClipAsync(Clip);
                }
                await UpdateAssocation();
            },
            () => {
                return Clip != null && Tag != null && IsEnabled;
            });
        #endregion
    }
}
