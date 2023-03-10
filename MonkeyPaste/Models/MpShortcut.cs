using MonkeyPaste.Common;
using SQLite;

using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MonkeyPaste {

    public class MpShortcut : MpDbModelBase {
        public const int MIN_USER_SHORTCUT_TYPE = 101;

        #region Columns
        [PrimaryKey, AutoIncrement]
        [Column("pk_MpShortcutId")]
        public override int Id { get; set; } = 0;

        [Column("MpShortcutGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        //[Column("fk_MpCommandId")]
        public string CommandParameter { get; set; } = null;

        public string ShortcutLabel { get; set; } = string.Empty;
        public string KeyString { get; set; } = string.Empty;
        public string DefaultKeyString { get; set; } = string.Empty;

        //[Column("RoutingType")]
        public string RouteTypeName { get; set; }

        //[Column("e_ShortcutTypeId")]
        public string ShortcutTypeName { get; set; }

        public int RoutingDelayMs { get; set; } = 100;

        [Column("b_IsReadOnly")]
        public int IsReadOnlyVal { get; set; } = 0;

        #endregion

        #region Fk Models

        #endregion

        #region Properties

        [Ignore]
        public Guid ShortcutGuid {
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

        //[Ignore]
        //public int ShortcutId {
        //    get {
        //        return Id;
        //    }
        //    set {
        //        Id = value;
        //    }
        //}

        [Ignore]
        public bool IsReadOnly {
            get => IsReadOnlyVal == 1;
            set => IsReadOnlyVal = value ? 1 : 0;
        }

        [Ignore]
        public MpShortcutType ShortcutType {
            get {
                return ShortcutTypeName.ToEnum<MpShortcutType>();
            }
            set {
                ShortcutTypeName = value.ToString();
            }
        }

        [Ignore]
        public MpRoutingType RoutingType {
            get {
                return RouteTypeName.ToEnum<MpRoutingType>();
            }
            set {
                RouteTypeName = value.ToString();
            }
        }


        #endregion

        #region Static Methods

        public static async Task<MpShortcut> CreateAsync(
            string shortcutLabel = "",
            string keyString = "",
            string defKeyString = "",
            MpRoutingType routeType = MpRoutingType.Bubble,
            MpShortcutType shortcutType = MpShortcutType.None,
            string commandParameter = null,
            string guid = "",
            bool isReadOnly = false) {
            if (shortcutType == MpShortcutType.None) {
                throw new Exception("Needs type");
            }
            if (string.IsNullOrEmpty(keyString)) {
                throw new Exception("Needs keystring");
            }

            shortcutLabel = string.IsNullOrEmpty(shortcutLabel) ? shortcutType.EnumToLabel() : shortcutLabel;
            guid = string.IsNullOrEmpty(guid) ? System.Guid.NewGuid().ToString() : guid;
            defKeyString = string.IsNullOrEmpty(defKeyString) ? keyString : defKeyString;

            var dupShortcut = await MpDataModelProvider.GetShortcutAsync(shortcutType.ToString(), commandParameter);
            if (dupShortcut != null) {
                MpConsole.WriteLine($"Shortcut '{dupShortcut.ShortcutLabel}' already exists.");
                MpConsole.WriteLine($"Updating name from '{dupShortcut.ShortcutLabel}' to '{shortcutLabel}'");
                MpConsole.WriteLine($"Updating keyString from '{dupShortcut.KeyString}' to '{keyString}'");
                MpConsole.WriteLine($"Updating routing type from '{dupShortcut.RoutingType}' to '{routeType}'");

                dupShortcut = await MpDataModelProvider.GetItemAsync<MpShortcut>(dupShortcut.Id);
                dupShortcut.ShortcutLabel = shortcutLabel;
                dupShortcut.KeyString = keyString;
                dupShortcut.RoutingType = routeType;

                await dupShortcut.WriteToDatabaseAsync();
                return dupShortcut;
            }
            var newShortcut = new MpShortcut() {
                ShortcutGuid = System.Guid.Parse(guid),
                ShortcutLabel = shortcutLabel,
                KeyString = keyString,
                DefaultKeyString = defKeyString,
                RoutingType = routeType,
                ShortcutType = shortcutType,
                CommandParameter = commandParameter,
                IsReadOnly = isReadOnly
            };

            await newShortcut.WriteToDatabaseAsync();

            return newShortcut;
        }

        public static bool IsUserDefinedShortcut(MpShortcutType stype) {
            return (int)stype > (int)MpShortcutType.MAX_APP_SHORTCUT;
        }

        #endregion

        #region Public Methods
        public MpShortcut() { }


        public override string ToString() {
            return $"Shortcut Id:{Id} Name: '{ShortcutLabel}' Gesture: '{KeyString}'";
        }

        public override async Task WriteToDatabaseAsync() {
            if (IsReadOnly && Id > 0) {
                // Should be caught before here
                Debugger.Break();
                return;
            }
            await base.WriteToDatabaseAsync();
        }

        public override async Task DeleteFromDatabaseAsync() {
            //if (IsReadOnly) {
            //    // Should be caught before here
            //    Debugger.Break();
            //    return;
            //}
            await base.DeleteFromDatabaseAsync();
        }
        #endregion

    }

}
