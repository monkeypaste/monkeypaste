using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions;
using SQLiteNetExtensions.Attributes;
using System.Linq;

namespace MonkeyPaste {
    [Table("MpCopyItemTag")]
    public class MpClipTag : MpDbModelBase {
        #region Columns
        [PrimaryKey, AutoIncrement]
        [Column("pk_MpCopyItemTagId")]
        public override int Id { get; set; }

        [Column("MpCopyItemTagGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Ignore]
        public Guid ClipTagGuid {
            get {
                if (string.IsNullOrEmpty(Guid)) {
                    return System.Guid.Empty;
                }
                return System.Guid.Parse(Guid);
            }
            set {
                Guid = value.ToString();
            }
        }

        [ForeignKey(typeof(MpTag))]
        [Column("fk_MpTagId")]
        public int TagId { get; set; }

        [ForeignKey(typeof(MpClip))]
        [Column("fk_MpCopyItemId")]
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

        public MpClipTag() : base(typeof(MpClipTag)) {
            ClipTagGuid = System.Guid.NewGuid();
        }
    }
}
