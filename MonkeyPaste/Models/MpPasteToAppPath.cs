using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SQLite;


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

        [Column("fk_MpIconId")]
        public int IconId { get; set; }

        public int WindowState { get; set; }

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
        
        public static async Task<MpPasteToAppPath> Create(
            string appPath = "",
            string appName = "",
            string iconStr = "",
            bool isAdmin = false,
            bool isSilent = false,
            string label = "",
            string args = "",
            int windowState = 1,
            bool pressEnter = false) {

            var icon = await MpIcon.CreateAsync(
                iconImgBase64: iconStr, 
                createBorder: false);

            var pasteToAppPath = new MpPasteToAppPath() {
                PasteToAppPathGuid = System.Guid.NewGuid(),
                IconId = icon.Id,
                AppPath = appPath,
                AppName = appName,
                IsAdmin = isAdmin,
                IsSilent = isSilent,
                Label = label,
                Args = args,
                WindowState = windowState,
                PressEnter = pressEnter
            };
            await pasteToAppPath.WriteToDatabaseAsync();
            return pasteToAppPath;
        }        
    }
}
