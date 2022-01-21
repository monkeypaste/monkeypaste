using System;
using System.Collections.Generic;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace MonkeyPaste {
    public class MpPasteToAppPath : MpDbModelBase {
        #region Columns
        [PrimaryKey, AutoIncrement]
        [Column("pk_MpPasteToAppPathId")]
        public override int Id { get; set; }

        [Column("MpPasteToAppPathGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }
              
        public string AppPath { get; set; }
        public string AppName { get; set; }

        [Column("IsAdmin")]
        public int Admin { get; set; }        

        [Column("IsSilent")]
        public int Silent { get; set; }        

        [Column("PressEnter")]
        public int Enter { get; set; }
        
        public string Args { get; set; }
        public string Label { get; set; }

        [Column("fk_MpDbImageId")]
        [ForeignKey(typeof(MpDbImage))]
        public int AvatarId { get; set; }

        public int WindowState { get; set; }

        #endregion

        #region Fk Models

        [OneToOne(CascadeOperations = CascadeOperation.All)]
        public MpDbImage AvatarDbImage { get; set; }
        #endregion

        #region Properties

        [Ignore]
        public Guid PasteToAppPathGuid {
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

        [Ignore]
        public int PasteToAppPathId {
            get {
                return Id;
            }
            set {
                Id = value;
            }
        }

        [Ignore]
        public bool PressEnter {
            get {
                return Enter == 1;
            }
            set {
                Enter = value == true ? 1 : 0;
            }
        }

        [Ignore]
        public bool IsSilent {
            get {
                return Silent == 1;
            }
            set {
                Silent = value == true ? 1 : 0;
            }
        }

        [Ignore]
        public bool IsAdmin {
            get {
                return Admin == 1;
            }
            set {
                Admin = value == true ? 1 : 0;
            }
        }
        #endregion
        

        public MpPasteToAppPath(
            string appPath, 
            string appName,
            string iconStr = "",
            bool isAdmin = false, 
            bool isSilent = false, 
            string label = "", 
            string args = "", 
            int windowState = 1, 
            bool pressEnter = false) {
            AppPath = appPath;
            AppName = appName;
            IsAdmin = isAdmin;
            IsSilent = isSilent;
            Label = label;
            Args = args;
            AvatarDbImage = new MpDbImage() {
                DbImageGuid = System.Guid.NewGuid(),
                ImageBase64 = iconStr
            };
            WindowState = windowState;
            PressEnter = pressEnter;
        }
        public MpPasteToAppPath() : this(string.Empty,string.Empty) { }

        //public MpPasteToAppPath(DataRow dr) {
        //    LoadDataRow(dr);
        //}
        //public void LoadDataRow(DataRow dr) {
        //    PasteToAppPathId = Convert.ToInt32(dr["pk_MpPasteToAppPathId"].ToString());
        //    AppPath = dr["AppPath"].ToString();
        //    AppName = dr["AppName"].ToString();
        //    IsAdmin = Convert.ToInt32(dr["IsAdmin"].ToString()) > 0 ? true : false;
        //    IsSilent = Convert.ToInt32(dr["IsSilent"].ToString()) > 0 ? true : false;
        //    PressEnter = Convert.ToInt32(dr["PressEnter"].ToString()) > 0 ? true : false;
        //    WindowState = Convert.ToInt32(dr["WindowState"].ToString());

        //    AvatarId = Convert.ToInt32(dr["fk_MpDbImageId"].ToString());
        //    AvatarDbImage = MpDbImage.GetDbImageById(AvatarId);
        //    Label = dr["Label"].ToString();
        //    Args = dr["Args"].ToString();
        //}

        //public void DeleteFromDatabase() {
        //    if (PasteToAppPathId <= 0) {
        //        return;
        //    }

        //    MpDb.ExecuteWrite(
        //        "delete from MpPasteToAppPath where pk_MpPasteToAppPathId=@cid",
        //        new Dictionary<string, object> {
        //            { "@cid", PasteToAppPathId }
        //        });
        //}

        //public void WriteToDatabase() {
        //    //AvatarDbImage.WriteToDatabase
        //    if (PasteToAppPathId == 0) {
        //        DataTable dt = MpDb.Execute(
        //            "select * from MpPasteToAppPath where AppPath=@ap and IsAdmin=@ia and IsSilent=@is and Args=@a",
        //            new System.Collections.Generic.Dictionary<string, object> {
        //                { "@ap", AppPath },
        //                { "@ia", IsAdmin ? 1:0 },
        //                { "@a", Args },
        //                { "@is", IsSilent ? 1:0 }
        //            });
        //        if (dt != null && dt.Rows.Count > 0) {
        //            PasteToAppPathId = Convert.ToInt32(dt.Rows[0]["pk_MpPasteToAppPathId"].ToString());
        //        } else {
        //            MpDb.ExecuteWrite(
        //                "insert into MpPasteToAppPath(MpPasteToAppPathGuid,AppPath,AppName,IsAdmin,Label,Args,IconBlob,IsSilent,WindowState,PressEnter) values(@apg,@ap,@an,@ia,@l,@a,@ib,@is,@ws,@pe)",
        //                new System.Collections.Generic.Dictionary<string, object> {
        //                    { "@apg", PasteToAppPathGuid.ToString() },
        //                { "@ap", AppPath },
        //                { "@an",AppName },
        //                { "@ia", IsAdmin ? 1:0 },
        //                { "@l", Label },
        //                { "@a", Args },
        //                //{ "@ib", MpHelpers.ConvertBitmapSourceToByteArray(Icon) },
        //                { "@is", IsSilent ? 1:0 },
        //                { "@ws", (int)WindowState },
        //                {"@pe", PressEnter ? 1:0 }
        //            });
        //            PasteToAppPathId = MpDb.GetLastRowId("MpPasteToAppPath", "pk_MpPasteToAppPathId");
        //        }
        //    } else {
        //        MpDb.ExecuteWrite(
        //            "update MpPasteToAppPath set AppPath=@ap, AppName=@an, IsAdmin=@ia, IsSilent=@is, Label=@l, Args=@a, IconBlob=@ib, WindowState=@ws, PressEnter=@pe where pk_MpPasteToAppPathId=@cid",
        //            new System.Collections.Generic.Dictionary<string, object> {
        //                { "@ap", AppPath },
        //                { "@an",AppName },
        //                { "@ia", IsAdmin ? 1:0 },
        //                { "@cid", PasteToAppPathId },
        //                { "@l", Label },
        //                { "@a", Args },
        //                //{ "@ib", MpHelpers.ConvertBitmapSourceToByteArray(Icon) },
        //                { "@is", IsSilent ? 1:0 },
        //                { "@ws", (int)WindowState },
        //                { "@pe", PressEnter ? 1:0 }
        //            });
        //    }
        //}
    }
}
