﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions;
using SQLiteNetExtensions.Attributes;
using System.Linq;

namespace MonkeyPaste {
    public class MpCopyItemTemplate : MpDbObject {
        private static List<MpCopyItemTag> _AllCopyItemTagList = null;
        #region Columns
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        [ForeignKey(typeof(MpCopyItem))]
        public int CopyItemId { get; set; }

        [ForeignKey(typeof(MpColor))]
        public int ColorId { get; set; }

        public string TemplateName { get; set; }

        [ManyToOne]
        public MpColor Color { get; set; }
        #endregion

        public MpCopyItemTemplate() : base(typeof(MpCopyItemTemplate)) { }
    }
}