using MpWinFormsClassLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    class MpClipRepository {
        #region Fields

        private ObservableCollection<MpClip> _clips = new ObservableCollection<MpClip>();
        #endregion

        #region Constructor
        public MpClipRepository() {
            // TODO Add workflow to check for cred's
            _clips = LoadClips();
        }
        #endregion

        #region Public Interface
        /// <summary>
        /// Raised when a customer is placed into the repository.
        /// </summary>
        public event EventHandler<MpClipAddedEventArgs> ClipAdded;

        /// <summary>
        /// Places the specified Clip into the repository.
        /// If the Clip is already in the repository, an
        /// exception is not thrown.
        /// </summary>
        public void AddClip(MpClip clip) {
            if(clip == null)
                throw new ArgumentNullException("clip");

            if(!_clips.Contains(clip)) {
                _clips.Add(clip);

                if(this.ClipAdded != null)
                    this.ClipAdded(this, new MpClipAddedEventArgs(clip));
            }
        }

        /// <summary>
        /// Returns true if the specified customer exists in the
        /// repository, or false if it is not.
        /// </summary>
        public bool ContainsClip(MpClip customer) {
            if(customer == null)
                throw new ArgumentNullException("customer");

            return _clips.Contains(customer);
        }
        public ObservableCollection<MpClip> GetClips() {
            return new ObservableCollection<MpClip>(_clips);
        }
        #endregion

        #region Helpers
        static ObservableCollection<MpClip> LoadClips() {
            return new ObservableCollection<MpClip>(MpDataStore.Instance.Db.GetCopyItems((string)MpRegistryHelper.Instance.GetValue("DBPath"), (string)MpRegistryHelper.Instance.GetValue("DBPassword")) as List<MpClip>);
        }
        #endregion
    }
}
