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
            Clip = clip;
            Tag = tag;
            Task.Run(Initialize);
        }
        #endregion

        #region Private Methods
        private async Task Initialize() {
            await UpdateAssocation();

            Tag.ClipList = await MpClip.GetAllClipsByTagId(Tag.Id);
            Tag.TagColor = await MpColor.GetColorById(Tag.ColorId);
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
