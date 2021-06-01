using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions;
using SQLiteNetExtensions.Attributes;
using System.Linq;

namespace MonkeyPaste {
    public class MpClipTag : MpDbModelBase {
        #region Columns
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        [ForeignKey(typeof(MpTag))]
        public int TagId { get; set; }

        [ForeignKey(typeof(MpClip))]
        public int ClipId { get; set; }

        public static async Task<List<MpClipTag>> GetAllClipsForTagId(int tagId) {
            var allClipTagList = await MpDb.Instance.GetItems<MpClipTag>();
            return allClipTagList.Where(x => x.TagId == tagId).ToList();
        }

        public static async Task DeleteAllClipTagsForClipId(int ClipId) {
            var allClipTagList = await MpDb.Instance.GetItems<MpClipTag>();
            var citl = allClipTagList.Where(x => x.ClipId == ClipId).ToList();
            foreach(var cit in citl) {
                await MpDb.Instance.DeleteItem<MpClipTag>(cit);
            }
        }

        public static async Task DeleteAllClipTagsForTagId(int tagId) {
            var allClipTagList = await MpDb.Instance.GetItems<MpClipTag>();
            var citl = allClipTagList.Where(x => x.TagId == tagId).ToList();
            foreach (var cit in citl) {
                await MpDb.Instance.DeleteItem<MpClipTag>(cit);
            }
        }

        #endregion

        public MpClipTag() : base(typeof(MpClipTag)) { }
    }
}
